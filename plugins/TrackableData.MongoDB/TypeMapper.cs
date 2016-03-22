using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization;

namespace TrackableData.MongoDB
{
    public static class TypeMapper
    {
        public static bool RegisterMap(Type type)
        {
            if (typeof(ITrackablePoco).IsAssignableFrom(type) && type.IsInterface == false)
                return RegisterTrackablePocoMap(type);
            else
                return RegisterClassMap(type);
        }

        public static bool RegisterTrackablePocoMap(Type trackablePocoType)
        {
            if (BsonClassMap.IsClassMapRegistered(trackablePocoType))
                return false;

            var classMap = new BsonClassMap(trackablePocoType);
            var pocoType = TrackableResolver.GetPocoType(trackablePocoType);

            // start with auto map
            classMap.AutoMap();

            // ignore extra elements for smooth schema change
            classMap.SetIgnoreExtraElements(true);

            // unmap all members which T doesn't have
            var propertyNames = new HashSet<string>(pocoType.GetProperties().Select(p => p.Name));
            var deletingMembers = classMap.DeclaredMemberMaps.Where(m =>
            {
                var propertyInfo = m.MemberInfo as PropertyInfo;
                return propertyInfo == null ||
                       propertyNames.Contains(propertyInfo.Name) == false;
            }).ToList();
            foreach (var m in deletingMembers)
                classMap.UnmapMember(m.MemberInfo);

            // set default ignore for saving spaces
            foreach (var memberMap in classMap.DeclaredMemberMaps)
            {
                var mt = memberMap.MemberType;
                var defaultValue = mt.IsValueType ? Activator.CreateInstance(mt) : null;
                memberMap.SetDefaultValue(defaultValue);
                memberMap.SetIgnoreIfDefault(true);
            }

            // tell customized id to mongo-db
            var identityColumn = pocoType.GetProperties().FirstOrDefault(
                p => TrackablePropertyAttribute.GetParameter(p, "mongodb.identity") != null);
            if (identityColumn != null)
            {
                classMap.MapIdProperty(identityColumn.Name);
            }

            try
            {
                BsonClassMap.RegisterClassMap(classMap);
            }
            catch (ArgumentException)
            {
                // if duplicate key exists
                return false;
            }

            return true;
        }

        public static bool RegisterClassMap(Type classType)
        {
            if (BsonClassMap.IsClassMapRegistered(classType))
                return false;

            var classMap = new BsonClassMap(classType);

            // start with auto map
            classMap.AutoMap();

            // ignore extra elements for smooth schema change
            classMap.SetIgnoreExtraElements(true);

            // unmap all members which has mongodb.ignore attribute
            var deletingMembers = classMap
                .DeclaredMemberMaps
                .Where(m => { return TrackablePropertyAttribute.GetParameter(m.MemberInfo, "mongodb.ignore") != null; })
                .ToList();
            foreach (var m in deletingMembers)
                classMap.UnmapMember(m.MemberInfo);

            // set default ignore for saving spaces
            foreach (var memberMap in classMap.DeclaredMemberMaps)
            {
                var mt = memberMap.MemberType;
                var defaultValue = mt.IsValueType ? Activator.CreateInstance(mt) : null;
                memberMap.SetDefaultValue(defaultValue);
                memberMap.SetIgnoreIfDefault(true);
            }

            try
            {
                BsonClassMap.RegisterClassMap(classMap);
            }
            catch (ArgumentException)
            {
                // if duplicate key exists
                return false;
            }

            return true;
        }
    }
}

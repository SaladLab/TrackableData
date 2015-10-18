using System;

namespace TrackableData.TestKits
{
    public static class TypeUtility
    {
        public static T CreateFrom<T>(object src)
        {
            var srcType = src.GetType();
            var objType = typeof(T);
            var obj = Activator.CreateInstance(objType);
            foreach (var property in objType.GetProperties())
            {
                var srcProperty = srcType.GetProperty(property.Name);
                if (srcProperty != null)
                {
                    var propertyValue = srcProperty.GetValue(src);
                    property.SetValue(obj, propertyValue);
                }
            }
            return (T)obj;
        }
    }
}

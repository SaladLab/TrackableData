using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ProtoBuf;

namespace CodeGen
{
    class TrackablePocoCodeGenerator
    {
        public Options Options { get; set; }

        public void GenerateCode(Type type, ICodeGenWriter writer)
        {
            Console.WriteLine("GenerateCode: " + type.FullName);

            writer.PushRegion(type.FullName);
            writer.PushNamespace(type.Namespace);

            GenerateTrackablePocoCode(type, writer); 
            if (Options.UseProtobuf)
                GenerateTrackablePocoSurrogateCode(type, writer);

            writer.PopNamespace();
            writer.PopRegion();
        }

        private PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties()
                .Where(p => p.GetMethod.IsVirtual)
                .OrderBy(p => p.Name).ToArray();
        }

        private PropertyInfo[] GetTrackableProperties(Type type)
        {
            return type.GetProperties()
                .Where(p => p.GetMethod.IsVirtual && Utility.IsTrackable(p.PropertyType))
                .OrderBy(p => p.Name).ToArray();
        }

        private void GenerateTrackablePocoCode(Type type, ICodeGenWriter writer)
        {
            var sb = new StringBuilder();
            var className = "Trackable" + type.Name.Substring(1);

            if (Options.UseProtobuf)
                sb.AppendLine("[ProtoContract]");
            sb.AppendLine($"public class {className} : {type.Name}, ITrackable<{type.Name}>");
            sb.AppendLine("{");

            // Tracker

            sb.AppendLine("\t[IgnoreDataMember]");
            sb.AppendFormat("\tpublic TrackablePocoTracker<{0}> Tracker {{ get; set; }}\n", type.Name);
            sb.AppendLine("");

            // ITrackable.Changed

            sb.AppendLine("\tpublic bool Changed { get { return Tracker != null && Tracker.HasChange; } }");
            sb.AppendLine("");

            // ITrackable.Tracker

            sb.AppendLine("\tITracker ITrackable.Tracker");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tget");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn Tracker;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tset");
            sb.AppendLine("\t\t{");
            sb.AppendFormat("\t\t\tvar t = (TrackablePocoTracker<{0}>)value;\n", type.Name);
            sb.AppendLine("\t\t\tTracker = t;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("");

            // ITrackable<T>.Tracker

            sb.AppendLine($"\tITracker<{type.Name}> ITrackable<{type.Name}>.Tracker");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tget");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn Tracker;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tset");
            sb.AppendLine("\t\t{");
            sb.AppendFormat("\t\t\tvar t = (TrackablePocoTracker<{0}>)value;\n", type.Name);
            sb.AppendLine("\t\t\tTracker = t;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("");

            // ITrackable.GetChildTrackable

            var childTrackableProperties = GetTrackableProperties(type);

            sb.AppendLine("\tpublic ITrackable GetChildTrackable(object name)");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tswitch ((string)name)");
            sb.AppendLine("\t\t{");
            foreach (var ctp in childTrackableProperties)
            {
                sb.AppendFormat("\t\t\tcase \"{0}\":\n", ctp.Name);
                sb.AppendFormat("\t\t\t\treturn {0} as ITrackable;\n", ctp.Name);
            }
            sb.AppendLine("\t\t\tdefault:");
            sb.AppendLine("\t\t\t\treturn null;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("");

            // ITrackable.GetChildTrackables

            sb.AppendLine("\tpublic IEnumerable<KeyValuePair<object, ITrackable>> GetChildTrackables(bool changedOnly = false)");
            sb.AppendLine("\t{");
            if (childTrackableProperties.Any())
            {
                foreach (var ctp in childTrackableProperties)
                {
                    sb.AppendFormat("\t\tvar trackable{0} = {0} as ITrackable;\n", ctp.Name, ctp.PropertyType.Name);
                    sb.AppendFormat("\t\tif (trackable{0} != null && (changedOnly == false || trackable{0}.Changed))\n", ctp.Name);
                    sb.AppendFormat("\t\t\tyield return new KeyValuePair<object, ITrackable>(\"{0}\", trackable{0});\n", ctp.Name);
                }
            }
            else
            {
                sb.AppendLine("\t\tyield break;");
            }
            sb.AppendLine("\t}");

            // Property Table

            sb.AppendLine("");
            foreach (var p in GetProperties(type))
            {
                sb.Append($"\tpublic static readonly PropertyInfo {p.Name}Property = " +
                          $"typeof({type.Name}).GetProperty(\"{p.Name}\");\n");
            }

            // Property Accessors

            foreach (var p in GetProperties(type))
            {
                sb.AppendLine("");

                var propertyType = Utility.GetTypeFullName(p.PropertyType);
                sb.AppendLine($"\tprivate {propertyType} _{p.Name};");
                sb.AppendLine("");

                if (Options.UseProtobuf)
                {
                    var protoMemberAttr = p.GetCustomAttribute<ProtoMemberAttribute>();
                    if (protoMemberAttr != null)
                        sb.Append($"\t[ProtoMember({protoMemberAttr.Tag})] ");
                    else
                        sb.Append($"\t");
                }
                else
                {
                    sb.Append($"\t");
                }

                sb.AppendLine($"public {propertyType} {p.Name}");
                sb.AppendLine("\t{");
                sb.AppendLine("\t\tget");
                sb.AppendLine("\t\t{");
                sb.AppendLine($"\t\t\treturn _{p.Name};");
                sb.AppendLine("\t\t}");
                sb.AppendLine("\t\tset");
                sb.AppendLine("\t\t{");
                sb.AppendLine($"\t\t\tif (Tracker != null && {p.Name} != value)");
                sb.AppendLine($"\t\t\t\tTracker.TrackSet({p.Name}Property, _{p.Name}, value);");
                sb.AppendLine($"\t\t\t_{p.Name} = value;");
                sb.AppendLine("\t\t}");
                sb.AppendLine("\t}");
            }

            sb.Append("}");

            writer.AddCode(sb.ToString());
        }

        private void GenerateTrackablePocoSurrogateCode(Type type, ICodeGenWriter writer)
        {
            var sb = new StringBuilder();
            var trackableClassName = "Trackable" + type.Name.Substring(1);
            var className = trackableClassName + "TrackerSurrogate";

            sb.AppendLine("[ProtoContract]");
            sb.AppendLine($"public class {className}");
            sb.AppendLine("{");

            // Assign unique id to properties

            var properties = GetProperties(type);
            var propertyIds = new Dictionary<PropertyInfo, int>();
            foreach (var p in properties)
            {
                var protoMemberAttr = p.GetCustomAttribute<ProtoMemberAttribute>();
                if (protoMemberAttr != null)
                    propertyIds[p] = protoMemberAttr.Tag;
            }

            // ProtoMember

            foreach (var item in propertyIds.OrderBy(x => x.Value))
            {
                var p = item.Key;
                sb.AppendFormat($"\t[ProtoMember({item.Value})] ");

                if (p.PropertyType.IsValueType)
                    sb.AppendFormat("public {0}? {1};\n", p.PropertyType, p.Name);
                else
                    sb.AppendFormat("public EnvelopedObject<{0}> {1};\n", p.PropertyType, p.Name);
            }

            // ConvertTrackerToSurrogate

            sb.AppendLine("");
            sb.AppendLine($"\tpublic static implicit operator {className}(TrackablePocoTracker<{type.Name}> tracker)");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tif (tracker == null)");
            sb.AppendLine("\t\t\treturn null;");
            sb.AppendLine("");
            sb.AppendLine($"\t\tvar surrogate = new {className}();");
            sb.AppendLine("\t\tforeach(var changeItem in tracker.ChangeMap)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tvar tag = changeItem.Key.GetCustomAttribute<ProtoMemberAttribute>().Tag;");
            sb.AppendLine("\t\t\tswitch (tag)");
            sb.AppendLine("\t\t\t{");
            foreach (var item in propertyIds.OrderBy(x => x.Value))
            {
                var p = item.Key;
                var typeName = Utility.GetTypeFullName(p.PropertyType);

                sb.AppendLine($"\t\t\t\tcase {item.Value}:");
                if (p.PropertyType.IsValueType)
                    sb.AppendLine($"\t\t\t\t\tsurrogate.{p.Name} = ({typeName})changeItem.Value.NewValue;");
                else
                    sb.AppendLine($"\t\t\t\t\tsurrogate.{p.Name} = new EnvelopedObject<{typeName}> {{ Value = ({typeName})changeItem.Value.NewValue }};");

                sb.AppendLine($"\t\t\t\t\tbreak;");
            }
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\treturn surrogate;");
            sb.AppendLine("\t}");

            // ConvertSurrogateToTracker

            sb.AppendLine("");
            sb.AppendLine($"\tpublic static implicit operator TrackablePocoTracker<{type.Name}>({className} surrogate)");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tif (surrogate == null)");
            sb.AppendLine("\t\t\treturn null;");
            sb.AppendLine("");
            sb.AppendLine($"\t\tvar tracker = new TrackablePocoTracker<{type.Name}>();");
            foreach (var item in propertyIds.OrderBy(x => x.Value))
            {
                var p = item.Key;
                var typeName = Utility.GetTypeFullName(p.PropertyType);

                sb.AppendLine($"\t\tif (surrogate.{p.Name} != null)");
                sb.Append($"\t\t\ttracker.ChangeMap.Add({trackableClassName}.{p.Name}Property, " +
                          $"new TrackablePocoTracker<{type.Name}>.Change {{ NewValue = surrogate.{p.Name}.Value }});\n");
            }
            sb.AppendLine("\t\treturn tracker;");
            sb.AppendLine("\t}");

            sb.Append("}");

            writer.AddCode(sb.ToString());
        }
    }
}

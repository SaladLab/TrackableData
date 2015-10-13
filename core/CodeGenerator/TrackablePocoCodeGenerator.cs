using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
            var className = "Trackable" + type.Name;

            if (Options.UseProtobuf)
                sb.Append("[ProtoContract]\n");
            sb.Append($"public class {className} : {type.Name}, ITrackable<{type.Name}>\n");
            sb.Append("{\n");

            // Tracker

            sb.Append("\t[IgnoreDataMember]\n");
            sb.AppendFormat("\tpublic TrackablePocoTracker<{0}> Tracker {{ get; set; }}\n", type.Name);
            sb.Append("\n");

            // ITrackable.Changed

            sb.Append("\tpublic bool Changed { get { return Tracker != null && Tracker.HasChange; } }\n");
            sb.Append("\n");

            // ITrackable.Tracker

            sb.Append("\tITracker ITrackable.Tracker\n");
            sb.Append("\t{\n");
            sb.Append("\t\tget\n");
            sb.Append("\t\t{\n");
            sb.Append("\t\t\treturn Tracker;\n");
            sb.Append("\t\t}\n");
            sb.Append("\t\tset\n");
            sb.Append("\t\t{\n");
            sb.AppendFormat("\t\t\tvar t = (TrackablePocoTracker<{0}>)value;\n", type.Name);
            sb.Append("\t\t\tTracker = t;\n");
            sb.Append("\t\t}\n");
            sb.Append("\t}\n");
            sb.Append("\n");

            // ITrackable<T>.Tracker

            sb.Append($"\tITracker<{type.Name}> ITrackable<{type.Name}>.Tracker\n");
            sb.Append("\t{\n");
            sb.Append("\t\tget\n");
            sb.Append("\t\t{\n");
            sb.Append("\t\t\treturn Tracker;\n");
            sb.Append("\t\t}\n");
            sb.Append("\t\tset\n");
            sb.Append("\t\t{\n");
            sb.AppendFormat("\t\t\tvar t = (TrackablePocoTracker<{0}>)value;\n", type.Name);
            sb.Append("\t\t\tTracker = t;\n");
            sb.Append("\t\t}\n");
            sb.Append("\t}\n");
            sb.Append("\n");

            // ITrackable.GetChildTrackable

            var childTrackableProperties = GetTrackableProperties(type);

            sb.Append("\tpublic ITrackable GetChildTrackable(object name)\n");
            sb.Append("\t{\n");
            sb.Append("\t\tswitch ((string)name)\n");
            sb.Append("\t\t{\n");
            foreach (var ctp in childTrackableProperties)
            {
                sb.AppendFormat("\t\t\tcase \"{0}\":\n", ctp.Name);
                sb.AppendFormat("\t\t\t\treturn {0} as ITrackable;\n", ctp.Name);
            }
            sb.Append("\t\t\tdefault:\n");
            sb.Append("\t\t\t\treturn null;\n");
            sb.Append("\t\t}\n");
            sb.Append("\t}\n");
            sb.Append("\n");

            // ITrackable.GetChildTrackables

            sb.Append("\tpublic IEnumerable<KeyValuePair<object, ITrackable>> GetChildTrackables(bool changedOnly = false)\n");
            sb.Append("\t{\n");
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
                sb.Append("\t\tyield break;\n");
            }
            sb.Append("\t}\n");

            // Properties

            foreach (var p in GetProperties(type))
            {
                sb.AppendLine("");
                sb.AppendFormat(
                    "\tpublic static readonly PropertyInfo {0}Property = typeof(Trackable{1}).GetProperty(\"{0}\");\n",
                    p.Name, type.Name);
                if (Options.UseProtobuf)
                {
                    var protoMemberAttr = p.GetCustomAttribute<ProtoMemberAttribute>();
                    if (protoMemberAttr != null)
                        sb.Append($"\t[ProtoMember({protoMemberAttr.Tag})]\n");
                }

                sb.AppendFormat("\tpublic override {0} {1}\n", p.PropertyType, p.Name);
                sb.AppendLine("\t{");
                sb.AppendFormat("\t\tget\n");
                sb.AppendLine("\t\t{");
                sb.AppendFormat("\t\t\treturn base.{0}; \n", p.Name);
                sb.AppendLine("\t\t}");
                sb.AppendFormat("\t\tset\n");
                sb.AppendLine("\t\t{");
                sb.AppendFormat("\t\t\tif (Tracker != null && {0} != value) \n", p.Name);
                sb.AppendFormat("\t\t\t\tTracker.TrackSet({0}Property, base.{0}, value); \n", p.Name);
                sb.AppendFormat("\t\t\tbase.{0} = value; \n", p.Name);
                sb.AppendLine("\t\t}");
                sb.AppendLine("\t}");
            }

            sb.Append("}");

            writer.AddCode(sb.ToString());
        }

        private void GenerateTrackablePocoSurrogateCode(Type type, ICodeGenWriter writer)
        {
            var sb = new StringBuilder();
            var className = "Trackable" + type.Name + "TrackerSurrogate";

            sb.Append("[ProtoContract]\n");
            sb.Append($"public class {className}\n");
            sb.Append("{\n");

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

            sb.Append("\n");
            sb.Append($"\tpublic static implicit operator {className}(TrackablePocoTracker<{type.Name}> tracker)\n");
            sb.Append("\t{\n");
            sb.Append("\t\tif (tracker == null)\n");
            sb.Append("\t\t\treturn null;\n");
            sb.Append("\n");
            sb.Append($"\t\tvar surrogate = new {className}();\n");
            sb.Append("\t\tforeach(var changeItem in tracker.ChangeMap)\n");
            sb.Append("\t\t{\n");
            sb.Append("\t\t\tvar tag = changeItem.Key.GetCustomAttribute<ProtoMemberAttribute>().Tag;\n");
            sb.Append("\t\t\tswitch (tag)\n");
            sb.Append("\t\t\t{\n");
            foreach (var item in propertyIds.OrderBy(x => x.Value))
            {
                var p = item.Key;
                var typeName = Utility.GetTypeName(p.PropertyType);

                sb.Append($"\t\t\t\tcase {item.Value}:\n");
                if (p.PropertyType.IsValueType)
                    sb.Append($"\t\t\t\t\tsurrogate.{p.Name} = ({typeName})changeItem.Value.NewValue;\n");
                else
                    sb.Append($"\t\t\t\t\tsurrogate.{p.Name} = new EnvelopedObject<{typeName}> {{ Value = ({typeName})changeItem.Value.NewValue }};\n");

                sb.Append($"\t\t\t\t\tbreak;\n");
            }
            sb.Append("\t\t\t}\n");
            sb.Append("\t\t}\n");
            sb.Append("\t\treturn surrogate;\n");
            sb.Append("\t}\n");

            // ConvertSurrogateToTracker

            sb.Append("\n");
            sb.Append($"\tpublic static implicit operator TrackablePocoTracker<{type.Name}>({className} surrogate)\n");
            sb.Append("\t{\n");
            sb.Append("\t\tif (surrogate == null)\n");
            sb.Append("\t\t\treturn null;\n");
            sb.Append("\n");
            sb.Append($"\t\tvar tracker = new TrackablePocoTracker<{type.Name}>();\n");
            foreach (var item in propertyIds.OrderBy(x => x.Value))
            {
                var p = item.Key;
                var typeName = Utility.GetTypeName(p.PropertyType);

                sb.Append($"\t\tif (surrogate.{p.Name} != null)\n");
                sb.Append($"\t\t\ttracker.ChangeMap.Add(Trackable{type.Name}.{p.Name}Property, " +
                          $"new TrackablePocoTracker<{type.Name}>.Change {{ NewValue = surrogate.{p.Name}.Value }});\n");
            }
            sb.Append("\t\treturn tracker;\n");
            sb.Append("\t}\n");

            sb.Append("}");

            writer.AddCode(sb.ToString());
        }
    }
}

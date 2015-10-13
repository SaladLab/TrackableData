using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ProtoBuf;

namespace CodeGen
{
    class TrackableContainerCodeGenerator
    {
        public Options Options { get; set; }

        public void GenerateCode(Type type, ICodeGenWriter writer)
        {
            Console.WriteLine("GenerateCode: " + type.FullName);

            writer.PushRegion(type.FullName);
            writer.PushNamespace(type.Namespace);

            GenerateTrackableContainerCode(type, writer);
            GenerateTrackableContainerTrackerCode(type, writer);

            writer.PopNamespace();
            writer.PopRegion();
        }

        private PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties()
                .Where(p => p.GetMethod.IsVirtual)
                .OrderBy(p => p.Name).ToArray();
        }

        private void GenerateTrackableContainerCode(Type type, ICodeGenWriter writer)
        {
            var sb = new StringBuilder();
            var className = "Trackable" + type.Name.Substring(1);

            if (Options.UseProtobuf)
                sb.AppendLine("[ProtoContract]");
            sb.AppendLine($"public class {className} : {type.Name}, ITrackable<{type.Name}>");
            sb.AppendLine("{");

            var childTrackableProperties = GetProperties(type);

            // Tracker

            sb.AppendLine("\t[IgnoreDataMember]");
            sb.AppendLine($"\tprivate {className}Tracker _tracker;");
            sb.AppendLine("");

            sb.Append("\t[IgnoreDataMember]\n");
            sb.Append($"\tpublic {className}Tracker Tracker\n");
            sb.Append("\t{\n");
            sb.Append("\t\tget\n");
            sb.Append("\t\t{\n");
            sb.Append("\t\t\treturn _tracker;\n");
            sb.Append("\t\t}\n");
            sb.Append("\t\tset\n");
            sb.Append("\t\t{\n");
            sb.Append("\t\t\t_tracker = value;\n");
            foreach (var ctp in childTrackableProperties)
            {
                sb.Append($"\t\t\t((ITrackable){ctp.Name}).Tracker = value?.{ctp.Name}Tracker;\n");
            }
            sb.Append("\t\t}\n");
            sb.Append("\t}\n");
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
            sb.Append($"\t\t\tvar t = ({className}Tracker)value;\n");
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
            sb.Append($"\t\t\tvar t = ({className}Tracker)value;\n");
            sb.Append("\t\t\tTracker = t;\n");
            sb.Append("\t\t}\n");
            sb.Append("\t}\n");
            sb.Append("\n");

            // ITrackable.GetChildTrackable

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

            // Property Accessors

            foreach (var p in childTrackableProperties)
            {
                sb.Append("\n");

                var propertyType = Utility.GetTypeFullName(p.PropertyType);
                var propertyTrackableName = Utility.GetTrackableClassName(p.PropertyType);
                sb.AppendLine($"\tprivate {propertyTrackableName} _{p.Name};");
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

                sb.AppendLine($"public {propertyTrackableName} {p.Name}");
                sb.AppendLine("\t{");
                sb.AppendLine("\t\tget");
                sb.AppendLine("\t\t{");
                sb.AppendLine($"\t\t\treturn _{p.Name};");
                sb.AppendLine("\t\t}");
                sb.AppendLine("\t\tset");
                sb.AppendLine("\t\t{");
                sb.AppendLine($"\t\t\tif (_{p.Name} != null)");
                sb.AppendLine($"\t\t\t\t_{p.Name}.Tracker = null;");
                sb.AppendLine("\t\t\tif (value != null)");
                sb.AppendLine($"\t\t\t\tvalue.Tracker = Tracker?.{p.Name}Tracker;");
                sb.AppendLine($"\t\t\t_{p.Name} = value;");
                sb.AppendLine("\t\t}");
                sb.AppendLine("\t}");

                sb.AppendLine("");
                sb.AppendLine($"\t{propertyType} {type.Name}.{p.Name}");
                sb.AppendLine("\t{");
                sb.AppendLine($"\t\tget {{ return _{p.Name}; }}");
                sb.AppendLine($"\t\tset {{ _{p.Name} = ({propertyTrackableName})value; }}");
                sb.AppendLine("\t}");
            }

            sb.Append("}");

            writer.AddCode(sb.ToString());
        }

        private void GenerateTrackableContainerTrackerCode(Type type, ICodeGenWriter writer)
        {
            var sb = new StringBuilder();
            var className = "Trackable" + type.Name.Substring(1) + "Tracker";

            if (Options.UseProtobuf)
                sb.Append("[ProtoContract]\n");
            sb.AppendLine($"public class {className} : ITracker<{type.Name}>, ITrackableContainerTracker");
            sb.AppendLine("{");

            // Property Trackers

            var childTrackableProperties = GetProperties(type);
            foreach (var ctp in childTrackableProperties)
            {
                if (Options.UseProtobuf)
                {
                    var protoMemberAttr = ctp.GetCustomAttribute<ProtoMemberAttribute>();
                    var tag = protoMemberAttr.Tag;
                    sb.Append($"\t[ProtoMember({tag})] ");
                }
                else
                {
                    sb.Append("\t");
                }

                var trackerName = Utility.GetTrackerClassName(ctp.PropertyType);
                sb.AppendLine($"public {trackerName} {ctp.Name}Tracker = new {trackerName}();");
            }

            // ITracker.HasChange

            sb.AppendLine("");
            sb.AppendLine("\tpublic bool HasChange");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tget");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn");
            foreach (var ctp in childTrackableProperties)
            {
                sb.AppendLine($"\t\t\t\t{ctp.Name}Tracker.HasChange ||");
            }
            sb.AppendLine("\t\t\t\tfalse;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");

            // ITracker.Clear

            sb.AppendLine("");
            sb.AppendLine("\tpublic void Clear()");
            sb.AppendLine("\t{");
            foreach (var ctp in childTrackableProperties)
            {
                sb.AppendLine($"\t\t{ctp.Name}Tracker.Clear();");
            }
            sb.AppendLine("\t}");

            // ITracker.ApplyTo(Trackable)

            sb.AppendLine("");
            sb.AppendLine("\tpublic void ApplyTo(object trackable)");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tApplyTo(({type.Name})trackable);");
            sb.AppendLine("\t}");

            sb.AppendLine("");
            sb.AppendLine($"\tpublic void ApplyTo({type.Name} trackable)");
            sb.AppendLine("\t{");
            foreach (var ctp in childTrackableProperties)
            {
                sb.AppendLine($"\t\t{ctp.Name}Tracker.ApplyTo(trackable.{ctp.Name});");
            }
            sb.AppendLine("\t}");

            // ITracker.ApplyTo(Tracker)

            sb.AppendLine("");
            sb.AppendLine("\tpublic void ApplyTo(ITracker tracker)");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tApplyTo(({className})tracker);");
            sb.AppendLine("\t}");

            sb.AppendLine("");
            sb.AppendLine($"\tpublic void ApplyTo(ITracker<{type.Name}> tracker)");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tApplyTo(({className})tracker);");
            sb.AppendLine("\t}");

            sb.AppendLine("");
            sb.AppendLine($"\tpublic void ApplyTo({className} tracker)");
            sb.AppendLine("\t{");
            foreach (var ctp in childTrackableProperties)
            {
                sb.AppendLine($"\t\t{ctp.Name}Tracker.ApplyTo(tracker.{ctp.Name}Tracker);");
            }
            sb.AppendLine("\t}");

            // RollbackTo.RollbackTo(Trackable)

            sb.AppendLine("");
            sb.AppendLine("\tpublic void RollbackTo(object trackable)");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tRollbackTo(({type.Name})trackable);");
            sb.AppendLine("\t}");

            sb.AppendLine("");
            sb.AppendLine($"\tpublic void RollbackTo({type.Name} trackable)");
            sb.AppendLine("\t{");
            foreach (var ctp in childTrackableProperties)
            {
                sb.AppendLine($"\t\t{ctp.Name}Tracker.RollbackTo(trackable.{ctp.Name});");
            }
            sb.AppendLine("\t}");

            // RollbackTo.RollbackTo(Tracker)

            sb.AppendLine("");
            sb.AppendLine("\tpublic void RollbackTo(ITracker tracker)");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tRollbackTo(({className})tracker);");
            sb.AppendLine("\t}");

            sb.AppendLine("");
            sb.AppendLine($"\tpublic void RollbackTo(ITracker<{type.Name}> tracker)");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tRollbackTo(({className})tracker);");
            sb.AppendLine("\t}");

            sb.AppendLine("");
            sb.AppendLine($"\tpublic void RollbackTo({className} tracker)");
            sb.AppendLine("\t{");
            foreach (var ctp in childTrackableProperties)
            {
                sb.AppendLine($"\t\t{ctp.Name}Tracker.RollbackTo(tracker.{ctp.Name}Tracker);");
            }
            sb.AppendLine("\t}");

            sb.Append("}");

            writer.AddCode(sb.ToString());
        }
    }
}

using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGen
{
    internal class TrackableContainerCodeGenerator
    {
        public Options Options { get; set; }

        public void GenerateCode(InterfaceDeclarationSyntax idecl, ICodeGenWriter writer)
        {
            var iname = idecl.Identifier.ToString();
            Console.WriteLine("GenerateCode: " + iname);

            writer.PushRegion(iname);
            writer.PushNamespace(idecl.GetNamespaceScope());

            GenerateTrackableContainerCode(idecl, writer);
            GenerateTrackableContainerTrackerCode(idecl, writer);

            writer.PopNamespace();
            writer.PopRegion();
        }

        private void GenerateTrackableContainerCode(InterfaceDeclarationSyntax idecl, ICodeGenWriter writer)
        {
            var sb = new StringBuilder();
            var typeName = idecl.GetTypeName();
            var className = "Trackable" + typeName.Substring(1);

            var useProtoContract = idecl.AttributeLists.GetAttribute("ProtoContractAttribute") != null;
            if (useProtoContract)
                sb.AppendLine("[ProtoContract]");
            sb.AppendLine($"public partial class {className} : {typeName}");
            sb.AppendLine("{");

            var properties = idecl.GetProperties();

            // Tracker

            sb.AppendLine("\t[IgnoreDataMember]");
            sb.AppendLine($"\tprivate {className}Tracker _tracker;");
            sb.AppendLine("");

            sb.AppendLine("\t[IgnoreDataMember]");
            sb.AppendLine($"\tpublic {className}Tracker Tracker");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tget");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn _tracker;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tset");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\t_tracker = value;");
            foreach (var p in properties)
            {
                var propertyName = p.Identifier.ToString();
                sb.AppendLine($"\t\t\t{propertyName}.Tracker = value?.{propertyName}Tracker;");
            }
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
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
            sb.AppendLine($"\t\t\tvar t = ({className}Tracker)value;");
            sb.AppendLine("\t\t\tTracker = t;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("");

            // ITrackable<T>.Tracker

            sb.AppendLine($"\tITracker<{typeName}> ITrackable<{typeName}>.Tracker");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tget");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn Tracker;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tset");
            sb.AppendLine("\t\t{");
            sb.AppendLine($"\t\t\tvar t = ({className}Tracker)value;");
            sb.AppendLine("\t\t\tTracker = t;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("");

            // IContainerTracker<T>.Tracker

            sb.AppendLine($"\tIContainerTracker<{typeName}> ITrackableContainer<{typeName}>.Tracker");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tget");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn Tracker;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tset");
            sb.AppendLine("\t\t{");
            sb.AppendLine($"\t\t\tvar t = ({className}Tracker)value;");
            sb.AppendLine("\t\t\tTracker = t;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("");

            // ITrackable.GetChildTrackable

            sb.AppendLine("\tpublic ITrackable GetChildTrackable(object name)");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tswitch ((string)name)");
            sb.AppendLine("\t\t{");
            foreach (var p in properties)
            {
                var propertyName = p.Identifier.ToString();
                sb.AppendLine($"\t\t\tcase \"{propertyName}\":");
                sb.AppendLine($"\t\t\t\treturn {propertyName} as ITrackable;");
            }
            sb.AppendLine("\t\t\tdefault:");
            sb.AppendLine("\t\t\t\treturn null;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("");

            // ITrackable.GetChildTrackables

            sb.AppendLine(
                "\tpublic IEnumerable<KeyValuePair<object, ITrackable>> GetChildTrackables(bool changedOnly = false)");
            sb.AppendLine("\t{");
            if (properties.Any())
            {
                foreach (var p in properties)
                {
                    var propertyType = p.Type.ToString();
                    var propertyName = p.Identifier.ToString();
                    sb.AppendFormat("\t\tvar trackable{0} = {0} as ITrackable;\n", propertyName, propertyType);
                    sb.AppendFormat(
                        "\t\tif (trackable{0} != null && (changedOnly == false || trackable{0}.Changed))\n",
                        propertyName);
                    sb.AppendFormat(
                        "\t\t\tyield return new KeyValuePair<object, ITrackable>(\"{0}\", trackable{0});\n",
                        propertyName);
                }
            }
            else
            {
                sb.AppendLine("\t\tyield break;");
            }
            sb.AppendLine("\t}");

            // Property Accessors

            foreach (var p in properties)
            {
                var propertyType = p.Type.ToString();
                var propertyName = p.Identifier.ToString();

                sb.AppendLine("");
                sb.AppendLine($"\tprivate {propertyType} _{propertyName} = new {propertyType}();");
                sb.AppendLine("");

                var protoMemberAttr = p.AttributeLists.GetAttribute("ProtoMemberAttribute");
                if (protoMemberAttr != null)
                    sb.Append($"\t[ProtoMember{protoMemberAttr.ArgumentList}] ");
                else
                    sb.Append("\t");

                sb.AppendLine($"public {propertyType} {propertyName}");
                sb.AppendLine("\t{");
                sb.AppendLine("\t\tget");
                sb.AppendLine("\t\t{");
                sb.AppendLine($"\t\t\treturn _{propertyName};");
                sb.AppendLine("\t\t}");
                sb.AppendLine("\t\tset");
                sb.AppendLine("\t\t{");
                sb.AppendLine($"\t\t\tif (_{propertyName} != null)");
                sb.AppendLine($"\t\t\t\t_{propertyName}.Tracker = null;");
                sb.AppendLine("\t\t\tif (value != null)");
                sb.AppendLine($"\t\t\t\tvalue.Tracker = Tracker?.{propertyName}Tracker;");
                sb.AppendLine($"\t\t\t_{propertyName} = value;");
                sb.AppendLine("\t\t}");
                sb.AppendLine("\t}");

                sb.AppendLine("");
                sb.AppendLine($"\t{propertyType} {typeName}.{propertyName}");
                sb.AppendLine("\t{");
                sb.AppendLine($"\t\tget {{ return _{propertyName}; }}");
                sb.AppendLine($"\t\tset {{ _{propertyName} = ({propertyType})value; }}");
                sb.AppendLine("\t}");
            }

            sb.Append("}");

            writer.AddCode(sb.ToString());
        }

        private void GenerateTrackableContainerTrackerCode(InterfaceDeclarationSyntax idecl, ICodeGenWriter writer)
        {
            var sb = new StringBuilder();
            var typeName = idecl.GetTypeName();
            var className = "Trackable" + typeName.Substring(1) + "Tracker";

            var useProtoContract = idecl.AttributeLists.GetAttribute("ProtoContractAttribute") != null;
            if (useProtoContract)
                sb.AppendLine("[ProtoContract]");

            sb.AppendLine($"public class {className} : IContainerTracker<{typeName}>");
            sb.AppendLine("{");

            // Property Trackers

            var properties = idecl.GetProperties();
            foreach (var p in properties)
            {
                var protoMemberAttr = p.AttributeLists.GetAttribute("ProtoMemberAttribute");
                if (protoMemberAttr != null)
                    sb.Append($"\t[ProtoMember{protoMemberAttr.ArgumentList}] ");
                else
                    sb.Append("\t");

                var propertyName = p.Identifier.ToString();
                var trackerName = Utility.GetTrackerClassName(p.Type);
                sb.AppendLine($"public {trackerName} {propertyName}Tracker {{ get; set; }} = new {trackerName}();");
            }

            // ToString()

            sb.AppendLine("");
            sb.AppendLine("\tpublic override string ToString()");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tvar sb = new StringBuilder();");
            sb.AppendLine("\t\tsb.Append(\"{ \");");
            sb.AppendLine("\t\tvar first = true;");
            foreach (var p in properties)
            {
                var propertyName = p.Identifier.ToString();
                sb.AppendLine($"\t\tif ({propertyName}Tracker != null && {propertyName}Tracker.HasChange)");
                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\tif (first)");
                sb.AppendLine("\t\t\t\tfirst = false;");
                sb.AppendLine("\t\t\telse");
                sb.AppendLine("\t\t\t\tsb.Append(\", \");");
                sb.AppendLine($"\t\t\tsb.Append(\"{propertyName}:\");");
                sb.AppendLine($"\t\t\tsb.Append({propertyName}Tracker);");
                sb.AppendLine("\t\t}");
            }
            sb.AppendLine("\t\tsb.Append(\" }\");");
            sb.AppendLine("\t\treturn sb.ToString();");
            sb.AppendLine("\t}");

            // ITracker.HasChange

            sb.AppendLine("");
            sb.AppendLine("\tpublic bool HasChange");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tget");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn");
            foreach (var p in properties)
            {
                var propertyName = p.Identifier.ToString();
                sb.AppendLine($"\t\t\t\t({propertyName}Tracker != null && {propertyName}Tracker.HasChange) ||");
            }
            sb.AppendLine("\t\t\t\tfalse;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");

            // ITracker.Clear

            sb.AppendLine("");
            sb.AppendLine("\tpublic void Clear()");
            sb.AppendLine("\t{");
            foreach (var p in properties)
            {
                var propertyName = p.Identifier.ToString();
                sb.AppendLine($"\t\tif ({propertyName}Tracker != null)");
                sb.AppendLine($"\t\t\t{propertyName}Tracker.Clear();");
            }
            sb.AppendLine("\t}");

            // ITracker.ApplyTo(Trackable)

            sb.AppendLine("");
            sb.AppendLine("\tpublic void ApplyTo(object trackable)");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tApplyTo(({typeName})trackable);");
            sb.AppendLine("\t}");

            sb.AppendLine("");
            sb.AppendLine($"\tpublic void ApplyTo({typeName} trackable)");
            sb.AppendLine("\t{");
            foreach (var p in properties)
            {
                var propertyName = p.Identifier.ToString();
                sb.AppendLine($"\t\tif ({propertyName}Tracker != null)");
                sb.AppendLine($"\t\t\t{propertyName}Tracker.ApplyTo(trackable.{propertyName});");
            }
            sb.AppendLine("\t}");

            // ITracker.ApplyTo(Tracker)

            sb.AppendLine("");
            sb.AppendLine("\tpublic void ApplyTo(ITracker tracker)");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tApplyTo(({className})tracker);");
            sb.AppendLine("\t}");

            sb.AppendLine("");
            sb.AppendLine($"\tpublic void ApplyTo(ITracker<{typeName}> tracker)");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tApplyTo(({className})tracker);");
            sb.AppendLine("\t}");

            sb.AppendLine("");
            sb.AppendLine($"\tpublic void ApplyTo({className} tracker)");
            sb.AppendLine("\t{");
            foreach (var p in properties)
            {
                var propertyName = p.Identifier.ToString();
                sb.AppendLine($"\t\tif ({propertyName}Tracker != null)");
                sb.AppendLine($"\t\t\t{propertyName}Tracker.ApplyTo(tracker.{propertyName}Tracker);");
            }
            sb.AppendLine("\t}");

            // ITracker.RollbackTo(Trackable)

            sb.AppendLine("");
            sb.AppendLine("\tpublic void RollbackTo(object trackable)");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tRollbackTo(({typeName})trackable);");
            sb.AppendLine("\t}");

            sb.AppendLine("");
            sb.AppendLine($"\tpublic void RollbackTo({typeName} trackable)");
            sb.AppendLine("\t{");
            foreach (var p in properties)
            {
                var propertyName = p.Identifier.ToString();
                sb.AppendLine($"\t\tif ({propertyName}Tracker != null)");
                sb.AppendLine($"\t\t\t{propertyName}Tracker.RollbackTo(trackable.{propertyName});");
            }
            sb.AppendLine("\t}");

            // ITracker.RollbackTo(Tracker)

            sb.AppendLine("");
            sb.AppendLine("\tpublic void RollbackTo(ITracker tracker)");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tRollbackTo(({className})tracker);");
            sb.AppendLine("\t}");

            sb.AppendLine("");
            sb.AppendLine($"\tpublic void RollbackTo(ITracker<{typeName}> tracker)");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tRollbackTo(({className})tracker);");
            sb.AppendLine("\t}");

            sb.AppendLine("");
            sb.AppendLine($"\tpublic void RollbackTo({className} tracker)");
            sb.AppendLine("\t{");
            foreach (var p in properties)
            {
                var propertyName = p.Identifier.ToString();
                sb.AppendLine($"\t\tif ({propertyName}Tracker != null)");
                sb.AppendLine($"\t\t\t{propertyName}Tracker.RollbackTo(tracker.{propertyName}Tracker);");
            }
            sb.AppendLine("\t}");

            sb.Append("}");

            writer.AddCode(sb.ToString());
        }
    }
}

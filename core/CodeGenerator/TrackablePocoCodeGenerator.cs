using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGen
{
    internal class TrackablePocoCodeGenerator
    {
        public Options Options { get; set; }

        public void GenerateCode(InterfaceDeclarationSyntax idecl, ICodeGenWriter writer)
        {
            var iname = idecl.Identifier.ToString();
            Console.WriteLine("GenerateCode: " + iname);

            writer.PushRegion(iname);
            writer.PushNamespace(idecl.GetNamespaceScope());

            var useProtoContract = idecl.AttributeLists.GetAttribute("ProtoContractAttribute") != null;
            GenerateTrackablePocoCode(idecl, writer, useProtoContract);

            if (useProtoContract)
                GenerateTrackablePocoSurrogateCode(idecl, writer);

            writer.PopNamespace();
            writer.PopRegion();
        }

        private void GenerateTrackablePocoCode(InterfaceDeclarationSyntax idecl, ICodeGenWriter writer, bool useProtoContract)
        {
            var sb = new StringBuilder();
            var typeName = idecl.GetTypeName();
            var className = "Trackable" + typeName.Substring(1);

            var properties = idecl.GetProperties();
            var trackableProperties = Utility.GetTrackableProperties(properties);

            if (useProtoContract)
                sb.AppendLine("[ProtoContract]");
            sb.AppendLine($"public class {className} : {typeName}, ITrackable<{typeName}>");
            sb.AppendLine("{");

            // Tracker

            sb.AppendLine("\t[IgnoreDataMember]");
            sb.AppendFormat("\tpublic IPocoTracker<{0}> Tracker {{ get; set; }}\n", typeName);
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
            sb.AppendFormat("\t\t\tvar t = (IPocoTracker<{0}>)value;\n", typeName);
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
            sb.AppendFormat("\t\t\tvar t = (IPocoTracker<{0}>)value;\n", typeName);
            sb.AppendLine("\t\t\tTracker = t;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("");

            // ITrackable.GetChildTrackable

            sb.AppendLine("\tpublic ITrackable GetChildTrackable(object name)");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tswitch ((string)name)");
            sb.AppendLine("\t\t{");
            foreach (var p in trackableProperties)
            {
                sb.AppendLine($"\t\t\tcase \"{p.Identifier}\":");
                sb.AppendLine($"\t\t\t\treturn {p.Identifier} as ITrackable;");
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
            if (trackableProperties.Any())
            {
                foreach (var p in trackableProperties)
                {
                    sb.AppendFormat("\t\tvar trackable{0} = {0} as ITrackable;\n", p.Identifier);
                    sb.AppendFormat(
                        "\t\tif (trackable{0} != null && (changedOnly == false || trackable{0}.Changed))\n",
                        p.Identifier);
                    sb.AppendFormat(
                        "\t\t\tyield return new KeyValuePair<object, ITrackable>(\"{0}\", trackable{0});\n",
                        p.Identifier);
                }
            }
            else
            {
                sb.AppendLine("\t\tyield break;");
            }
            sb.AppendLine("\t}");

            // Property Table

            sb.AppendLine("");
            sb.AppendLine("\tpublic static class PropertyTable");
            sb.AppendLine("\t{");
            foreach (var p in properties)
            {
                sb.Append($"\t\tpublic static readonly PropertyInfo {p.Identifier} = " +
                          $"typeof({typeName}).GetProperty(\"{p.Identifier}\");\n");
            }
            sb.AppendLine("\t}");

            // Property Accessors

            foreach (var p in properties)
            {
                var propertyType = p.Type.ToString();
                var propertyName = p.Identifier.ToString();

                sb.AppendLine("");
                sb.AppendLine($"\tprivate {propertyType} _{p.Identifier};");
                sb.AppendLine("");

                var protoMemberAttr = p.AttributeLists.GetAttribute("ProtoMemberAttribute");
                if (protoMemberAttr != null)
                    sb.Append($"\t[ProtoMember{protoMemberAttr?.ArgumentList}] ");
                else
                    sb.Append($"\t");

                sb.AppendLine($"public {propertyType} {propertyName}");
                sb.AppendLine("\t{");
                sb.AppendLine("\t\tget");
                sb.AppendLine("\t\t{");
                sb.AppendLine($"\t\t\treturn _{propertyName};");
                sb.AppendLine("\t\t}");
                sb.AppendLine("\t\tset");
                sb.AppendLine("\t\t{");
                sb.AppendLine($"\t\t\tif (Tracker != null && {propertyName} != value)");
                sb.AppendLine($"\t\t\t\tTracker.TrackSet(PropertyTable.{propertyName}, _{propertyName}, value);");
                sb.AppendLine($"\t\t\t_{propertyName} = value;");
                sb.AppendLine("\t\t}");
                sb.AppendLine("\t}");
            }

            sb.Append("}");

            writer.AddCode(sb.ToString());
        }

        private void GenerateTrackablePocoSurrogateCode(InterfaceDeclarationSyntax idecl, ICodeGenWriter writer)
        {
            var sb = new StringBuilder();
            var typeName = idecl.GetTypeName();
            var trackableClassName = "Trackable" + typeName.Substring(1);
            var className = trackableClassName + "TrackerSurrogate";

            sb.AppendLine("[ProtoContract]");
            sb.AppendLine($"public class {className}");
            sb.AppendLine("{");

            // Collect properties with ProtoMemberAttribute attribute

            var propertyWithTags = idecl.GetProperties()
                                        .Select(
                                            p => Tuple.Create(p, p.AttributeLists.GetAttribute("ProtoMemberAttribute")))
                                        .Where(x => x.Item2 != null)
                                        .ToArray();

            // ProtoMember

            foreach (var item in propertyWithTags)
            {
                var p = item.Item1;
                sb.AppendFormat($"\t[ProtoMember{item.Item2.ArgumentList}] ");

                if (p.Type.IsValueType())
                    sb.AppendLine($"public {p.Type}? {p.Identifier};");
                else
                    sb.AppendLine($"public EnvelopedObject<{p.Type}> {p.Identifier};");
            }

            // ConvertTrackerToSurrogate

            sb.AppendLine("");
            sb.AppendLine($"\tpublic static implicit operator {className}(TrackablePocoTracker<{typeName}> tracker)");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tif (tracker == null)");
            sb.AppendLine("\t\t\treturn null;");
            sb.AppendLine("");
            sb.AppendLine($"\t\tvar surrogate = new {className}();");
            sb.AppendLine("\t\tforeach(var changeItem in tracker.ChangeMap)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tvar tag = changeItem.Key.GetCustomAttributes(false).OfType<ProtoMemberAttribute>().First().Tag;");
            sb.AppendLine("\t\t\tswitch (tag)");
            sb.AppendLine("\t\t\t{");
            foreach (var item in propertyWithTags)
            {
                var p = item.Item1;

                sb.AppendLine($"\t\t\t\tcase {item.Item2.ArgumentList.Arguments[0]}:");
                if (p.Type.IsValueType())
                    sb.AppendLine($"\t\t\t\t\tsurrogate.{p.Identifier} = ({p.Type})changeItem.Value.NewValue;");
                else
                    sb.AppendLine(
                        $"\t\t\t\t\tsurrogate.{p.Identifier} = new EnvelopedObject<{p.Type}> {{ Value = ({p.Type})changeItem.Value.NewValue }};");

                sb.AppendLine($"\t\t\t\t\tbreak;");
            }
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\treturn surrogate;");
            sb.AppendLine("\t}");

            // ConvertSurrogateToTracker

            sb.AppendLine("");
            sb.AppendLine($"\tpublic static implicit operator TrackablePocoTracker<{typeName}>({className} surrogate)");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tif (surrogate == null)");
            sb.AppendLine("\t\t\treturn null;");
            sb.AppendLine("");
            sb.AppendLine($"\t\tvar tracker = new TrackablePocoTracker<{typeName}>();");
            foreach (var item in propertyWithTags)
            {
                var p = item.Item1;

                sb.AppendLine($"\t\tif (surrogate.{p.Identifier} != null)");
                sb.Append($"\t\t\ttracker.ChangeMap.Add({trackableClassName}.PropertyTable.{p.Identifier}, " +
                          $"new TrackablePocoTracker<{typeName}>.Change {{ NewValue = surrogate.{p.Identifier}.Value }});\n");
            }
            sb.AppendLine("\t\treturn tracker;");
            sb.AppendLine("\t}");

            sb.Append("}");

            writer.AddCode(sb.ToString());
        }
    }
}

using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeWriter;

namespace CodeGen
{
    internal class TrackableContainerCodeGenerator
    {
        public Options Options { get; set; }

        public void GenerateCode(InterfaceDeclarationSyntax idecl, CodeWriter.CodeWriter w)
        {
            var iname = idecl.Identifier.ToString();
            Console.WriteLine("GenerateCode: " + iname);

            w._($"#region {iname}");
            w._();

            var namespaceScope = idecl.GetNamespaceScope();
            var namespaceHandle = (string.IsNullOrEmpty(namespaceScope) == false)
                ? w.B($"namespace {idecl.GetNamespaceScope()}")
                : null;

            GenerateTrackableContainerCode(idecl, w);
            GenerateTrackableContainerTrackerCode(idecl, w);

            namespaceHandle?.Dispose();

            w._();
            w._($"#endregion");
        }

        private void GenerateTrackableContainerCode(InterfaceDeclarationSyntax idecl, CodeWriter.CodeWriter w)
        {
            var typeName = idecl.GetTypeName();
            var className = "Trackable" + typeName.Substring(1);

            var useProtoContract = idecl.AttributeLists.GetAttribute("ProtoContractAttribute") != null;
            if (useProtoContract)
                w._($"[ProtoContract]");

            using (w.B($"public partial class {className} : {typeName}"))
            {
                var properties = idecl.GetProperties();

                // Tracker

                w._($"[IgnoreDataMember]",
                    $"private {className}Tracker _tracker;");
                w._();

                w._($"[IgnoreDataMember]");
                using (w.B($"public {className}Tracker Tracker"))
                {
                    using (w.b($"get"))
                    {
                        w._($"return _tracker;");
                    }
                    using (w.b($"set"))
                    {
                        w._($"_tracker = value;");
                        foreach (var p in properties)
                        {
                            var propertyName = p.Identifier.ToString();
                            w._($"{propertyName}.Tracker = value?.{propertyName}Tracker;");
                        }
                    }
                }

                // ITrackable.Changed

                w._("public bool Changed { get { return Tracker != null && Tracker.HasChange; } }");
                w._();

                // ITrackable.Tracker

                using (w.B($"ITracker ITrackable.Tracker"))
                {
                    using (w.b($"get"))
                    {
                        w._($"return Tracker;");
                    }
                    using (w.b($"set"))
                    {
                        w._($"var t = ({className}Tracker)value;");
                        w._($"Tracker = t;");
                    }
                }

                // ITrackable<T>.Tracker

                using (w.B($"ITracker<{typeName}> ITrackable<{typeName}>.Tracker"))
                {
                    using (w.b($"get"))
                    {
                        w._($"return Tracker;");
                    }
                    using (w.b($"set"))
                    {
                        w._($"var t = ({className}Tracker)value;");
                        w._($"Tracker = t;");
                    }
                }

                // IContainerTracker<T>.Tracker

                using (w.B($"IContainerTracker<{typeName}> ITrackableContainer<{typeName}>.Tracker"))
                {
                    using (w.b($"get"))
                    {
                        w._($"return Tracker;");
                    }
                    using (w.b($"set"))
                    {
                        w._($"var t = ({className}Tracker)value;");
                        w._($"Tracker = t;");
                    }
                }

                // ITrackable.GetChildTrackable

                using (w.B($"public ITrackable GetChildTrackable(object name)"))
                {
                    using (w.b($"switch ((string)name)"))
                    {
                        foreach (var p in properties)
                        {
                            var propertyName = p.Identifier.ToString();
                            w._($"case \"{propertyName}\":",
                                $"    return {propertyName} as ITrackable;");
                        }
                        w._($"default:",
                            $"    return null;");
                    }
                }

                // ITrackable.GetChildTrackables

                using (w.B($"public IEnumerable<KeyValuePair<object, ITrackable>> GetChildTrackables(bool changedOnly = false)"))
                {
                    if (properties.Any())
                    {
                        foreach (var p in properties)
                        {
                            var propertyType = p.Type.ToString();
                            var propertyName = p.Identifier.ToString();
                            w._($"var trackable{propertyName} = {propertyName} as ITrackable;",
                                $"if (trackable{propertyName} != null && (changedOnly == false || trackable{propertyName}.Changed))",
                                $"    yield return new KeyValuePair<object, ITrackable>(\"{propertyName}\", trackable{propertyName});");
                        }
                    }
                    else
                    {
                        w._($"yield break;");
                    }
                }

                // Property Accessors

                foreach (var p in properties)
                {
                    var propertyType = p.Type.ToString();
                    var propertyName = p.Identifier.ToString();

                    w._();
                    w._($"private {propertyType} _{propertyName} = new {propertyType}();");
                    w._();

                    var protoMemberAttr = p.AttributeLists.GetAttribute("ProtoMemberAttribute");
                    if (protoMemberAttr != null)
                        w._($"[ProtoMember{protoMemberAttr.ArgumentList}] ");

                    using (w.B($"public {propertyType} {propertyName}"))
                    {
                        using (w.b($"get"))
                        {
                            w._($"return _{propertyName};");
                        }
                        using (w.b($"set"))
                        {
                            w._($"if (_{propertyName} != null)",
                                $"    _{propertyName}.Tracker = null;",
                                $"if (value != null)",
                                $"    value.Tracker = Tracker?.{propertyName}Tracker;",
                                $"_{propertyName} = value;");
                        }
                    }

                    using (w.B($"{propertyType} {typeName}.{propertyName}"))
                    {
                        w._($"get {{ return _{propertyName}; }}",
                            $"set {{ _{propertyName} = ({propertyType})value; }}");
                    }
                }
            }
        }

        private void GenerateTrackableContainerTrackerCode(InterfaceDeclarationSyntax idecl, CodeWriter.CodeWriter w)
        {
            var typeName = idecl.GetTypeName();
            var className = "Trackable" + typeName.Substring(1) + "Tracker";

            var useProtoContract = idecl.AttributeLists.GetAttribute("ProtoContractAttribute") != null;
            if (useProtoContract)
                w._($"[ProtoContract]");

            using (w.B($"public class {className} : IContainerTracker<{typeName}>"))
            {
                // Property Trackers

                var properties = idecl.GetProperties();
                foreach (var p in properties)
                {
                    var protoMemberAttr = p.AttributeLists.GetAttribute("ProtoMemberAttribute");
                    if (protoMemberAttr != null)
                        w._($"[ProtoMember{protoMemberAttr.ArgumentList}] ");

                    var propertyName = p.Identifier.ToString();
                    var trackerName = Utility.GetTrackerClassName(p.Type);
                    w._($"public {trackerName} {propertyName}Tracker {{ get; set; }} = new {trackerName}();");
                }
                w._();

                // ToString()

                using (w.B($"public override string ToString()"))
                {
                    w._("var sb = new StringBuilder();",
                        "sb.Append(\"{ \");",
                        "var first = true;");
                    foreach (var p in properties)
                    {
                        var propertyName = p.Identifier.ToString();
                        using (w.B($"if ({propertyName}Tracker != null && {propertyName}Tracker.HasChange)"))
                        {
                            w._($"if (first)",
                                $"    first = false;",
                                $"else",
                                $"    sb.Append(\", \");",
                                $"sb.Append(`{propertyName}:`);",
                                $"sb.Append({propertyName}Tracker);");
                        }
                    }
                    w._("sb.Append(\" }\");",
                        "return sb.ToString();");
                }

                // ITracker.HasChange

                using (w.B($"public bool HasChange"))
                {
                    using (w.b($"get"))
                    {
                        w._($"return");
                        foreach (var p in properties)
                        {
                            var propertyName = p.Identifier.ToString();
                            w._($"    ({propertyName}Tracker != null && {propertyName}Tracker.HasChange) ||");
                        }
                        w._($"    false;");
                    }
                }

                // ITracker.HasChange

                using (w.B($"public event TrackerHasChangeSet HasChangeSet"))
                {
                    w._("add { throw new NotImplementedException(); }",
                        "remove { throw new NotImplementedException(); }");
                }

                // ITracker.Clear

                using (w.B($"public void Clear()"))
                {
                    foreach (var p in properties)
                    {
                        var propertyName = p.Identifier.ToString();
                        w._($"if ({propertyName}Tracker != null)",
                            $"    {propertyName}Tracker.Clear();");
                    }
                }

                // ITracker.ApplyTo(Trackable)

                using (w.B($"public void ApplyTo(object trackable)"))
                {
                    w._($"ApplyTo(({typeName})trackable);");
                }

                using (w.B($"public void ApplyTo({typeName} trackable)"))
                {
                    foreach (var p in properties)
                    {
                        var propertyName = p.Identifier.ToString();
                        w._($"if ({propertyName}Tracker != null)",
                            $"    {propertyName}Tracker.ApplyTo(trackable.{propertyName});");
                    }
                }

                // ITracker.ApplyTo(Tracker)

                using (w.B($"public void ApplyTo(ITracker tracker)"))
                {
                    w._($"ApplyTo(({className})tracker);");
                }

                using (w.B($"public void ApplyTo(ITracker<{typeName}> tracker)"))
                {
                    w._($"ApplyTo(({className})tracker);");
                }

                using (w.B($"public void ApplyTo({className} tracker)"))
                {
                    foreach (var p in properties)
                    {
                        var propertyName = p.Identifier.ToString();
                        w._($"if ({propertyName}Tracker != null)",
                            $"    {propertyName}Tracker.ApplyTo(tracker.{propertyName}Tracker);");
                    }
                }

                // ITracker.RollbackTo(Trackable)

                using (w.B($"public void RollbackTo(object trackable)"))
                {
                    w._($"RollbackTo(({typeName})trackable);");
                }

                using (w.B($"public void RollbackTo({typeName} trackable)"))
                {
                    foreach (var p in properties)
                    {
                        var propertyName = p.Identifier.ToString();
                        w._($"if ({propertyName}Tracker != null)",
                            $"    {propertyName}Tracker.RollbackTo(trackable.{propertyName});");
                    }
                }

                // ITracker.RollbackTo(Tracker)

                using (w.B($"public void RollbackTo(ITracker tracker)"))
                {
                    w._($"RollbackTo(({className})tracker);");
                }

                using (w.B($"public void RollbackTo(ITracker<{typeName}> tracker)"))
                {
                    w._($"RollbackTo(({className})tracker);");
                }

                using (w.B($"public void RollbackTo({className} tracker)"))
                {
                    foreach (var p in properties)
                    {
                        var propertyName = p.Identifier.ToString();
                        w._($"if ({propertyName}Tracker != null)",
                            $"    {propertyName}Tracker.RollbackTo(tracker.{propertyName}Tracker);");
                    }
                }
            }
        }
    }
}

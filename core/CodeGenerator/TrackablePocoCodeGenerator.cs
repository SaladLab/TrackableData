using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeWriter;

namespace CodeGen
{
    internal class TrackablePocoCodeGenerator
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

            var useProtoContract = idecl.AttributeLists.GetAttribute("ProtoContractAttribute") != null;
            GenerateTrackablePocoCode(idecl, w, useProtoContract);

            if (useProtoContract)
                GenerateTrackablePocoSurrogateCode(idecl, w);

            namespaceHandle?.Dispose();

            w._();
            w._($"#endregion");
        }

        private void GenerateTrackablePocoCode(InterfaceDeclarationSyntax idecl, CodeWriter.CodeWriter w, bool useProtoContract)
        {
            var typeName = idecl.GetTypeName();
            var className = "Trackable" + typeName.Substring(1);

            var properties = idecl.GetProperties();
            var trackableProperties = Utility.GetTrackableProperties(properties);

            if (useProtoContract)
                w._($"[ProtoContract]");

            using (w.B($"public partial class {className} : {typeName}"))
            {
                // Tracker

                w._($"[IgnoreDataMember]",
                    $"public IPocoTracker<{typeName}> Tracker {{ get; set; }}");
                w._();

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
                        w._($"var t = (IPocoTracker<{typeName}>)value;",
                            $"Tracker = t;");
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
                        w._($"var t = (IPocoTracker<{typeName}>)value;",
                            $"Tracker = t;");
                    }
                }

                // ITrackable.GetChildTrackable

                using (w.B($"public ITrackable GetChildTrackable(object name)"))
                {
                    using (w.B($"switch ((string)name)"))
                    {
                        foreach (var p in trackableProperties)
                        {
                            w._($"case \"{p.Identifier}\":",
                                $"    return {p.Identifier} as ITrackable;");
                        }
                        w._($"default:",
                            $"    return null;");
                    }
                }

                // ITrackable.GetChildTrackables

                using (w.B($"public IEnumerable<KeyValuePair<object, ITrackable>> GetChildTrackables(bool changedOnly = false)"))
                {
                    if (trackableProperties.Any())
                    {
                        foreach (var p in trackableProperties)
                        {
                            var id = p.Identifier;
                            w._($"var trackable{id} = {id} as ITrackable;",
                                $"if (trackable{id} != null && (changedOnly == false || trackable{id}.Changed))",
                                $"    yield return new KeyValuePair<object, ITrackable>(`{id}`, trackable{id});");
                        }
                    }
                    else
                    {
                        w._($"yield break;");
                    }
                }

                // Property Table

                using (w.B($"public static class PropertyTable"))
                {
                    foreach (var p in properties)
                    {
                        w._($"public static readonly PropertyInfo {p.Identifier} = " +
                            $"typeof({typeName}).GetProperty(\"{p.Identifier}\");");
                    }
                }

                // Property Accessors

                foreach (var p in properties)
                {
                    var propertyType = p.Type.ToString();
                    var propertyName = p.Identifier.ToString();

                    w._();
                    w._($"private {propertyType} _{p.Identifier};");
                    w._();

                    var protoMemberAttr = p.AttributeLists.GetAttribute("ProtoMemberAttribute");
                    if (protoMemberAttr != null)
                        w._($"[ProtoMember{protoMemberAttr?.ArgumentList}] ");

                    using (w.B($"public {propertyType} {propertyName}"))
                    {
                        using (w.b($"get"))
                        {
                            w._($"return _{propertyName};");
                        }
                        using (w.b($"set"))
                        {
                            w._($"if (Tracker != null && {propertyName} != value)",
                                $"    Tracker.TrackSet(PropertyTable.{propertyName}, _{propertyName}, value);",
                                $"_{propertyName} = value;");
                        }
                    }
                }
            }
        }

        private void GenerateTrackablePocoSurrogateCode(InterfaceDeclarationSyntax idecl, CodeWriter.CodeWriter w)
        {
            var sb = new StringBuilder();
            var typeName = idecl.GetTypeName();
            var trackableClassName = "Trackable" + typeName.Substring(1);
            var className = trackableClassName + "TrackerSurrogate";

            w._($"[ProtoContract]");
            using (w.B($"public class {className}"))
            {
                // Collect properties with ProtoMemberAttribute attribute

                var propertyWithTags =
                    idecl.GetProperties()
                         .Select(p => Tuple.Create(p, p.AttributeLists.GetAttribute("ProtoMemberAttribute")))
                         .Where(x => x.Item2 != null)
                         .ToArray();

                // ProtoMember

                foreach (var item in propertyWithTags)
                {
                    var p = item.Item1;
                    w._($"[ProtoMember{item.Item2.ArgumentList}] " +
                        $"public EnvelopedObject<{p.Type}> {p.Identifier};");
                }
                w._();

                // ConvertTrackerToSurrogate

                using (w.B($"public static implicit operator {className}(TrackablePocoTracker<{typeName}> tracker)"))
                {
                    w._($"if (tracker == null)",
                        $"    return null;");
                    w._();

                    w._($"var surrogate = new {className}();");
                    using (w.B($"foreach(var changeItem in tracker.ChangeMap)"))
                    {
                        using (w.B($"switch (changeItem.Key.Name)"))
                        {
                            foreach (var item in propertyWithTags)
                            {
                                var p = item.Item1;

                                w._($"case \"{item.Item1.Identifier}\":");
                                w._($"    surrogate.{p.Identifier} = new EnvelopedObject<{p.Type}>" +
                                    $" {{ Value = ({p.Type})changeItem.Value.NewValue }};");
                                w._($"    break;");
                            }
                        }
                    }
                    w._($"return surrogate;");
                }

                // ConvertSurrogateToTracker

                using (w.B($"public static implicit operator TrackablePocoTracker<{typeName}>({className} surrogate)"))
                {
                    w._($"if (surrogate == null)",
                        $"    return null;");
                    w._();
                    w._($"var tracker = new TrackablePocoTracker<{typeName}>();");
                    foreach (var item in propertyWithTags)
                    {
                        var p = item.Item1;

                        w._($"if (surrogate.{p.Identifier} != null)");
                        w._($"    tracker.ChangeMap.Add({trackableClassName}.PropertyTable.{p.Identifier}, " +
                            $"new TrackablePocoTracker<{typeName}>.Change {{ NewValue = surrogate.{p.Identifier}.Value }});");
                    }
                    w._($"return tracker;");
                }
            }
        }
    }
}

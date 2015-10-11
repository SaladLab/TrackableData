using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

            writer.PopNamespace();
            writer.PopRegion();
        }

        private void GenerateTrackablePocoCode(Type type, ICodeGenWriter writer)
        {
            var sb = new StringBuilder();
            var className = "Trackable" + type.Name;

            sb.Append($"public class {className} : {type.Name}, ITrackable<{type.Name}>\n");
            sb.Append("{\n");

            // Tracker

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

            // ITrackable.SetDefaultTracker

            sb.Append("\tpublic void SetDefaultTracker()\n");
            sb.Append("\t{\n");
            sb.AppendFormat("\t\tTracker = new TrackablePocoTracker<{0}>();\n", type.Name);
            sb.Append("\t}\n");
            sb.Append("\n");

            // ITrackable.ChildrenTrackables

            sb.Append("\tpublic IEnumerable<ITrackable> ChildrenTrackables\n");
            sb.Append("\t{\n");
            sb.Append("\t\tget\n");
            sb.Append("\t\t{\n");
            var childrenTrackablesCount = 0;
            foreach (var p in type.GetProperties().Where(p => p.GetMethod.IsVirtual))
            {
                if (Utility.IsTrackablePoco(p.PropertyType))
                {
                    sb.AppendFormat("\t\t\tvar trackable{0} = (Trackable{1}){0};\n", p.Name, p.PropertyType.Name);
                    sb.AppendFormat("\t\t\tif (trackable{0} != null)\n", p.Name);
                    sb.AppendFormat("\t\t\t\tyield return trackable{0};\n", p.Name);
                    childrenTrackablesCount += 1;
                }
            }
            if (childrenTrackablesCount == 0)
                sb.Append("\t\t\tyield break;\n");
            sb.Append("\t\t}\n");
            sb.Append("\t}\n");
            sb.Append("\n");

            // Properties

            foreach (var p in type.GetProperties().Where(p => p.GetMethod.IsVirtual))
            {
                sb.AppendLine("");
                sb.AppendFormat(
                    "\tprivate static readonly PropertyInfo {0}Property = typeof(Trackable{1}).GetProperty(\"{0}\");\n",
                    p.Name, type.Name);
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
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGen
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            // Parse command line options

            if (args.Length == 1 && args[0].StartsWith("@"))
            {
                var argFile = args[0].Substring(1);
                if (File.Exists(argFile))
                {
                    args = File.ReadAllLines(argFile);
                }
                else
                {
                    Console.WriteLine("File not found: " + argFile);
                    return 1;
                }
            }

            var parser = new Parser(config => config.HelpWriter = Console.Out);
            if (args.Length == 0)
            {
                parser.ParseArguments<Options>(new[] { "--help" });
                return 1;
            }

            Options options = null;
            var result = parser.ParseArguments<Options>(args)
                               .WithParsed(r => { options = r; });

            // Run process !

            if (options != null)
                return Process(options);
            else
                return 1;
        }

        private static int Process(Options options)
        {
            try
            {
                Console.WriteLine("Start Process!");

                // Resolve options

                var basePath = Path.GetFullPath(options.Path ?? ".");
                var sources =
                    options.Sources.Where(p => string.IsNullOrWhiteSpace(p) == false &&
                                               p.ToLower().IndexOf(".codegen.cs") == -1)
                           .Select(p => MakeFullPath(p, basePath))
                           .ToArray();
                var references =
                    options.References.Where(p => string.IsNullOrWhiteSpace(p) == false)
                           .Select(p => MakeFullPath(p, basePath))
                           .ToArray();
                var targetDefaultPath = @".\Properties\TrackableData.CodeGen.cs";
                var targetPath = MakeFullPath(options.TargetFile ?? targetDefaultPath, basePath);

                // Parse sources and extract interfaces

                Console.WriteLine("- Parse sources");

                var syntaxTrees =
                    sources.Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file), path: file)).ToArray();
                var interfaceDeclarations =
                    syntaxTrees.SelectMany(st => st.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>())
                               .ToArray();

                // Generate code

                Console.WriteLine("- Generate code");

                var writer = new TextCodeGenWriter();
                writer.AddUsing("System");
                writer.AddUsing("System.Collections.Generic");
                writer.AddUsing("System.Reflection");
                writer.AddUsing("System.Runtime.Serialization");
                writer.AddUsing("System.Linq");
                writer.AddUsing("TrackableData");
                if (options.UseProtobuf)
                {
                    writer.AddUsing("ProtoBuf");
                    writer.AddUsing("System.ComponentModel");
                }

                // TrackablePoco

                var pocoCodeGen = new TrackablePocoCodeGenerator() { Options = options };
                foreach (var idecl in interfaceDeclarations)
                {
                    if (idecl.HasBase("TrackableData.ITrackablePoco"))
                        pocoCodeGen.GenerateCode(idecl, writer);
                }

                // TrackableContainer

                var containerCodeGen = new TrackableContainerCodeGenerator() { Options = options };
                foreach (var idecl in interfaceDeclarations)
                {
                    if (idecl.HasBase("TrackableData.ITrackableContainer"))
                        containerCodeGen.GenerateCode(idecl, writer);
                }

                // Save generated code

                Console.WriteLine("- Save code");

                if (SaveFileIfChanged(targetPath, writer.ToString()) == false)
                    Console.WriteLine("Nothing changed. Skip writing.");

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in processing:\n" + e);
                return 1;
            }
        }

        private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine(ex);
                foreach (var e in ex.LoaderExceptions)
                    Console.WriteLine(e);
                return ex.Types.Where(x => x != null);
            }
        }

        private static string MakeFullPath(string path, string basePath)
        {
            if (Path.IsPathRooted(path))
                return path;
            else
                return Path.Combine(basePath, path);
        }

        private static bool SaveFileIfChanged(string path, string text)
        {
            if (File.Exists(path))
            {
                var existingText = File.ReadAllText(path);
                if (existingText == text)
                {
                    return false;
                }
            }
            File.WriteAllText(path, text);
            return true;
        }
    }
}

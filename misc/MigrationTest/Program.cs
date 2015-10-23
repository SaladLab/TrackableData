using System;
using CommandLine;

namespace MigrationTest
{
    internal class Options
    {
        [Option('p', "parallel", HelpText = "Parallel")]
        public bool Parallel { get; set; }

        [Value(0)]
        public string Job { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser(config => config.HelpWriter = Console.Out);
            if (args.Length == 0)
            {
                parser.ParseArguments<Options>(new[] { "--help" });
                return;
            }

            Options options = null;
            parser.ParseArguments<Options>(args)
                  .WithParsed(r => { options = r; });

            if (string.IsNullOrEmpty(options.Job))
            {
                Console.WriteLine("Job required!");
                return;
            }

            Console.WriteLine(options.Job);
            switch (options.Job.ToLower())
            {
                case "migrate":
                    RunMigration.MigrateAsync().Wait();
                    break;

                case "read":
                    RunBenchmark.ReadAsync(options.Parallel).Wait();
                    break;

                case "duplicate":
                    RunBenchmark.DuplicateAsync(options.Parallel).Wait();
                    break;

                case "replace":
                    RunBenchmark.ReplaceAsync(options.Parallel).Wait();
                    break;

                case "savesimple":
                    RunBenchmark.SaveSimpleAsync(options.Parallel).Wait();
                    break;

                case "savecomplex":
                    RunBenchmark.SaveComplexAsync(options.Parallel, false).Wait();
                    break;

                case "savecomplexfull":
                    RunBenchmark.SaveComplexAsync(options.Parallel, true).Wait();
                    break;

                default:
                    Console.WriteLine("Invalid job!");
                    break;
            }
        }
    }
}

using System;
using CommandLine;

namespace MigrationTest
{
    internal class Options
    {
        [Option('p', "parallel", HelpText = "Parallel")] public bool Parallel { get; set; }

        [Value(0)] public string Job { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
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
                case "sqlread":
                    SqlBenchmark.ReadAsync(options.Parallel).Wait();
                    break;

                case "migrate":
                    Sql2MongoMigrator.MigrateAsync().Wait();
                    break;

                case "read":
                    MongoDbBenchmark.ReadAsync(options.Parallel).Wait();
                    break;

                case "duplicate":
                    MongoDbBenchmark.DuplicateAsync(options.Parallel).Wait();
                    break;

                case "replace":
                    MongoDbBenchmark.ReplaceAsync(options.Parallel).Wait();
                    break;

                case "savesimple":
                    MongoDbBenchmark.SaveSimpleAsync(options.Parallel).Wait();
                    break;

                case "savecomplex":
                    MongoDbBenchmark.SaveComplexAsync(options.Parallel, false).Wait();
                    break;

                case "savecomplexfull":
                    MongoDbBenchmark.SaveComplexAsync(options.Parallel, true).Wait();
                    break;

                default:
                    Console.WriteLine("Invalid job!");
                    break;
            }
        }
    }
}

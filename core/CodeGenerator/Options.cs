﻿using System.Collections.Generic;
using CommandLine;

namespace CodeGen
{
    internal class Options
    {
        [Option('p', "path", HelpText = "Base path for processing sources, references and target.")]
        public string Path { get; set; }

        [Option('s', "source", Separator = ';', HelpText = "Input source files.")]
        public IEnumerable<string> Sources { get; set; }

        [Option('r', "reference", Separator = ';', HelpText = "Input reference files for building sources.")]
        public IEnumerable<string> References { get; set; }

        [Option('d', "define", Separator = ';', HelpText = "Defines name as a symbol which is used in compiling.")]
        public IEnumerable<string> Defines { get; set; }

        [Option('t', "target", HelpText = "Filename of a generated code.")]
        public string TargetFile { get; set; }
    }
}

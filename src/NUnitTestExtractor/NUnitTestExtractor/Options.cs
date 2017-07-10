using System.Collections;
using System.Collections.Generic;
using CommandLine;
namespace NUnitTestExtractor
{
    public sealed class Options
    {
        [Option('h', "help", HelpText = "Prints this help, to specify DLL files just list them out at the end with spaces inbetween", Required = false, DefaultValue = false)]
        public bool Help { get; set; }

        [Option('l', "level", HelpText = "Specifies the level at which the test is located", Required = true)]
        public string Level { get; set; }

        [Option('o', "output", MetaValue = "DIRECTORY", Required = false,
            HelpText = "The specifies the output file")]
        public string Output { get; set; }

        [ValueList(typeof(List<string>))]
        public IList<string> DLLs { get; set; }

    }
}

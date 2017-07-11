using System.Collections;
using System.Collections.Generic;
using CommandLine;
namespace NUnitTestExtractor
{
    public sealed class Options
    {
        [Option('h', "help", HelpText = "Prints this help, to specify DLL files just list them out at the end with spaces inbetween", Required = false, DefaultValue = false)]
        public bool Help { get; set; }

        [Option('l', "level", HelpText = "Specifies the level of granuality to use for the output tests. Valid values are 'namespace', 'class' or 'function'", Required = true)]
        public string Level { get; set; }

        [Option('o', "output", MetaValue = "DIRECTORY", Required = false,
            HelpText = "The specifies the output file. By default output goes to stdout.")]
        public string Output { get; set; }

        //Throws NullReferenceException if set is removed
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [ValueList(typeof(List<string>))]
        public IList<string> DLLs { get; set; }

    }
}

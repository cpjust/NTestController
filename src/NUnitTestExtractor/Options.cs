using System.Collections;
using System.Collections.Generic;
using CommandLine;
namespace NUnitTestExtractor
{
    public sealed class Options
    {
        [Option('h', "help", HelpText = "Prints this help, to specify DLL files just list them out at the end with spaces inbetween", Required = false, DefaultValue = false)]
        public bool Help { get; set; }

        [Option('l', "level", MetaValue = "LEVEL", HelpText = "Specifies the level of granuality to use for the output tests. Valid values are 'namespace', 'class', 'function' or 'testcase'", Required = true)]
        public string Level { get; set; }

        [Option('o', "output", MetaValue = "FILENAME", Required = false,
            HelpText = "The specifies the output txt file. By default output goes to stdout.")]
        public string Output { get; set; }

        [Option('i', "include", Required = false,
           HelpText = "This specifies which specific categories of tests that you want to write. Write info in double quotes and separate with a ,")]
        public string IncludeInfo { get; set; }

        [Option('e', "exclude", Required = false,
            HelpText = "This specifies which specific categories of tests that you want to exclude from writing. Write info in double quotes and separate with a ,")]
        public string ExcludeInfo { get; set; }

        //Throws NullReferenceException if set is removed
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [ValueList(typeof(List<string>))]
        public IList<string> DLLs { get; set; }
    }
}

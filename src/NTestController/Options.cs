using System;
using CommandLine;
using System.Collections.Generic;
using CommandLine.Text;

namespace NTestController
{
    /// <summary>
    /// Contains a list of all command line options, with their associated help text.
    /// </summary>
    public sealed class Options
    {
        // Required parameters:
        [Option('c', "config", MetaValue = "FILE", Required = true,
            HelpText = "The XML file that contains a list of target machines, plugin info & other config data.")]
        public string ConfigFile { get; set; }

        [Option('o', "output", MetaValue = "DIRECTORY", Required = true,
            HelpText = "The top-level directory where the XML test results are stored.")]
        public string Directory { get; set; }

        // Optional parameters:
        [Option('d', "dry-run", MetaValue = "true", DefaultValue = false,
            HelpText = "(optional) Don't run tests.  Just simulate what would normally happen.")]
        public bool? DryRun { get; set; }

        [Option('f', "file", MetaValue = "FILE",
            HelpText = "(optional) A CSV File containing a list of test names. (if unspecified, read from console)")]
        public string TestFile { get; set; }

        [Option('r', "retry", MetaValue = "INT", DefaultValue = 0,
            HelpText = "(optional) Number of times to retry failed tests.")]
        public int Retry { get; set; }

        [Option('R', "retry-threshold", MetaValue = "INT", DefaultValue = 0,
            HelpText = "(optional) The percentage of retried tests that should pass to allow another retry attempt.")]
        public int RetryThreshold { get; set; }

        [Option('v', "verbose", MetaValue = "STRING", DefaultValue = "Info",
            HelpText = "(optional) (Default: Info). The log level to use (i.e None, Error, Warning, Info, Debug, Trace).")]
        public string VerboseLevel { get; set; }


        [ParserState]
        public IParserState LastParserState { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}


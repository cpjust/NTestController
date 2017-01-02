using System;

namespace NTestController
{
    public interface IReadOnlyOptions
    {
        // Required parameters:

        /// <summary>
        /// The XML file that contains a list of target machines, plugin info & other config data.
        /// </summary>
        /// <value>The config file.</value>
        string ConfigFile { get; }

        /// <summary>
        /// The top-level directory where the XML test results are stored.
        /// </summary>
        /// <value>The output directory.</value>
        string OutputDirectory { get; }

        // Optional parameters:

        /// <summary>
        /// (optional) Don't run tests.  Just simulate what would normally happen.
        /// </summary>
        /// <value>True means don't run tests.</value>
        bool? DryRun { get; }

        /// <summary>
        /// (optional) A CSV File containing a list of test names. (if unspecified, read from console)
        /// </summary>
        /// <value>The test file.</value>
        string TestFile { get; }

        /// <summary>
        /// (optional) Number of times to retry failed tests.
        /// </summary>
        /// <value>The number of retries for failed tests.</value>
        int Retry { get; }

        /// <summary>
        /// (optional) The percentage of retried tests that should pass to allow another retry attempt.
        /// </summary>
        /// <value>The retry threshold.</value>
        int RetryThreshold { get; }

        /// <summary>
        /// (optional) (Default: Info). The log level to use (i.e None, Error, Warning, Info, Debug, Trace).
        /// </summary>
        /// <value>The verbose level.</value>
        string VerboseLevel { get; }
    }
}


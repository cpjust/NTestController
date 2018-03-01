using System;
using System.Collections.Generic;

namespace NTestController
{
    public interface IComputer
    {
        /// <summary>
        /// Gets the credentials needed for accessing this computer.
        /// This is only needed if the plugin needs access to this computer.
        /// </summary>
        /// <value>The credentials to access this computer.</value>
        Credentials Credentials { get; }

        /// <summary>
        /// Gets or sets the target hostname where tests will be run against.
        /// </summary>
        /// <value>The target hostname for tests.</value>
        string Hostname { get; set; }

        /// <summary>
        /// Gets or sets the test timeout (in seconds) before each test on this computer is cancelled.
        /// </summary>
        /// <value>The timeout in seconds.</value>
        int Timeout { get; set; }

        /// <summary>
        /// Gets a list of environment variables for the thread responsible for running tests on this computer.
        /// Ex. This might be necessary if your tests need a different config file for each computer...
        /// </summary>
        /// <value>The environment variables.</value>
        /// <remarks>Executor plugins should injext these variables into the thread created for this computer.</remarks>
        Dictionary<string, string> EnvironmentVariables { get; }

        /// <summary>
        /// Gets or sets the output path where test results for this computer are stored.
        /// </summary>
        /// <value>The output path for test results.</value>
        string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets the working directory for the thread responsible for running tests on this computer.
        /// </summary>
        /// <value>The working directory for this thread.</value>
        string WorkingDirectory { get; set; }
    }
}


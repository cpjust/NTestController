using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using Logger;
using NTestController;
using NTestController.Factories;
using NUnitReader;
using NUnitReader.Factories;
using Utilities;

namespace NUnitExecutor
{
    public class NUnitExecutorPlugin : IExecutorPlugin
    {
        private static ILogger Log { get; } = ConsoleLogger.Instance;

        private string NunitPath => ((NUnitComputer)Computer).NunitPath;
        private string _xmlConfig;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitExecutorPlugin"/> class.
        /// </summary>
        /// <param name="xmlConfig">NTestController.xml file path.</param>
        public NUnitExecutorPlugin(string xmlConfig)
        {
            ThrowIf.StringIsNullOrWhiteSpace(xmlConfig, nameof(xmlConfig));

            _xmlConfig = xmlConfig;
        }

        #endregion Constructors

        #region Inherited from IPlugin

        public string Name { get; private set; }
        public PluginType PluginType { get { return PluginType.TestExecutor; } }
        public IComputerFactory ComputerFactory { get { return new NUnitComputerFactory(); } }
        public IPlatformFactory PlatformFactory { get { return new NUnitPlatformFactory(); } }

        /// <seealso cref="IPlugin.Execute()"/>
        public bool Execute()
        {
            Log.WriteInfo("===== Starting tests on Computer: {0}", Computer.Hostname);

            var test = TestQueue.DequeueTestToRun() as NUnitTest;

            while (test != null)
            {
                RunTest(test);
                test = TestQueue.DequeueTestToRun() as NUnitTest;
            }

            Log.WriteInfo("===== Finished all tests on Computer: {0}", Computer.Hostname);

            return true;
        }

        #endregion Inherited from IPlugin

        #region Inherited from IExecutorPlugin

        public IReadOnlyOptions Options { get; set; }
        public IComputer Computer { get; set; }
        public TestQueue TestQueue { get; set; }

        /// <seealso cref="IExecutorPlugin.ClonePlugin()"/>
        public IExecutorPlugin ClonePlugin()
        {
            var newPlugin = new NUnitExecutorPlugin(_xmlConfig);
            newPlugin.Options = Options;

            newPlugin.Name = Name;
            newPlugin.Computer = Computer;
            newPlugin.TestQueue = TestQueue;

            return newPlugin;
        }

        #endregion Inherited from IExecutorPlugin

        private void RunTest(NUnitTest test)
        {
            // TODO: Run the test.
            string baseOutputFile = StringUtils.FormatInvariant(@"{0}\{1}", Options.OutputDirectory, test.TestName);

            string arguments = StringUtils.FormatInvariant(@"{0} /nologo /labels /run:{1} /out:{2}.txt /xml:{2}.xml /timeout:{3}",
                test.DllPath, test.TestName, baseOutputFile, Computer.Timeout * 1000);

            if (Computer.WorkingDirectory != null)
            {
                arguments = StringUtils.FormatInvariant("{0} /work:{1}", arguments, Computer.WorkingDirectory);
            }

            var processStartInfo = new ProcessStartInfo(NunitPath, arguments);
            processStartInfo.CreateNoWindow = true;
            processStartInfo.ErrorDialog = false;
            processStartInfo.UseShellExecute = false;

            if (Computer.WorkingDirectory != null)
            {
                processStartInfo.WorkingDirectory = Computer.WorkingDirectory;
            }

            foreach (var envKeyValue in Computer.EnvironmentVariables)
            {
                Log.WriteDebug("** On Computer '{0}', setting env var '{1}' = '{2}'", Computer.Hostname, envKeyValue.Key, envKeyValue.Value);
                processStartInfo.EnvironmentVariables.Add(envKeyValue.Key, envKeyValue.Value);
            }

            Log.WriteInfo("----- Running test: '{0}' on: {1}", test.TestName, Computer.Hostname);
            Log.WriteDebug("***** Running: {0} {1}", NunitPath, arguments);
            var process = Process.Start(processStartInfo);
            bool finished = process.WaitForExit((Computer.Timeout + 60) * 1000);

            if (!finished)
            {
                Log.WriteWarning("Timed out when running: '{0}' on {1}!", test.TestName, Computer.Hostname);
            }
            
            // TODO: Add result to CompletedTests list.
            var testResult = new TestResult();            
            string xmlOutputFile = StringUtils.FormatInvariant("{0}.xml", baseOutputFile);

            if (File.Exists(xmlOutputFile))
            {
                ParseNunitResults(xmlOutputFile, testResult);
            }
            else
            {
                testResult.Result = TestResult.ExitResult.Error;
            }

            test.TestRuns.Add(testResult);
            TestQueue.AddCompletedTest(test);

            // Print test output to stdout.
            string outputFile = StringUtils.FormatInvariant("{0}.txt", baseOutputFile);

            if (File.Exists(outputFile))
            {
                string outputContents = File.ReadAllText(outputFile);
                Log.WriteDebug(outputContents);
            }

            Log.WriteInfo("----- Test: '{0}'  Status = {1} on {2}", test.TestName, testResult.Result, Computer.Hostname);
        }

        private static void ParseNunitResults(string xmlOutputFile, TestResult testResult)
        {
            // TODO: Implement this properly by creating a single result for each individual test.

            var xmlDoc = XmlUtils.LoadXmlDocument(xmlOutputFile);

            XmlNode testResultsNode = XmlUtils.FindFirstChildByName(xmlDoc, "test-results");

//            int totalTests = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "total"), CultureInfo.InvariantCulture);
            int errors = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "errors"), CultureInfo.InvariantCulture);
            int failures = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "failures"), CultureInfo.InvariantCulture);
//            int notRun = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "not-run"), CultureInfo.InvariantCulture);
//            int inconclusive = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "inconclusive"), CultureInfo.InvariantCulture);
//            int ignored = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "ignored"), CultureInfo.InvariantCulture);
//            int skipped = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "skipped"), CultureInfo.InvariantCulture);
//            int invalid = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "invalid"), CultureInfo.InvariantCulture);
            TimeSpan totalTime = TimeSpan.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "time"), CultureInfo.InvariantCulture);

            testResult.ExecutionTime = totalTime.TotalMilliseconds;
            testResult.Result = (errors + failures) == 0 ? TestResult.ExitResult.Pass : TestResult.ExitResult.Fail;
        }
    }
}


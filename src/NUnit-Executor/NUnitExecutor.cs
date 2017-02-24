using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using NTestController;
using NUnitReader;
using Utilities;

namespace NUnitExecutor
{
    public class NUnitExecutorPlugin : IExecutorPlugin
    {
        private string NunitPath => Computer.NunitPath;
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

        #region Inherited from IExecutorPlugin

        public string Name { get; private set; }

        public PluginType PluginType
        {
            get { return PluginType.TestExecutor; }
        }

        public IReadOnlyOptions Options { get; set; }
        public IComputer Computer { get; set; }
        public TestQueue TestQueue { get; set; }

        /// <seealso cref="IPlugin.Execute()"/>
        public bool Execute()
        {
            var test = TestQueue.DequeueTestToRun() as NUnitTest;

            while (test != null)
            {
                RunTest(test);
                test = TestQueue.DequeueTestToRun() as NUnitTest;
            }

            return true;
        }

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

            string arguments = StringUtils.FormatInvariant(@"{0} /nologo /run:{1} /out:{2}.txt /xml:{2}.xml /timeout:{3}",
                test.DllPath, test.TestName, baseOutputFile, Computer.Timeout * 1000);

            if (Computer.WorkingDirectory != null)
            {
                arguments = StringUtils.FormatInvariant("{0} /work:{1}", arguments, Computer.WorkingDirectory);
            }

            var processStartInfo = new ProcessStartInfo(NunitPath, arguments);
            processStartInfo.CreateNoWindow = true;
            processStartInfo.ErrorDialog = false;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;

            if (Computer.WorkingDirectory != null)
            {
                processStartInfo.WorkingDirectory = Computer.WorkingDirectory;
            }

            foreach (var envKeyValue in Computer.EnvironmentVariables)
            {
                processStartInfo.EnvironmentVariables.Add(envKeyValue.Key, envKeyValue.Value);
            }

            var process = Process.Start(processStartInfo);
            bool finished = process.WaitForExit((Computer.Timeout + 60) * 1000);

            // TODO: Add result to CompletedTests list.
            var testResult = new TestResult()
            {
                Error = process.StandardError.ReadToEnd(),
                Output = process.StandardOutput.ReadToEnd()
            };

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
        }

        private void ParseNunitResults(string xmlOutputFile, TestResult testResult)
        {
            // TODO: Implement this properly by creating a single result for each individual test.

            var xmlDoc = XmlUtils.LoadXmlDocument(xmlOutputFile);

            XmlNode testResultsNode = XmlUtils.FindFirstChildByName(xmlDoc, "test-results");

            int totalTests = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "total"));
            int errors = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "errors"));
            int failures = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "failures"));
            int notRun = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "not-run"));
            int inconclusive = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "inconclusive"));
            int ignored = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "ignored"));
            int skipped = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "skipped"));
            int invalid = int.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "invalid"));
            TimeSpan totalTime = TimeSpan.Parse(XmlUtils.GetXmlAttribute(testResultsNode, "time"));

            testResult.ExecutionTime = totalTime.TotalMilliseconds;
            testResult.Result = (errors + failures) == 0 ? TestResult.ExitResult.Pass : TestResult.ExitResult.Fail;
        }
    }
}


using System;
using NTestController;
using Utilities;
using static NTestController.TestResult;
using System.Linq;
using System.Collections.Generic;

[assembly: CLSCompliant(true)]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
namespace NUnitReporter
{
    public class NUnitReporterPlugin : IReporterPlugin
    {
        public TestQueue TestQueue { get; set; }
        private string _testInputFile;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitReporterPlugin"/> class.
        /// </summary>
        /// <param name="testInputFile">NTestController.xml file path.</param>
        public NUnitReporterPlugin(string testInputFile)
        {
            ThrowIf.StringIsNullOrWhiteSpace(testInputFile, nameof(testInputFile));

            _testInputFile = testInputFile;
        }

        #region Inherited from IPlugin

        public string Name { get { return nameof(NUnitReporter); } }
        public PluginType PluginType { get { return PluginType.TestReporter; } }

        /// <seealso cref="IPlugin.Execute()"/>
        public bool Execute()
        {
            List<Test> tests = TestQueue.CompletedTests.ToList();
        
            int successfull_tests = 0;
            int failed_tests = 0;
            int error_tests = 0;
            int ignored_tests = 0;
            int inconclusive_tests = 0;
            int not_ran_tests = 0;

            // Read list of test results and calculate summary.
            foreach (var test in tests)
            {
                var successfull_test = test.TestRuns.ToList().Find(u => u.Result == ExitResult.Pass);
                if (successfull_test != null) successfull_tests++;

                var failed_test = test.TestRuns.ToList().Find(u => u.Result == ExitResult.Fail);
                if (failed_test != null) failed_tests++;

                var error_test = test.TestRuns.ToList().Find(u => u.Result == ExitResult.Error);
                if (error_test != null) error_tests++;

                var ignored_test = test.TestRuns.ToList().Find(u => u.Result == ExitResult.Ignored);
                if (ignored_test != null) ignored_tests++;

                var inconclusive_test = test.TestRuns.ToList().Find(u => u.Result == ExitResult.Inconclusive);
                if (inconclusive_test != null) inconclusive_tests++;

                var not_ran_test = test.TestRuns.ToList().Find(u => u.Result == ExitResult.NotRun);
                if (not_ran_test != null) not_ran_tests++;
            }

            if (successfull_tests > 0)
                Console.WriteLine("Total test cases passed:         {0}", successfull_tests);
            if (failed_tests > 0)
                Console.WriteLine("Total test cases failed:         {0}", failed_tests);
            if (error_tests > 0)
                Console.WriteLine("Total test cases with errors:    {0}", error_tests);
            if (ignored_tests > 0)
                Console.WriteLine("Total test cases ignored:        {0}", ignored_tests);
            if (inconclusive_tests > 0)
                Console.WriteLine("Total test cases inconclusive:   {0}", inconclusive_tests);
            if (not_ran_tests > 0)
                Console.WriteLine("Total test cases not run:        {0}", not_ran_tests);

            if (tests != null)
                Console.WriteLine("Total test cases:                {0}", tests.Count);

            return true;
        }

        /// <seealso cref="IReporterPlugin.ClonePlugin()"/>
        public IReporterPlugin ClonePlugin()
        {
            var newPlugin = new NUnitReporterPlugin(_testInputFile);

            newPlugin.TestQueue = TestQueue;

            return newPlugin;
        }

        #endregion Inherited from IPlugin
    }
}

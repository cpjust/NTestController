using System;
using System.Collections.Generic;
using Utilities;
using Logger;

namespace NTestController
{
    /// <summary>
    /// The result of a single test execution.
    /// </summary>
    public class TestResult
    {
        /// <summary>Exit result of a test.</summary>
        public enum ExitResult
        {
            NotRun,
            Pass,
            Fail,
            Error,
            Ignored,
            Inconclusive
        }

        /// <summary>The pass/fail result of the test.</summary>
        public ExitResult Result { get; set; }

        /// <summary>The standard output of the test.</summary>
        public string Output { get; set; }

        /// <summary>The error message if the test failed, otherwise null.</summary>
        public string Error { get; set; }

        /// <summary>The stack trace if an error occurred, otherwise null.</summary>
        public string StackTrace { get; set; }

        /// <summary>If supported by the TestExecutor plugin, this is the location of the output file for this test attempt.</summary>
        public string OutputFile { get; set; }

        /// <summary>The source computer where this test was executing.</summary>
        public string ExecutionSource { get; set; }

        /// <summary>The target of this test (ex. the computer or URL where the test was executing against).</summary>
        public string ExecutionTarget { get; set; }

        /// <summary>The test execution time in milliseconds.</summary>
        public double ExecutionTime { get; set; }

        /// <summary>The test timeout in milliseconds.  0 indicates no timeout.</summary>
        public int Timeout { get; set; }

        /// <summary>Plugins can add their own custom properties here, with the key being the property name.</summary>
        public Dictionary<string, object> ExtendedProperties { get; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// A test to run including the full test name and the results of each retry attempt.
    /// </summary>
    public class Test
    {
        private string _testName = null;

        /// <summary>The fully qualified name of the test (ex. namespace.class.function).</summary>
        public virtual string TestName
        {
            get
            {
                if ((_testName == null) && !string.IsNullOrWhiteSpace(TestNamespace) &&
                    !string.IsNullOrWhiteSpace(TestClass) && !string.IsNullOrWhiteSpace(TestFunction))
                {
                    return StringUtils.FormatInvariant("{0}.{1}.{2}",
                        TestNamespace, TestClass, TestFunction);
                }

                return _testName;
            }
            set
            {
                _testName = value;
            }
        }

        /// <summary>If supported by the TestExecutor plugin, the namespace of the test.</summary>
        public string TestNamespace { get; set; }

        /// <summary>If supported by the TestExecutor plugin, the class name (or class.subclasses) of the test.</summary>
        public string TestClass { get; set; }

        /// <summary>If supported by the TestExecutor plugin, the test function name (and any function parameters used in the test).</summary>
        public string TestFunction { get; set; }

        /// <summary>A list of test results for each attempt.  The first attempt is at index 0 and the latest result is at the highest index.</summary>
        public IList<TestResult> TestRuns { get; } = new List<TestResult>();

        /// <summary>Plugins can add their own custom properties here, with the key being the property name.</summary>
        public Dictionary<string, object> ExtendedProperties { get; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// A test queue to hold all of the tests to be run (or that completed).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")] // This is a stupid rule.
    public class TestQueue
    {
        #region Member variables

        private readonly object _lock = new object();
        private Queue<Test> _testsToRun = new Queue<Test>();
        private List<Test> _completedTests = new List<Test>();
        private static ILogger Log { get; } = ConsoleLogger.Instance;

        #endregion Member variables

        /// <summary>Gets a collection of completed tests.</summary>
        public IReadOnlyCollection<Test> CompletedTests
        {
            get { return _completedTests.AsReadOnly(); }
        }

        /// <summary>
        /// Enqueues a test to run.
        /// </summary>
        /// <param name="test">The test to add to the queue.</param>
        public void EnqueueTestToRun(Test test)
        {
            ThrowIf.ArgumentNull(test, nameof(test));

            Log.WriteDebug("Queueing test: {0}", test.TestName);

            lock(_lock)
            {
                _testsToRun.Enqueue(test);
            }
        }

        /// <summary>
        /// Dequeues a test to run.
        /// </summary>
        /// <returns>A test to run or null if the queue is empty.</returns>
        public Test DequeueTestToRun()
        {
            Test test = null;

            lock (_lock)
            {
                if (_testsToRun.Count > 0)
                {
                    test = _testsToRun.Dequeue();
                }
            }

            return test;
        }

        /// <summary>
        /// Adds a test that finished and has no retries left.
        /// </summary>
        /// <param name="test">The test to add to the list.</param>
        public void AddCompletedTest(Test test)
        {
            ThrowIf.ArgumentNull(test, nameof(test));

            lock(_lock)
            {
                _completedTests.Add(test);
            }
        }
    }
}


using System;
using Utilities;
using NTestController;
using Logger;

namespace NUnitReader
{
    public class NUnitTest : Test
    {
        private ILogger Logger { get; } = ConsoleLogger.Instance;
        private string _testFullName;

        public string DllPath { get; set; }

        public override string TestName
        {
            get { return _testFullName; }
            set { SetTestName(value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitReader.NUnitTest"/> class.
        /// </summary>
        /// <param name="dllPath">The path to the DLL.</param>
        /// <param name="fullTestName">Fully qualified test name
        ///     (either 'namespace' or 'namespace.class' or 'namespace.class.function'.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NUnitTest(string dllPath, string fullTestName)
        {
            DllPath = dllPath;
            SetTestName(fullTestName);
        }

        #region Private functions

        /// <summary>
        /// Splits the full test name into it's namespace, class and test name components.
        /// </summary>
        /// <param name="testName">The full test name.</param>
        private void ParseTestName(string testName)
        {
            ThrowIf.StringIsNullOrWhiteSpace(testName, nameof(testName));

            var nameParts = testName.Split('.');

            TestNamespace = nameParts[0];

            if (nameParts.Length > 1)
            {
                // Find the last part that isn't the test function name.
                int testNameIndex;

                for (testNameIndex = 1; testNameIndex < nameParts.Length; ++testNameIndex)
                {
                    if (nameParts[testNameIndex].Contains("("))
                    {
                        break;
                    }
                }

                Logger.WriteDebug("testName = {0}", testName);
                Logger.WriteDebug("nameParts.Length = {0}", nameParts.Length);
                Logger.WriteDebug("testNameIndex = {0}", testNameIndex);

                TestClass = string.Join(".", nameParts, startIndex: 1, count: testNameIndex - 1);
                Logger.WriteDebug("TestClass = {0}", TestClass);

                // Everything else must be the test function name (and any parameters).
                if ((nameParts.Length > 2) && (testNameIndex < nameParts.Length))
                {
                    TestFunction = string.Join(".", nameParts, startIndex: testNameIndex, count: nameParts.Length - testNameIndex);
                    Logger.WriteDebug("TestFunction = {0}", TestFunction);
                }
            }
        }

        /// <summary>
        /// Sets the test name and parses it into namespace, class and function.
        /// </summary>
        /// <param name="testName">The full test name.</param>
        private void SetTestName(string testName)
        {
            _testFullName = testName;
            ParseTestName(testName);
        }

        #endregion Private functions
    }
}


using System;
using Utilities;

namespace NUnitReader
{
    public class TestInput
    {
        private string _testFullName;

        public string DllPath;
        public string TestFullName
        {
            get { return _testFullName; }
            set
            {
                _testFullName = value;
                ParseTestName(value);
            }
        }

        public string TestNamespace;
        public string TestClass;
        public string TestFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitReader.TestInput"/> class.
        /// </summary>
        /// <param name="dllPath">The path to the DLL.</param>
        /// <param name="fullTestName">Fully qualified test name
        ///     (either 'namespace' or 'namespace.class' or 'namespace.class.function'.</param>
        public TestInput(string dllPath, string fullTestName)
        {
            DllPath = dllPath;
            TestFullName = fullTestName;
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
                // If the test name has parameters, it will contain a parentheses.
                int testNameIndex;

                for (testNameIndex = 1; testNameIndex < nameParts.Length; ++testNameIndex)
                {
                    if (nameParts[testNameIndex].Contains("("))
                    {
                        break;
                    }
                }

                TestClass = string.Join(".", nameParts, startIndex: 1, count: testNameIndex - 1);

                // Everything else must be the test function name (and any parameters).
                if (nameParts.Length > 2)
                {
                    TestFunction = string.Join("", nameParts, startIndex: testNameIndex, count: nameParts.Length - testNameIndex);
                }
            }
        }

        #endregion Private functions
    }
}


using NUnit.Framework;
using System;
using Utilities;
using System.IO;
using NUnitReader;
using Logger;
using NTestController;

namespace NUnitReaderTests
{
    [TestFixture]
    public class NUnitReaderPluginTests
    {
        private const string EXECUTE_FUNC = "NUnitReaderPlugin.Execute()";

        private string _filename;
        private ILogger Logger { get; } = ConsoleLogger.Instance;

        [TearDown]
        public void TearDown()
        {
            if (!string.IsNullOrEmpty(_filename) && File.Exists(_filename))
            {
                Logger.WriteInfo("Deleting temp file: '{0}'", _filename);
                File.Delete(_filename);
            }
        }

        #region Success Tests

        [TestCase]
        public void Execute_EmptyFile_NoTests()
        {
            // Setup:
            // Create an empty file.
            _filename = FileUtilities.CreateTempFile();

            var plugin = new NUnitReaderPlugin(_filename);

            // Execute:
            Assert.IsTrue(plugin.Execute(), "{0} should return true!", EXECUTE_FUNC);

            // Verify:
            Assert.AreEqual(0, plugin.Tests.Count, "There should be no tests in a blank file!");
        }

        [TestCase]
        public void Execute_NameProperty_HasCorrectValue()
        {
            // Setup:
            // Create an empty file.
            _filename = FileUtilities.CreateTempFile();

            var plugin = new NUnitReaderPlugin(_filename);

            // Execute & Verify:
            string expectedName = "NUnitReader";
            Assert.AreEqual(expectedName, plugin.Name, "The NUnitReaderPlugin.Name property should equal {0}!", expectedName);
        }

        [TestCase]
        public void Execute_PluginTypeProperty_HasCorrectValue()
        {
            // Setup:
            // Create an empty file.
            _filename = FileUtilities.CreateTempFile();

            var plugin = new NUnitReaderPlugin(_filename);

            // Execute & Verify:
            var expectedType = PluginType.TestReader;
            Assert.AreEqual(expectedType, plugin.PluginType, "The NUnitReaderPlugin.PluginType property should equal {0}!", expectedType);
        }

        [TestCase(1, "Test.dll | Test1")]
        [TestCase(1, "Test.dll | Test1\n# This line is commented out.")]
        [TestCase(2, "Test.dll | Test1\nTest.dll | Test2")]
        [TestCase(3, "Test.dll | Test1\n \nTest.dll | Test2\r\n Test.dll | Test3")]
        public void Execute_ValidData_ExpectedNumberOfTests(int expectedNumberOfTests, string line)
        {
            // Setup:
            // Create a file with the specified line of text.
            _filename = FileUtilities.CreateTempFile();
            FileUtilities.AppendLineToFile(_filename, line);

            var plugin = new NUnitReaderPlugin(_filename);

            // Execute:
            Assert.IsTrue(plugin.Execute(), "{0} should return true!", EXECUTE_FUNC);

            // Verify:
            Assert.AreEqual(expectedNumberOfTests, plugin.Tests.Count,
                "There should be {0} tests in the file!", expectedNumberOfTests);
        }

        [TestCase("C:\\Folder1\\Test.dll", "TestNamespace1", null, null)]
        [TestCase("/home/testuser/Test.dll", "TestNamespace1", null, null)]
        [TestCase("Test.dll", "TestNamespace1", null, null)]
        [TestCase("Test.dll", "TestNamespace1", "TestClass1", null)]
        [TestCase("Test.dll", "TestNamespace1", "TestClass1", "TestFunction1()")]
        [TestCase("Test.dll", "TestNamespace1", "TestClass1.TestClass2", null)]
        [TestCase("Test.dll", "TestNamespace1", "TestClass1.TestClass2", "TestFunction1()")]
        [TestCase("Test.dll", "TestNamespace1", "TestClass1", "TestFunction1(3.14, \"pi\")")]
        [TestCase("Test.dll", "TestNamespace1", "TestClass1", "TestFunction1(\"left | right\")")]
        public void Execute_DifferentTestFormats_TestsAreAdded(
            string testDll, string testNamespace, string testClass, string testFunction)
        {
            // Setup:
            string testName = CreateFullTestName(testNamespace, testClass, testFunction);
            string line = StringUtils.FormatInvariant("{0} | {1}", testDll, testName);

            // Create a file with the specified line of text.
            _filename = FileUtilities.CreateTempFile();
            FileUtilities.AppendLineToFile(_filename, line);

            var plugin = new NUnitReaderPlugin(_filename);

            // Execute:
            Assert.IsTrue(plugin.Execute(), "{0} should return true!", EXECUTE_FUNC);

            // Verify:
            Assert.AreEqual(1, plugin.Tests.Count, "There should be 1 test in the file!");
            var test = plugin.Tests[0];

            // These should have the values we set.
            Assert.AreEqual(testNamespace, test.TestNamespace, "Test.TestNamespace doesn't have the expected value!");
            Assert.AreEqual(testClass, test.TestClass, "Test.TestClass doesn't have the expected value!");
            Assert.AreEqual(testFunction, test.TestFunction, "Test.TestFunction doesn't have the expected value!");
            Assert.AreEqual(testName, test.TestName, "Test.TestName doesn't have the expected value!");

            var nunitTest = test as NUnitTest;
            Assert.AreEqual(testDll, nunitTest.DllPath, "NUnitTest.DllPath doesn't have the expected value!");

            // These shouldn't have any values in them.
            Assert.AreEqual(0, test.ExtendedProperties.Count, "Test.ExtendedProperties should be empty!");
            Assert.AreEqual(0, test.TestRuns.Count, "Test.TestRuns should be empty!");
        }

        #endregion Success Tests

        #region Error Tests

        [TestCase]
        public static void Execute_NonExistentFile_FileNotFoundException()
        {
            // Setup:
            var plugin = new NUnitReaderPlugin("ThisFileShouldNotExist");

            // Execute & Verify:
            Assert.Throws<FileNotFoundException>(() => plugin.Execute(),
                "{0} should throw a FileNotFoundException if the test file doesn't exist!", EXECUTE_FUNC);
        }

        [TestCase("Test.dll")]
        public void Execute_WrongNumberOfPartsInFile_InvalidDataException(string line)
        {
            // Setup:
            // Create a file with the specified line of text.
            _filename = FileUtilities.CreateTempFile();
            FileUtilities.AppendLineToFile(_filename, line);

            var plugin = new NUnitReaderPlugin(_filename);

            // Execute:
            Assert.Throws<InvalidDataException>(() => plugin.Execute(),
                "{0} should throw a InvalidDataException if there aren't exactly 2 parts in the file!", EXECUTE_FUNC);

            // Verify:
            Assert.AreEqual(0, plugin.Tests.Count, "There should be no tests in a blank file!");
        }

        #endregion Error Tests

        #region Private Functions

        /// <summary>
        /// Creates the full name of the test.
        /// </summary>
        /// <param name="testNamespace">Test namespace.</param>
        /// <param name="testClass">Test class.</param>
        /// <param name="testFunction">Test function.</param>
        /// <returns>The full test name.</returns>
        private static string CreateFullTestName(string testNamespace, string testClass, string testFunction)
        {
            // Setup:
            string testName = StringUtils.FormatInvariant("{0}.{1}.{2}",
                                  testNamespace, testClass ?? string.Empty, testFunction ?? string.Empty);
            return testName.TrimEnd('.');
        }

        #endregion Private Functions
    }
}


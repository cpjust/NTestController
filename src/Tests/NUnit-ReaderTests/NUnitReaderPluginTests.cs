using NUnit.Framework;
using System;
using Utilities;
using System.IO;
using NUnitReader;
using Logger;

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

        #endregion Success Tests

        #region Error Tests

        [TestCase]
        public void Execute_NonExistentFile_FileNotFoundException()
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
    }
}


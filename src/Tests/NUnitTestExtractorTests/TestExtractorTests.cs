using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;

using NUnit.Framework;

using NUnitTestExtractor;

namespace NUnitTestExtractorTests
{
    [TestFixture]
    public static class TestExtractorTests
    {
        #region WritingToFile Tests

        [TestCaseSource(typeof(InvalidDataTestCasesCollection))]
        [Description("Write data to a file using an invalid dll. Verify nothing is written.")]
        [Test]
        public static void WritingToFile_InvalidDll_NothingWritten(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);
            
            StringBuilder builder = new StringBuilder();

            var ex = Assert.Throws<FileNotFoundException>(() => NUnitTestExtractorApp.GetTests(dlls, NUnitTestExtractorApp.ParseLevel(level)));
            Assert.That(ex.Message, Is.StringContaining("File was not found:"));

            Assert.That(builder.ToString(), Is.Empty, "Nothing should be written.");
        }

        [TestCaseSource(typeof(ValidDataTestCasesCollection))]
        [Description("Write data to a file using a valid dll and level. Verify the correct data is written.")]
        [Test]
        public static void WritingToFile_ValidDllAndEveryLevel_GetsWrittenWithProperFormat(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            StringBuilder builder = new StringBuilder();

            NUnitTestExtractorApp.GetTests(dlls, NUnitTestExtractorApp.ParseLevel(level));

            string builderText = builder.ToString();
            string firstLine = builderText.Substring(0, builderText.IndexOf(Environment.NewLine, StringComparison.OrdinalIgnoreCase));

            NUnitTestExtractorApp.Level myLevel = NUnitTestExtractorApp.ParseLevel(level);

            Assert.That(VerifyOutputFormat(myLevel).IsMatch(firstLine), "Regex format should match output.");
        }
        
        [TestCaseSource(typeof(ValidDataTestCasesCollection))]
        [Description("Write data to a file using a valid dll and level. Verify the file contains no duplicates.")]
        [Test]
        public static void WritingToFile_ValidDllAndEveryLevel_FileContainsNoDuplicates(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            StringBuilder builder = new StringBuilder();

            NUnitTestExtractorApp.GetTests(dlls, NUnitTestExtractorApp.ParseLevel(level));

            string[] lines = builder.ToString().Split('\n');
           
            Assert.AreEqual(lines.Length, lines.Distinct().Count());
        }

        #endregion WritingToFile Tests

        #region WritingToStdout Tests

        [TestCaseSource(typeof(ValidDataTestCasesCollection))]
        [Description("Write data to stdout using a valid dll and level. Verify data is written with the proper format.")]
        [Test]
        public static void WritingToStdout_ValidDllAndEveryLevel_GetsWrittenWithProperFormat(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            using (Process process = SetUpProcess(dll, level))
            {
                string line = process.StandardOutput.ReadLine();

                NUnitTestExtractorApp.Level myLevel = NUnitTestExtractorApp.ParseLevel(level);

                Assert.That(VerifyOutputFormat(myLevel).IsMatch(line), "Regex format should match output.");
            }
        }

        [TestCaseSource(typeof(InvalidDataTestCasesCollection))]
        [Description("Write data to stdout using an invalid dll. Verify nothing is written.")]
        [Test]
        public static void WritingToStdout_InvalidDllAndEveryLevel_NothingWritten(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            using (Process process = SetUpProcess(dll, level))
            {
                Assert.That(process.StandardOutput.ReadToEnd(), Is.Empty, "Nothing should be written");
            }
        }

        [TestCaseSource(typeof(ValidDataTestCasesCollection))]
        [Description("Write data to stdout using a valid dll and level. Verify no duplicates are written.")]
        [Test]
        public static void WritingToStdout_ValidDllAndEveryLevel_ContainsNoDuplicates(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            using (Process process = SetUpProcess(dll, level))
            {
                string line = string.Empty;

                List<string> lines = new List<string>();

                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    lines.Add(line);
                }

                Assert.AreEqual(lines.Count, lines.Distinct().Count(), "No duplicates should be written.");
            } 
        }

        #endregion WritingToStdout Tests

        #region ParsingLevel Tests

        [TestCase("NAMESPACE")]
        [TestCase("namespace")]
        [TestCase("NaMeSpAcE")]
        [Description("Parse string 'namespace' with different cases to a level enum. Verify string gets parsed successfuly.")]
        [Test]
        public static void ParsingLevel_NamespaceWrittenInDifferentCases_WillParseWithoutError(string level)
        {
            Assert.AreEqual(NUnitTestExtractorApp.Level.Namespace, NUnitTestExtractorApp.ParseLevel(level), "string should be parsed successfuly.");
        }

        [TestCase("CLASS")]
        [TestCase("class")]
        [TestCase("ClAsS")]
        [Description("Parse string 'class' with different cases to a level enum. Verify string gets parsed successfuly.")]
        [Test]
        public static void ParsingLevel_ClassWrittenInDifferentCases_WillParseWithoutError(string level)
        {
            Assert.AreEqual((NUnitTestExtractorApp.Level.Class), NUnitTestExtractorApp.ParseLevel(level), "string should be parsed successfuly.");
        }
        
        [TestCase("FUNCTION")]
        [TestCase("function")]
        [TestCase("FuNcTiOn")]
        [Description("Parse string 'function' with different cases to a level enum. Verify string gets parsed successfuly.")]
        [Test]
        public static void ParsingLevel_FunctionWrittenInDifferentCases_WillParseWithoutError(string level)
        {
            Assert.AreEqual((NUnitTestExtractorApp.Level.Function), NUnitTestExtractorApp.ParseLevel(level), "string should be parsed successfuly.");
        }

        [TestCase("TESTCASE")]
        [TestCase("testcase")]
        [TestCase("TeStCaSe")]
        [Description("Parse string 'testcase' with different cases to a level enum. Verify string gets parsed successfuly.")]
        [Test]
        public static void ParsingLevel_TestCaseWrittenInDifferentCases_WillParseWithoutError(string level)
        {
            Assert.AreEqual((NUnitTestExtractorApp.Level.TestCase), NUnitTestExtractorApp.ParseLevel(level), "string should be parsed successfuly.");
        }

        #endregion ParsingLevel Tests

        #region TestCase Tests

        [TestCase(new object[] { 2.34 })]
        [TestCase(new object[] { "text" })]
        [TestCase(new object[] { 2 })]
        [Description("Append the arguments to a string. Verify the arguments get parsed to their type successfuly.")]
        [Test]
        public static void WritingTestCaseArgs_ArgIsSingleValidDataType_WritesWithoutError(object[] array)
        {
            if(array != null)
            {
                string data = string.Empty;
                data = NUnitTestExtractorApp.AppendArgumentsForTestCasesToString(array, data);

                object actualValue = null;
                
                object expectedValue = array[0];

                switch (expectedValue.GetType().ToString())
                {
                    case "System.Int32":
                        actualValue = int.Parse(data, CultureInfo.CurrentCulture);
                        break;

                    case "System.String":
                        actualValue = data;
                        expectedValue = "\"" + expectedValue + "\"";
                        break;

                    case "System.Double":
                        actualValue = double.Parse(data, CultureInfo.CurrentCulture);
                        break;
                }

                Assert.AreEqual(expectedValue, actualValue, "Argument should parse from string to original type successfuly.");
            }
        }
        
        [Description("Verify a TestCase with no arguments gets formatted correctly.")]
        [Test]
        public static void WritingTestCase_NoArgs_WritesWithEmptyparentheses()
        {
            object[] args = new object[] { };

            string declaringType = "Namespace.Class";
            string name = "TestCase";
            string data = string.Empty;

            data = NUnitTestExtractorApp.FormattedTestCase(declaringType, name, args, data);

            Assert.AreEqual("Namespace.Class.TestCase()", data, "TestCase should be formatted correctly.");
        }

        #endregion TestCase Tests

        #region Private Functions

        /// <summary>
        /// Determines which regex string return depending on the Level 
        /// </summary>
        /// <param name="myLevel">The level which to use to determine which regex to return</param>
        /// <returns>The proper regex string format which is then used to compare strings to</returns>
        private static Regex VerifyOutputFormat(NUnitTestExtractorApp.Level myLevel)
        {
            Regex regex = null;

            switch (myLevel)
            {
                case NUnitTestExtractorApp.Level.Namespace:
                    regex = new Regex(@"^(\""(.+)""\s+)\|\s+\w+$");
                    break;

                case NUnitTestExtractorApp.Level.Class:
                    regex = new Regex(@"^(\""(.+)""\s+)\|\s+\w+\.\w+$");
                    break;

                case NUnitTestExtractorApp.Level.Function:
                    regex = new Regex(@"^(\""(.+)""\s+)\|\s+\w+\.\w+\.\w+$");
                    break;
            }
            return regex;
        }

        /// <summary>
        /// Sets up and runs process which is used to read lines in the standard output
        /// </summary>
        /// <param name="dll">Dlls to pass to the process</param>
        /// <param name="level"> level to pass to the process</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static Process SetUpProcess(string dll, string level)
        {
            Process process = new Process();

            process.StartInfo.FileName = "NUnitTestExtractor.exe";
            process.StartInfo.Arguments = "-l " + level + " " + dll;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            process.Start();

            return process;
        }

        #endregion Private Functions
    }

    #region TestCaseSources

    public class ValidDataTestCasesCollection : IEnumerable<string[]>
    {
        public IEnumerator GetEnumerator()
        {
            yield return new string[] { "NUnitTestExtractorTests.dll", ((NUnitTestExtractorApp.Level)0).ToString() };
            yield return new string[] { "NUnitTestExtractorTests.dll", ((NUnitTestExtractorApp.Level)1).ToString() };
            yield return new string[] { "NUnitTestExtractorTests.dll", ((NUnitTestExtractorApp.Level)2).ToString() };
        }

        IEnumerator<string[]> IEnumerable<string[]>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class InvalidDataTestCasesCollection : IEnumerable<string[]>
    {
        public IEnumerator GetEnumerator()
        {
            yield return new string[] { "fakefile.dll", ((NUnitTestExtractorApp.Level)0).ToString() };
            yield return new string[] { "fakefile.dll", ((NUnitTestExtractorApp.Level)1).ToString() };
            yield return new string[] { "fakefile.dll", ((NUnitTestExtractorApp.Level)2).ToString() };
        }

        IEnumerator<string[]> IEnumerable<string[]>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    #endregion TestCaseSources

}
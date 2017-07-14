using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using NUnitTestExtractor;
using System.Diagnostics;
using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;

namespace NUnitTestExtractorTests
{
    [TestFixture]
    public class TestExtractorTests
    {

        /// <summary>
        /// Used to invoke the GetTests() method in NUnitTestExtractorApp
        /// </summary>
        /// <param name="dlls">The dlls to pass to GetTests()</param> 
        /// <param name="level">The level to pass to GetTests()</param>
        private static void InvokeGetTestsUsingFileOutput(StringBuilder builder, List<string> dlls, NUnitTestExtractorApp.Level level)
        {
            using (StringWriter writer = new StringWriter(builder, CultureInfo.CurrentCulture))
            {
                NUnitTestExtractorApp.GetTests(dlls, level, writer);
            }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCaseSource(typeof(InvalidDataTestCasesCollection))]
        [Test]
        public void WritingToFile_InvalidDll_NothingWritten(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);
            
            StringBuilder builder = new StringBuilder();

            InvokeGetTestsUsingFileOutput(builder, dlls, NUnitTestExtractorApp.ParseLevel(level));

            Assert.That(builder.ToString(), Is.Empty);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCaseSource(typeof(ValidDataTestCasesCollection))]
        [Test]
        public void WritingToFile_ValidDllAndEveryLevel_GetsWrittenWithProperFormat(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            StringBuilder builder = new StringBuilder();

            InvokeGetTestsUsingFileOutput(builder, dlls, NUnitTestExtractorApp.ParseLevel(level));

            string builderText = builder.ToString();
            string firstLine = builderText.Substring(0,builderText.IndexOf(Environment.NewLine, StringComparison.OrdinalIgnoreCase) + 1);

            Regex regex = null;

            NUnitTestExtractorApp.Level myLevel = NUnitTestExtractorApp.ParseLevel(level);

            switch (myLevel)
            {
                case NUnitTestExtractorApp.Level.Namespace:
                    regex = new Regex(@"\w+\s\|\s[^\.]");
                    break;

                case NUnitTestExtractorApp.Level.Class:
                    regex = new Regex(@"\w+\s\|\s[^\.\.^\.]");
                    break;

                case NUnitTestExtractorApp.Level.Function:
                    regex = new Regex(@"\w+\s\|\s[^\.\.^\.^\.]");
                    break;
            }

            Assert.That(regex.IsMatch(firstLine));
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCaseSource(typeof(ValidDataTestCasesCollection))]
        [Test]
        public void WritingToFile_ValidDllAndEveryLevel_FileContainsNoDuplicates(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            StringBuilder builder = new StringBuilder();
            
            InvokeGetTestsUsingFileOutput(builder, dlls, NUnitTestExtractorApp.ParseLevel(level));

            string[] lines = builder.ToString().Split('\n');
           
            Assert.AreEqual(lines.Length, lines.Distinct().Count());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCaseSource(typeof(ValidDataTestCasesCollection))]
        [Test]
        public void WritingToStdout_ValidDllAndEveryLevel_GetsWrittenWithProperFormat(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            using (Process process = SetUpProcess(dll, level))
            {
                string line = process.StandardOutput.ReadLine();

                Regex regex = null;

                NUnitTestExtractorApp.Level myLevel = NUnitTestExtractorApp.ParseLevel(level);

                switch (myLevel)
                {
                    case NUnitTestExtractorApp.Level.Namespace:
                        regex = new Regex(@"\w+\s\|\s[^\.]");
                        break;

                    case NUnitTestExtractorApp.Level.Class:
                        regex = new Regex(@"\w+\s\|\s[^\.\.^\.]");
                        break;

                    case NUnitTestExtractorApp.Level.Function:
                        regex = new Regex(@"\w+\s\|\s[^\.\.^\.^\.]");
                        break;
                }

                Assert.That(regex.IsMatch(line));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCaseSource(typeof(InvalidDataTestCasesCollection))]
        [Test]
        public void WritingToStdout_InvalidDllAndEveryLevel_NothingWritten(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            using (Process process = SetUpProcess(dll, level))
            {
                Assert.That(process.StandardOutput.ReadToEnd(), Is.Empty);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCaseSource(typeof(ValidDataTestCasesCollection))]
        [Test]
        public void WritingToStdout_ValidDllAndEveryLevel_ContainsNoDuplicates(string dll, string level)
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

                Assert.AreEqual(lines.Count, lines.Distinct().Count());
            } 
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase("NAMESPACE", Result = "bleh")]
        [TestCase("namespace")]
        [TestCase("NaMeSpAcE")]
        [Test]
        public void ParsingLevel_NamespaceWrittenInDifferentCases_WillParseWithoutError(string level)
        {
            Assert.That(NUnitTestExtractorApp.ParseLevel(level), Is.EqualTo(NUnitTestExtractorApp.Level.Namespace));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase("CLASS")]
        [TestCase("class")]
        [TestCase("ClAsS")]
        [Test]
        public void ParsingLevel_ClassWrittenInDifferentCases_WillParseWithoutError(string level)
        {
            Assert.AreEqual(NUnitTestExtractorApp.ParseLevel(level), (NUnitTestExtractorApp.Level.Class));
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase("FUNCTION")]
        [TestCase("function")]
        [TestCase("FuNcTiOn")]
        [Test]
        public void ParsingLevel_FunctionWrittenInDifferentCases_WillParseWithoutError(string level)
        {
            Assert.AreEqual(NUnitTestExtractorApp.ParseLevel(level), (NUnitTestExtractorApp.Level.Function));
        }
    }

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
}
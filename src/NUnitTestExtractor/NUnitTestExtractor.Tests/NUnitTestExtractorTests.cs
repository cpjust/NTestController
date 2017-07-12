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

namespace NUnitTestExtractorTests
{
    [TestFixture]
    public class NUnitTestExtractorTests
    {
        private const string outputDirectory = "C:/Git/NTestController/src/NUnitTestExtractor/NUnitTestExtractor.Tests/bin/Debug/TestOutput/file.txt";
        
        /// <summary>
        /// Used to invoke the GetTests() method in NUnitTestExtractorApp
        /// </summary>
        /// <param name="dlls">The dlls to pass to GetTests()</param>
        /// <param name="level">The level to pass to GetTests()</param>
        private void InvokeGetTestsUsingFileOutput(List<string> dlls, NUnitTestExtractorApp.Level level)
        {
            using (StreamWriter writer = new StreamWriter(outputDirectory))
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
       private Process SetUpProcess(string dll, string level)
        {
            Process process = new Process();

            process.StartInfo.FileName = "NUnitTestExtractor.exe";
            process.StartInfo.Arguments = "-l " + level + " " + dll;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            process.Start();

            return process;
        }

        [SetUp]
        public void CreateDirectoryAndFile()
        {
            int index = outputDirectory.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);

            string directory = "";

            if (index > 0)
            {
                directory = outputDirectory.Substring(0, index);
            }

            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(outputDirectory))
            {
                using (FileStream stream = File.Create(outputDirectory)) { }
            }
        } 

        [TearDown]
        public void EraseOutputFile()
        {
            if (File.Exists(outputDirectory))
            {
                File.Delete(outputDirectory);
            }
        }

        [TestCaseSource(typeof(InvalidDataTestCases))]
        public void WritingToFile_InvalidDll_NothingWritten(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            InvokeGetTestsUsingFileOutput(dlls, NUnitTestExtractorApp.ParseLevel(level));

            using (StreamReader reader = new StreamReader(outputDirectory))
            {
                string lines = reader.ReadToEnd();

                Assert.That(lines, Is.Empty);
            }
        }

        [TestCaseSource(typeof(ValidDataTestCases))]
        public void WritingToFile_ValidDllAndEveryLevel_GetsWritten(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            InvokeGetTestsUsingFileOutput(dlls, NUnitTestExtractorApp.ParseLevel(level));

            using (StreamReader reader = new StreamReader(outputDirectory))
            {
                string lines = reader.ReadToEnd();

                Assert.That(lines, Is.Not.Empty);
            }
        }

        [TestCase("C:/Git/NTestController/bin/Debug/NTestControllerTests.dll", NUnitTestExtractorApp.Level.Namespace)]
       public void WritingToFile_LevelIsNamespace_OnlyNamespaceWritten(string dll, NUnitTestExtractorApp.Level level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            InvokeGetTestsUsingFileOutput(dlls, level);

            string[] lines = File.ReadAllLines(outputDirectory);
            string result = lines[0].Substring(lines[0].IndexOf("|")+2);

            bool containsPeriod = result.Contains(".");

            Assert.That(!containsPeriod);
        }

        [TestCase("C:/Git/NTestController/bin/Debug/NTestControllerTests.dll", NUnitTestExtractorApp.Level.Class)]
        public void WritingToFile_LevelIsClass_NamespaceAndClassWritten(string dll, NUnitTestExtractorApp.Level level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            InvokeGetTestsUsingFileOutput(dlls, level);

            string[] lines = File.ReadAllLines(outputDirectory);
            string result = lines[0].Substring(lines[0].IndexOf("|") + 2);
            
            //This works as class and namespace are seperated by a period example: namepspace.class
            Assert.That(result.Contains("."));
        }

        [TestCase("C:/Git/NTestController/bin/Debug/NTestControllerTests.dll", NUnitTestExtractorApp.Level.Function)]
        public void WritingToFile_LevelIsFunction_NamespaceClassAndFunctionGetWritten(string dll, NUnitTestExtractorApp.Level level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            InvokeGetTestsUsingFileOutput(dlls, level);

            string[] lines = File.ReadAllLines(outputDirectory);
            string result = lines[0].Substring(lines[0].IndexOf("|") + 2);

            //Same deal as test above, function and class separated with a period: namespace.class.function
            Assert.That(result.Count(p => p == '.'), Is.EqualTo(2));
        }
        
        [TestCaseSource(typeof(ValidDataTestCases))]
        public void WritingToFile_ValidDllAndEveryLevel_FileContainsNoDuplicates(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            InvokeGetTestsUsingFileOutput(dlls, NUnitTestExtractorApp.ParseLevel(level));

            List<string> lines = new List<string>(File.ReadAllLines(outputDirectory));

            Assert.That(lines.Count == lines.Distinct().Count());
        }

        [TestCaseSource(typeof(ValidDataTestCases))]
        public void WritingToStdout_ValidDllAndEveryLevel_GetsWritten(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            Process process = SetUpProcess(dll, level);

            Assert.That(process.StandardOutput.ReadToEnd(), Is.Not.Empty);

            process.WaitForExit();
        }

        [TestCaseSource(typeof(InvalidDataTestCases))]
        public void WritingToStdout_InvalidDllAndEveryLevel_NothingWritten(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            Process process = SetUpProcess(dll, level);

            Assert.That(process.StandardOutput.ReadToEnd(), Is.Empty);

            process.WaitForExit();
        }

        [TestCase("C:/Git/NTestController/bin/Debug/NTestControllerTests.dll", NUnitTestExtractorApp.Level.Namespace)]
        public void WritingToStdout_LevelIsNamespace_OnlyNamespaceWritten(string dll, NUnitTestExtractorApp.Level level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            Process process = SetUpProcess(dll, level.ToString());

            string line = process.StandardOutput.ReadLine();
            string result = line.Substring(line.IndexOf("|") + 2);

            Assert.That(!result.Contains("."));
        }

        [TestCase("C:/Git/NTestController/bin/Debug/NTestControllerTests.dll", NUnitTestExtractorApp.Level.Class)]
        public void WritingToStdout_LevelIsClass_NamespaceAndClassWritten(string dll, NUnitTestExtractorApp.Level level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            Process process = SetUpProcess(dll, level.ToString());

            string line = process.StandardOutput.ReadLine();
            string result = line.Substring(line.IndexOf("|") + 2);

            //This works as class and namespace are seperated by a period example: namepspace.class
            Assert.That(result.Contains("."));
        }

        [TestCase("C:/Git/NTestController/bin/Debug/NTestControllerTests.dll", NUnitTestExtractorApp.Level.Function)]
        public void WritingToStdout_LevelIsFunction_NamespaceClassAndFunctionGetWritten(string dll, NUnitTestExtractorApp.Level level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            Process process = SetUpProcess(dll, level.ToString());

            string line = process.StandardOutput.ReadLine();
            string result = line.Substring(line.IndexOf("|") + 2);

            //Same deal as test above, function and class separated with a period: namespace.class.function
            Assert.That(result.Count(p => p == '.'), Is.EqualTo(2));
        }

        [TestCaseSource(typeof(ValidDataTestCases))]
        public void WritingToStdout_ValidDllAndEveryLevel_ContainsNoDuplicates(string dll, string level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            Process process = SetUpProcess(dll, level);

            string line = "";

            List<string> lines = new List<string>();

            while((line = process.StandardOutput.ReadLine())!= null)
            {
                lines.Add(line);
            }

            Assert.That(lines.Count == lines.Distinct().Count());
        }


        [TestCase("NAMESPACE")]
        [TestCase("namespace")]
        [TestCase("NaMeSpAcE")]
        public void ParsingLevel_NamespaceWrittenInDifferentCases_WillParseWithoutError(string levelString)
        {
            Assert.That(NUnitTestExtractorApp.ParseLevel(levelString),Is.EqualTo(NUnitTestExtractorApp.Level.Namespace));
        }

        [TestCase("CLASS")]
        [TestCase("class")]
        [TestCase("ClAsS")]
        public void ParsingLevel_ClassWrittenInDifferentCases_WillParseWithoutError(string levelString)
        {
            Assert.That(NUnitTestExtractorApp.ParseLevel(levelString), Is.EqualTo(NUnitTestExtractorApp.Level.Class));
        }

        [TestCase("FUNCTION")]
        [TestCase("function")]
        [TestCase("FuNcTiOn")]
        public void ParsingLevel_FunctionWrittenInDifferentCases_WillParseWithoutError(string levelString)
        {
            Assert.That(NUnitTestExtractorApp.ParseLevel(levelString), Is.EqualTo(NUnitTestExtractorApp.Level.Function));
        }
    }

    public class ValidDataTestCases : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new string[] { "C:/Git/NTestController/bin/Debug/NTestControllerTests.dll", ((NUnitTestExtractorApp.Level)0).ToString() };
            yield return new string[] { "C:/Git/NTestController/bin/Debug/NTestControllerTests.dll", ((NUnitTestExtractorApp.Level)1).ToString() };
            yield return new string[] { "C:/Git/NTestController/bin/Debug/NTestControllerTests.dll", ((NUnitTestExtractorApp.Level)2).ToString() };
        }
    }

    public class InvalidDataTestCases : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new string[] { "fakefile.dll", ((NUnitTestExtractorApp.Level)0).ToString() };
            yield return new string[] { "fakefile.dll", ((NUnitTestExtractorApp.Level)1).ToString() };
            yield return new string[] { "fakefile.dll", ((NUnitTestExtractorApp.Level)2).ToString() };
        }
    }

}

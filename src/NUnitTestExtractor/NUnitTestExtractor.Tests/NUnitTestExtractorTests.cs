using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using NUnitTestExtractor;

namespace NUnitTestExtractorTests
{
    [TestFixture]
    public class NUnitTestExtractorTests
    {
        private string outputDirectory = "C:/Git/NTestController/src/NUnitTestExtractor/NUnitTestExtractor.Tests/bin/Debug/TestOutput/file.txt";

        private void InvokeGetTestsUsingFileOutput(List<string> dlls, NUnitTestExtractorApp.Level level)
        {
            using (StreamWriter writer = new StreamWriter(outputDirectory))
            {
                NUnitTestExtractorApp.GetTests(dlls, level, writer);
            }
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
        
        [TestCase("Fake.dll", NUnitTestExtractorApp.Level.Class)]
        [Test]
        public void WritingToFile_InvalidDll_NothingWritten(string dll, NUnitTestExtractorApp.Level level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            InvokeGetTestsUsingFileOutput(dlls, level);

            using (StreamReader reader = new StreamReader(outputDirectory))
            {
                string lines = reader.ReadToEnd();

                Assert.That(lines, Is.Empty);
            }
        }

        [TestCase("C:/Git/NTestController/bin/Debug/NTestControllerTests.dll", NUnitTestExtractorApp.Level.Class)]
        [Test]
        public void WritingToFile_ValidDll_GetsWritten(string dll, NUnitTestExtractorApp.Level level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            InvokeGetTestsUsingFileOutput(dlls, level);

            using (StreamReader reader = new StreamReader(outputDirectory))
            {
                string lines = reader.ReadToEnd();

                Assert.That(lines, Is.Not.Empty);
            }
        }

        [TestCase("C:/Git/NTestController/bin/Debug/NTestControllerTests.dll", NUnitTestExtractorApp.Level.Namespace)]
        [Test]
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
        [Test]
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
        [Test]
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

        [TestCase("C:/Git/NTestController/bin/Debug/NTestControllerTests.dll", NUnitTestExtractorApp.Level.Namespace)]
        [Test]
        public void WritingToFile_WritingToFileUsingValidDllWithLevelEqualToNamespace_FileContainsNoDuplicates(string dll, NUnitTestExtractorApp.Level level)
        {
            List<string> dlls = new List<string>();
            dlls.Add(dll);

            InvokeGetTestsUsingFileOutput(dlls, level);

            List<string> lines = new List<string>(File.ReadAllLines(outputDirectory));

            Assert.That(lines.Count == lines.Distinct().Count());
        }

        [TestCase("NAMESPACE")]
        [TestCase("namespace")]
        [TestCase("NaMeSpAcE")]
        [Test]
        public void ParsingLevel_NamespaceWrittenInDifferentCases_WillParseWithoutError(string levelString)
        {
            Assert.That(NUnitTestExtractorApp.ParseLevel(levelString),Is.EqualTo(NUnitTestExtractorApp.Level.Namespace));
        }

        [TestCase("CLASS")]
        [TestCase("class")]
        [TestCase("ClAsS")]
        [Test]
        public void ParsingLevel_ClassWrittenInDifferentCases_WillParseWithoutError(string levelString)
        {
            Assert.That(NUnitTestExtractorApp.ParseLevel(levelString), Is.EqualTo(NUnitTestExtractorApp.Level.Class));
        }

        [TestCase("FUNCTION")]
        [TestCase("function")]
        [TestCase("FuNcTiOn")]
        [Test]
        public void ParsingLevel_FunctionWrittenInDifferentCases_WillParseWithoutError(string levelString)
        {
            Assert.That(NUnitTestExtractorApp.ParseLevel(levelString), Is.EqualTo(NUnitTestExtractorApp.Level.Function));
        }




    }
}

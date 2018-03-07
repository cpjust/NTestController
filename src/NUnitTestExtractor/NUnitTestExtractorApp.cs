using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using System.IO;
using System.Reflection;
using NUnit;
using System.Diagnostics;
using NUnit.Framework;

namespace NUnitTestExtractor
{
    public static class NUnitTestExtractorApp
    {
        private static Options _options = new Options();

        public enum Level { Namespace, Class, Function, TestCase };

        public static void Main(string[] args)
        {
            using (var parser = new CommandLine.Parser(with => with.HelpWriter = Console.Error))
            {
                if (parser.ParseArgumentsStrict(args, _options, () => Environment.Exit(-1)))
                {
                    if (_options.Help)
                    {
                        Console.WriteLine(CommandLine.Text.HelpText.AutoBuild(_options));

                        return;
                    }

                    // _options.Output is an abusolutely path of output file for the test name list
                    if (!string.IsNullOrEmpty(_options.Output))
                    {
                        FileInfo fi = new FileInfo(_options.Output);

                        // if the parent directory doesn't exist, then create it
                        if (!fi.Directory.Exists)
                        {
                            fi.Directory.Create();
                        }

                        // if the output file doesn't exist, then create it
                        if (!fi.Exists)
                        {
                            fi.Create();
                        }
                    }

                    if (_options.Level != null)
                    {
                        Level level = ParseLevel(_options.Level);

                        GetTests(_options.DLLs, level);
                    }
                }
            }
        }

        /// <summary>
        /// Parses string into proper Level
        /// </summary>
        /// <param name="levelToParse">The string to parse</param>
        /// <returns>The Level from parsing the string</returns>
        public static Level ParseLevel(string levelToParse)
        {
            try
            {
                return (Level)Enum.Parse(typeof(Level), levelToParse, ignoreCase: true);
            }
            catch
            {
                string errorMessage = string.Format("Incorrect argument - Level: {0}", levelToParse);

                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Uses reflection in order to search user submitted dlls to find either namespaces, classes, or functions which contain tests, 
        /// then uses WriteTestDllAndName() to  write data to either stdout or a txt file
        /// </summary>
        /// <param name="dlls">The dlls which are submitted by user</param>
        /// <param name="level">Specifies the level of granuality to use for the output: either namepspace, class or function</param>
        /// <param name="writer">The StreamWriter which will be passed to WriteTestDllAndName() </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom")]
        public static void GetTests(IList<string> dlls, Level level)
        {
            if (dlls == null || dlls.Count == 0)
            {
                throw new ArgumentNullException("dlls", "no dlls were specified");
            }

            foreach (string dll in dlls)
            {
                List<Type> testSuites = getValidTestSuites(dll);

                foreach (Type testSuite in testSuites)
                {
                    getValidTestsFromTestSuite(testSuite, level);
                }
            }
        }

        /// <summary>
        /// Get all valid test suites from the assembly
        /// </summary>
        /// <param name="assemblyFullPath">The full qualified path for the assembly</param>
        /// <returns>Return a list of test suites</returns>
        private static List<Type> getValidTestSuites(string assemblyFullPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyFullPath))
            {
                throw new ArgumentNullException("Assembly full path can't be NULL or Empty.");
            }

            Assembly assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(assemblyFullPath);
            }
            catch (FileNotFoundException)
            {
                string errorMessage = string.Format("File was not found: '{0}'", assemblyFullPath);

                throw new FileNotFoundException(errorMessage);
            }

            List<Type> testSuites = new List<Type>();
            foreach (var item in assembly.GetTypes())
            {
                if (item.IsClass
                    && item.IsPublic
                    && item.CustomAttributes.Any(x => x.AttributeType.Equals(typeof(TestFixtureAttribute)))
                    && !item.CustomAttributes.Any(x => x.AttributeType.Equals(typeof(ExplicitAttribute)))
                    && !item.CustomAttributes.Any(x => x.AttributeType.Equals(typeof(IgnoreAttribute))))
                {
                    testSuites.Add(item);
                }
            }

            return testSuites;
        }

        /// <summary>
        /// Get all valid tests from the suite
        /// </summary>
        /// <param name="testSuite"></param>
        /// <returns>Return a list of tests</returns>
        private static void getValidTestsFromTestSuite(Type testSuite, Level levelToDisplay)
        {
            HashSet<string> tests = new HashSet<string>();

            foreach (MethodInfo test in testSuite.GetMethods())
            {
                var attributes = test.GetCustomAttributesData();

                // search function attributes to find out whether the function has "Test" or "TestCase" attribute defined
                if (!attributes.Any(x => x.AttributeType.Equals(typeof(TestAttribute)))
                    && !attributes.Any(x => x.AttributeType.Equals(typeof(TestCaseAttribute))))
                {
                    continue;
                }

                // search the category attributes for the function
                var categories = attributes.Where(x => x.AttributeType.Equals(typeof(CategoryAttribute)));

                // match the "Exclude" category from the function category list
                // if the function has matched exclude category, we will skip this function
                if (categories.Any(x => x.ConstructorArguments.Any(y => y.Value.Equals(_options.ExcludeInfo))))
                {
                    continue;
                }

                // match the "Include" category from the function category list
                // if the function has matched include category, we will count this function as a valid test
                if (string.IsNullOrWhiteSpace(_options.IncludeInfo)
                    || categories.Any(x => x.ConstructorArguments.Any(y => y.Value.Equals(_options.IncludeInfo))))
                {
                    Console.WriteLine("Found the test and add it.");

                    formatOutputInfo(test, levelToDisplay);
                }
            }
        }

        private static void formatOutputInfo(MethodInfo test, Level levelToDisplay)
        {
            if (test == null)
            {
                string errorMessage = string.Format("Missing test information.");

                throw new Exception(errorMessage);
            }

            string outputInfo = string.Empty;

            switch (levelToDisplay)
            {
                case Level.Namespace:
                    //string a = test.DeclaringType.Module.Name;
                    //string b = test.DeclaringType.Module.FullyQualifiedName;
                    outputInfo = test.DeclaringType.GetTypeInfo().Namespace;
                    //string d = test.DeclaringType.GetTypeInfo().Name;
                    break;
                case Level.Class:
                    outputInfo = test.DeclaringType.GetTypeInfo().FullName;
                    break;
                case Level.Function:
                    outputInfo = test.DeclaringType.GetTypeInfo().Name + "." + test.Name;
                    break;
                case Level.TestCase:
                    outputInfo = generateTestCaseName(test);
                    break;
                default:
                    throw new Exception(string.Format("Invalid level option: {0}", levelToDisplay.ToString()));
            }
        }

        private static string generateTestCaseName(MethodInfo test)
        {
            string testCaseName = string.Empty;

            // under construction :)

            return testCaseName;
        }

        private static void modifyOutput(string outputInfo, MethodInfo test)
        {
            if (test == null)
            {
                throw new Exception("Missing test information.");
            }

            if (string.IsNullOrWhiteSpace(_options.Output))
            {
                Console.WriteLine(outputInfo);
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(_options.Output, append: true))
                {
                    string moduleFullQualifiedName = test.DeclaringType.Module.FullyQualifiedName;

                    writer.WriteLine("\"" + moduleFullQualifiedName + "\"" + " | " + outputInfo);
                }
            }
        }



        // whatever after this line will go away...

        /// <summary>
        /// Writes each DLL file path along with either a namespace, class or function which contains tests(dependent on level specified by user)
        /// </summary>
        /// <param name="writer"> StreamWriter used to write the dll path and data, either to a txt file or stdout </param>
        /// <param name="dll"> The dll path which is written </param>
        /// <param name="data"> The test information corrosponding to the dll file </param>
        private static void WriteTestDllAndName(TextWriter writer, string dll, string data)
        {
            writer.WriteLine("\"" + dll + "\"" + " | " + data);
        }

        /// <summary>
        /// Returns the properly formatted test string depending on the level specified. 
        /// </summary>
        /// <param name="level">Used in the switch statement to determine how much info to put into the data string</param>
        /// <param name="type">Used to get the namespace of the test</param>
        /// <param name="methodInfo">Used to get the test info such as declaring type and name</param>
        /// <param name="testCase">Used to get arguments of a testcase</param>
        /// <param name="data">The string which is modified and then returned</param>
        /// <returns>Returns properly formatted test string</returns>
        private static string formattedTest(Level level, Type type, MethodInfo methodInfo, TestCaseAttribute testCase, string data)
        {
            switch (level)
            {
                case Level.Namespace:
                    data = type.Namespace;
                    break;
                case Level.Class:
                    data = methodInfo.DeclaringType.ToString();
                    break;
                case Level.Function:
                    data = methodInfo.DeclaringType + "." + methodInfo.Name;
                    break;
                case Level.TestCase:
                    if (testCase != null)
                    {
                        data = FormattedTestCase(methodInfo.DeclaringType.ToString(), methodInfo.Name, testCase.Arguments, data);
                    }
                    break;
            }

            return data;
        }

        /// <summary>
        /// Depending on number of args within the TestCase, function will return properly formatted testcase
        /// </summary>
        /// <param name="declaringType">declaring type of testcase method(namespace.class)</param>
        /// <param name="name">name of testcase</param>
        /// <param name="testCaseArgs">the object array containing the testcase's arguments</param>
        /// <param name="data">the string which is appended to</param>
        /// <returns>the properly formatted TestCase string</returns>
        public static string FormattedTestCase(string declaringType, string name, object[] testCaseArgs, string data)
        {

            if (testCaseArgs != null && testCaseArgs.Length > 0)
            {
                data = declaringType + "." + name + "(";
                data = AppendArgumentsForTestCasesToString(testCaseArgs, data);
                data += ")";
            }
            else if (testCaseArgs != null && testCaseArgs.Length == 0)
            {
                data = declaringType + "." + name + "()";
            }

            return data;
        }

        /// <summary>
        /// Appends all of a TestCase's arguments to a string
        /// </summary>
        /// <param name="args">array of arguments to append to string</param>
        /// <param name="data">the string to add to and also return</param>
        /// <returns>all of the arguments in a string with proper formatting</returns>
        public static string AppendArgumentsForTestCasesToString(object[] args, string data)
        {
            if (args != null)
            {
                List<string> testArgs = new List<string>();

                foreach (object arg in args)
                {
                    string argString = arg as string;

                    if (argString != null)
                    {
                        testArgs.Add("\"" + arg + "\"");
                    }
                    else if (arg == null)
                    {
                        testArgs.Add("null");
                    }
                    else
                    {
                        testArgs.Add(arg.ToString());
                    }
                }
                data += string.Join(", ", testArgs);

                return data;
            }
            throw new ArgumentNullException("args", "no args were specified");
        }
    }
}

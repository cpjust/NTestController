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

        public enum Level { Namespace, Class, Function, TestCase, Null }

        static void Main(string[] args)
        {
            //Setting a default value
            Level level = Level.Null;

            using (var parser = new CommandLine.Parser(with => with.HelpWriter = Console.Error))
            {
                if (parser.ParseArgumentsStrict(args, _options, () => Environment.Exit(-1)))
                {
                    if (_options.Help)
                    {
                        Console.WriteLine(CommandLine.Text.HelpText.AutoBuild(_options));
                        return;
                    }
                   
                    if(!string.IsNullOrEmpty(_options.Output))
                    {
                        string directory = null;

                        int index = _options.Output.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);

                        if (index > 0)
                        {
                            directory = _options.Output.Substring(0, index);
                        }  

                        if (directory!=null && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        if (!File.Exists(_options.Output))
                        {
                            using (FileStream stream = File.Create(_options.Output)) { }
                        }
                    }

                    if (_options.Level != null)
                    {
                        StreamWriter writer = null;
                        level = ParseLevel(_options.Level);
                        try
                        {
                            if (_options.Output == null)
                            {
                                writer = new StreamWriter(Console.OpenStandardOutput());
                            }
                            else
                            {
                                writer = new StreamWriter(_options.Output, append: true);
                            }

                            GetTests(_options.DLLs, level, writer);
                        }
                        finally
                        {
                            if (writer != null)
                            {
                                writer.Dispose();
                                
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes each DLL file path along with either a namespace, class or function which contains tests(dependent on level specified by user)
        /// </summary>
        /// <param name="writer"> StreamWriter used to write the dll path and data, either to a txt file or stdout </param>
        /// <param name="dll"> The dll path which is written </param>
        /// <param name="data"> The test information corrosponding to the dll file </param>
        private static void WriteTestDllAndName(TextWriter writer, string dll, string data)
        {
             writer.WriteLine("\""+dll+"\"" + " | " + data);
             
        }

        /// <summary>
        /// Uses reflection in order to search user submitted dlls to find either namespaces, classes, or functions which contain tests, 
        /// then uses WriteTestDllAndName() to  write data to either stdout or a txt file
        /// </summary>
        /// <param name="dlls">The dlls which are submitted by user</param>
        /// <param name="level">Specifies the level of granuality to use for the output: either namepspace, class or function</param>
        /// <param name="writer">The StreamWriter which will be passed to WriteTestDllAndName() </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom")]
        public static void GetTests(IList<string> dlls, Level level, TextWriter writer)
        {
            List<string> testsWritten = new List<string>();

            List<string> categoryNames = new List<string>();

            if (dlls != null)
            {
                foreach (string dll in dlls)
                {
                    Assembly assembly = null;
                    try
                    {
                        assembly = Assembly.LoadFrom(dll);
                    }
                    catch (FileNotFoundException e)
                    {
                        Console.Error.WriteLine("{0}: file {1} was not found on system", e.GetType(), dll);
                        continue;
                    }

                    foreach (Type type in assembly.GetTypes())
                    {
                        // Skip Explicit or Ignored tests.
                        if (!type.IsPublic ||
                            type.CustomAttributes.Any(a => a.AttributeType.Equals(typeof(ExplicitAttribute))) ||
                            type.CustomAttributes.Any(a => a.AttributeType.Equals(typeof(IgnoreAttribute))))
                        {
                            continue;
                        }

                        foreach (MethodInfo methodInfo in type.GetMethods())
                        {
                            var attributes = methodInfo.GetCustomAttributes(true);

                            var categories = methodInfo.GetCustomAttributes(typeof(CategoryAttribute));
                            
                            foreach (var attr in attributes)
                            {
                                TestAttribute test = attr as TestAttribute;
                                TestCaseAttribute testCase = attr as TestCaseAttribute;

                                if (test != null || testCase != null)
                                {
                                    //adding categories from methods to list
                                    AddMethodCategoriesToList(categoryNames, categories);

                                    //adding categories from testcase into list
                                    if (testCase != null && testCase.Categories != null)
                                    {
                                        AddTestCaseCategoriesToList(categoryNames, testCase);
                                    }

                                    bool include = true;
                                    bool exclude = false;

                                    if (!string.IsNullOrEmpty(_options.IncludeInfo))
                                    {
                                        include = Include(categoryNames, _options.IncludeInfo);
                                    }

                                    if (!string.IsNullOrEmpty(_options.ExcludeInfo))
                                    {
                                        exclude = Exclude(categoryNames, _options.ExcludeInfo);
                                    }

                                    string data = string.Empty;

                                    //If exclude is true then move on to next method as this one has category marked as exluded
                                    if (exclude)
                                    {
                                        continue;
                                    }
                                    else if (include)
                                    {
                                        data = FormattedTest(level, type, methodInfo, testCase, data);

                                        //Prevent duplicate entries from being written
                                        if (!string.IsNullOrEmpty(data) && !testsWritten.Contains(data))
                                        {
                                            WriteTestDllAndName(writer, dll, data);
                                            testsWritten.Add(data);
                                        }
                                    }
                                }
                            }
                            //reset categoryNames for next method
                            categoryNames = new List<string>();
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentNullException("dlls", "no dlls were specified");
            }
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
        private static string FormattedTest(Level level, Type type, MethodInfo methodInfo, TestCaseAttribute testCase, string data)
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
        /// Add testCase categories to categoryNames 
        /// </summary>
        /// <param name="categoryNames">list to add to</param>
        /// <param name="testCase">testCase used to get arguments from</param>
        private static void AddTestCaseCategoriesToList(List<string> categoryNames, TestCaseAttribute testCase)
        {
            foreach (string category in testCase.Categories)
            {
                categoryNames.Add(category);
            }
        }

        /// <summary>
        /// Add method categories to list
        /// </summary>
        /// <param name="categoryNames">list to add to</param>
        /// <param name="categories">categories to add to list</param>
        private static void AddMethodCategoriesToList(List<string> categoryNames, IEnumerable<Attribute> categories)
        {
            foreach (var category in categories)
            {
                CategoryAttribute categoryAttr = (CategoryAttribute)category;

                categoryNames.Add(categoryAttr.Name);
            }
        }

        /// <summary>
        /// Used to determine whether or not the current test/testcase should be included 
        /// </summary>
        /// <param name="categoryNames">List of categories to compare info to</param>
        /// <param name="includeInfo">string of info used to compare to categoryNames</param>
        /// <returns>returns false if includeInfo is inside of the categoryNames List, true otherwise</returns>
        public static bool Include(IReadOnlyList<string> categoryNames, string includeInfo)
        {
            if (!string.IsNullOrEmpty(includeInfo))
            {
                //Remove quotes from string in order to properly process it
                includeInfo = includeInfo.Replace("\"", "");

                List<string> includeInfoValues = new List<string>(includeInfo.Split(','));
                
                foreach (string value in includeInfoValues)
                {
                    if (categoryNames != null && !categoryNames.Contains(value))
                    {
                        return false;
                    }
                }
                return true;
            }
            throw new ArgumentNullException("includeInfo", "no includeInfo specified");

        }

        /// <summary>
        /// Used to determine whether or not a test/testcase should be excluded or not
        /// </summary>
        /// <param name="categoryNames">List of categories to compare info to</param>
        /// <param name="excludeInfo">string of info used to compare to categoryNames</param>
        /// <returns>returns true if excludeInfo is inside of the categoryNames List, false otherwise</returns>
        public static bool Exclude(IReadOnlyList<string> categoryNames, string excludeInfo)
        {
            if (!string.IsNullOrEmpty(excludeInfo))
            {
                excludeInfo = excludeInfo.Replace("\"", "");

                List<string> excludeInfoValues = new List<string>(excludeInfo.Split(','));

                foreach (string value in excludeInfoValues)
                {
                    if (categoryNames != null && categoryNames.Contains(value))
                    {
                        return true;
                    }
                }
                return false;
            }
            throw new ArgumentNullException("excludeInfo", "no excludeInfo specified");
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
        /// Parses string into proper Level
        /// </summary>
        /// <param name="levelToParse">The string to parse</param>
        /// <returns>The Level from parsing the string</returns>
        public static Level ParseLevel(string levelToParse)
        {
            Level level = Level.Null;
            try
            {
                level = (Level)Enum.Parse(typeof(Level), levelToParse, ignoreCase: true);
            }
            catch(ArgumentException e)
            {
                Console.Error.WriteLine(e.GetType() + ": Please enter either Namespace, Class, or Function for level");
                Environment.Exit(-1);
            }
            return level;
        }

        /// <summary>
        /// Appends all of a TestCase's arguments to a string
        /// </summary>
        /// <param name="args">array of arguments to append to string</param>
        /// <param name="data">the string to add to and also return</param>
        /// <returns>all of the arguments in a string with proper formatting</returns>
        public static string AppendArgumentsForTestCasesToString(object[] args, string data)
        {
            if(args != null)
            {
                List<string> testArgs = new List<string>();
                 
                foreach(object arg in args)
                {
                    string argString = arg as string;

                    if(argString != null)
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

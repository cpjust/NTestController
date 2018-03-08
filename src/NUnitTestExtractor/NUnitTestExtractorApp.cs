using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

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

                    // _options.Output is an abusolute path of output file for the test name list
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

                    foreach (var assembly in _options.DLLs)
                    {
                        GetInfoFromAssembly(assembly);
                    }
                }
            }
        }

        public static string ScopeLevel
        {
            get
            {
                return _options.Level;
            }
            set
            {
                _options.Level = value;
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
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "Incorrect argument - Level: {0}", levelToParse);

                throw new ArgumentException(errorMessage);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblyFullPath"></param>
        /// <returns></returns>
        public static HashSet<string> GetInfoFromAssembly(string assemblyFullPath)
        {
            HashSet<string> rv = new HashSet<string>();

            foreach (var item in getValidTestSuiteTypesFromAssembly(assemblyFullPath))
            {
                Level lv = ParseLevel(_options.Level);

                switch (lv)
                {
                    case Level.Namespace:
                        rv.Add(item.Namespace);
                        break;
                    case Level.Class:
                        rv.Add(item.FullName);
                        break;
                    case Level.Function:
                        rv.IntersectWith(getValidTestsFromTestSuite(item));
                        break;
                    case Level.TestCase:
                        break;
                    default:
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Invalid level option: {0}", lv.ToString()));
                }
            }

            return rv;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblyFullPath"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom")]
        private static Assembly loadAssembly(string assemblyFullPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyFullPath))
            {
                throw new ArgumentNullException("assemblyFullPath", "can't be NULL or Empty.");
            }

            Assembly assembly = null;

            try
            {
                assembly = Assembly.LoadFrom(assemblyFullPath);
            }
            catch (FileNotFoundException)
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "File was not found: '{0}'", assemblyFullPath);

                throw new FileNotFoundException(errorMessage);
            }

            return assembly;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool isValidTestSuite(Type type)
        {
            return type.IsClass
                && type.IsPublic
                && type.CustomAttributes.Any(x => x.AttributeType.Equals(typeof(TestFixtureAttribute)))
                && !type.CustomAttributes.Any(x => x.AttributeType.Equals(typeof(ExplicitAttribute)))
                && !type.CustomAttributes.Any(x => x.AttributeType.Equals(typeof(IgnoreAttribute)));
        }

        /// <summary>
        /// Get all valid test suites from the assembly
        /// </summary>
        /// <param name="assemblyFullPath">The full qualified path for the assembly</param>
        /// <returns>Return a list of test suites</returns>
        private static HashSet<Type> getValidTestSuiteTypesFromAssembly(string assemblyFullPath)
        {
            HashSet<Type> testSuites = new HashSet<Type>();

            foreach (var item in loadAssembly(assemblyFullPath).GetTypes())
            {
                if (isValidTestSuite(item))
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
        private static HashSet<string> getValidTestsFromTestSuite(Type testSuite)
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

                    tests.Add(test.DeclaringType.GetTypeInfo().Name + "." + test.Name);
                }
            }

            return tests;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputInfo"></param>
        /// <param name="test"></param>
        private static void modifyOutput(string outputInfo, MethodInfo test)
        {
            if (test == null)
            {
                throw new ArgumentNullException("test", "can't be NULL or Empty.");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        private static string generateTestCaseName(MethodInfo test)
        {
            string testCaseName = string.Empty;

            Console.WriteLine(test.Name);

            // under construction :)

            return testCaseName;
        }
    }
}

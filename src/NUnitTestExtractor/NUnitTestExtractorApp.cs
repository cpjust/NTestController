using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

using Utilities;

using NUnit.Framework;

namespace NUnitTestExtractor
{
    public static class NUnitTestExtractorApp
    {
        private static Options _options = new Options();

        private static Assembly _assembly = null;

        public enum Level { Namespace, Class, Function, TestCase };

        public static void Main(string[] args)
        {
            using (var parser = new CommandLine.Parser(with => with.HelpWriter = Console.Error))
            {
                if (parser.ParseArgumentsStrict(args, _options, () => Environment.Exit(-1)))
                {
                    try
                    {
                        if (_options.Help)
                        {
                            Console.WriteLine(CommandLine.Text.HelpText.AutoBuild(_options));

                            Environment.Exit(0);
                        }

                        // _options.Output is an abusolute path of output file for the test name list
                        if (!string.IsNullOrEmpty(_options.Output))
                        {
                            CreateOutputFile();
                        }

                        foreach (var assembly in _options.DLLs)
                        {
                            GenerateInfoFromAssembly(assembly);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(StringUtils.FormatInvariant("Failed to process: {0}", e.InnerException));

                        Environment.Exit(-2);
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
            ThrowIf.ArgumentNull(levelToParse, nameof(levelToParse));

            return (Level)Enum.Parse(typeof(Level), levelToParse, ignoreCase: true);
        }

        /// <summary>
        /// Create the output file's parent directory if it is necessary
        /// </summary>
        private static void CreateOutputFile()
        {
            FileInfo fi = new FileInfo(_options.Output);

            // if the parent directory doesn't exist, then create it
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
        }

        /// <summary>
        /// Get a list of namespaces/classes/functions/testcases from the assembly
        /// </summary>
        /// <param name="assemblyFullPath"></param>
        /// <returns></returns>
        public static void GenerateInfoFromAssembly(string assemblyFullPath)
        {
            HashSet<string> rv = new HashSet<string>();

            foreach (var item in GetValidTestSuiteTypesFromAssembly(assemblyFullPath))
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
                        rv.UnionWith(GetValidTestsFromTestSuite(item));
                        break;
                    case Level.TestCase:
                        break;
                    default:
                        throw new ArgumentException(StringUtils.FormatInvariant("Invalid level option: {0}", lv.ToString()));
                }
            }

            OutputInfo(rv);
        }

        /// <summary>
        /// Load assembly by passing its path
        /// </summary>
        /// <param name="assemblyFullPath"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom")]
        private static void LoadAssembly(string assemblyFullPath)
        {
            ThrowIf.StringIsNullOrWhiteSpace(assemblyFullPath, nameof(assemblyFullPath));

            try
            {
                _assembly = Assembly.LoadFrom(assemblyFullPath);
            }
            catch (FileNotFoundException)
            {
                string errorMessage = StringUtils.FormatInvariant("File was not found: '{0}'", assemblyFullPath);

                throw new FileNotFoundException(errorMessage);
            }
        }

        /// <summary>
        /// Validate whether the type object is a valid test suite
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsValidTestSuite(Type type)
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
        private static HashSet<Type> GetValidTestSuiteTypesFromAssembly(string assemblyFullPath)
        {
            HashSet<Type> testSuites = new HashSet<Type>();

            LoadAssembly(assemblyFullPath);

            foreach (var item in _assembly.GetTypes())
            {
                if (IsValidTestSuite(item))
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
        private static HashSet<string> GetValidTestsFromTestSuite(Type testSuite)
        {
            HashSet<string> tests = new HashSet<string>();

            foreach (MethodInfo test in testSuite.GetMethods())
            {
                var attributes = test.GetCustomAttributesData();

                // search function attributes to find out whether the function has "Test" or "TestCase" attribute defined
                // if no matches, skip this function
                // search function attributes to find out whether the function has "Explicit" or "Ingored" attribute defined
                // if found any attribute related to "Explicit" or "Ingored", skip this function
                if (!attributes.Any(x => x.AttributeType.Equals(typeof(TestAttribute))
                                    && x.AttributeType.Equals(typeof(TestCaseAttribute)))
                                    || attributes.Any(x => x.AttributeType.Equals(typeof(ExplicitAttribute)))
                                    || attributes.Any(x => x.AttributeType.Equals(typeof(IgnoreAttribute))))
                {
                    continue;
                }

                // search the category attributes for the function
                var categories = attributes.Where(x => x.AttributeType.Equals(typeof(CategoryAttribute)));

                // match the "Exclude" category from the function category attibute list
                // if the function has matched exclude category, we will skip this function
                if (categories.Any(x => x.ConstructorArguments.Any(y => y.Value.Equals(_options.ExcludeInfo))))
                {
                    continue;
                }

                // match the "Include" category from the function category attribute list
                // if the function has matched include category, we will count this function as a valid test
                if (string.IsNullOrWhiteSpace(_options.IncludeInfo)
                    || categories.Any(x => x.ConstructorArguments.Any(y => y.Value.Equals(_options.IncludeInfo))))
                {
                    tests.Add(test.DeclaringType.GetTypeInfo().FullName + "." + test.Name);
                }
            }

            return tests;
        }

        /// <summary>
        /// Create the output file if it doesn't exist, or append it if it exists
        /// </summary>
        /// <param name="outputInfo"></param>
        /// <param name="type"></param>
        private static void OutputInfo(HashSet<string> outputInfoList)
        {
            ThrowIf.ArgumentNull(outputInfoList, nameof(outputInfoList));

            foreach (var outputInfo in outputInfoList)
            {
                string finalOutput = StringUtils.FormatInvariant("\"" + _assembly.Location + "\"" + " | " + outputInfo);

                if (string.IsNullOrWhiteSpace(_options.Output))
                {
                    Console.WriteLine(finalOutput);
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(_options.Output, append: true))
                    {
                        writer.WriteLine(finalOutput);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        //private static string generateTestCaseName(MethodInfo test)
        //{
        //    string testCaseName = string.Empty;

        //    Console.WriteLine(test.Name);

        //    // under construction :)

        //    return testCaseName;
        //}
    }
}

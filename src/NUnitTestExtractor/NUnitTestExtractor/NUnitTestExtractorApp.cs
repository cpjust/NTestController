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

namespace NUnitTestExtractor
{
    public static class NUnitTestExtractorApp
    {
        private static Options _options = new Options();

        public enum Level { Namespace, Class, Function, Null }

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
                   
                    if(_options.Output != null)
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
        private static void WriteTestDllAndName(StreamWriter writer, string dll, string data)
        {
            writer.Write(dll + " | " + data + Environment.NewLine);
        }

        /// <summary>
        /// Uses reflection in order to search user submitted dlls to find either namespaces, classes, or functions which contain tests, then uses WriteTestDllAndName() to
        /// write data to either stdout or a txt file
        /// </summary>
        /// <param name="dlls">The dlls which are submitted by user</param>
        /// <param name="level">Specifies the level of granuality to use for the output: either namepspace, class or function</param>
        /// <param name="writer">The StreamWriter which will be passed to WriteTestDllAndName() </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom")]
        public static void GetTests(IList<string> dlls, Level level, StreamWriter writer)
        {
            List<string> testsWritten = new List<string>();

            foreach(string dll in dlls)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.LoadFrom(dll);

                }catch(FileNotFoundException e)
                {
                    Console.Error.WriteLine("{0}: file {1} was not found on system", e.GetType(), dll);
                    continue;
                }
               
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (MethodInfo methodInfo in type.GetMethods())
                    {
                        var attributes = methodInfo.GetCustomAttributes(true);

                        foreach (var attr in attributes)
                        {
                            if (attr is NUnit.Framework.TestAttribute || attr is NUnit.Framework.TestCaseAttribute)
                            {
                                string data = "";
                                
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
                                }

                                //Prevent duplicate entries from being written
                                if (!testsWritten.Contains(data))
                                {
                                    WriteTestDllAndName(writer, dll, data);
                                    testsWritten.Add(data);
                                }
                            }
                        }
                    }
                }      
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static Level ParseLevel(string levelString)
        {
            Level level = Level.Null;
            try
            {
                level = (Level)Enum.Parse(typeof(Level), levelString, ignoreCase: true);
            }
            catch(ArgumentException e)
            {
                Console.Error.WriteLine(e.GetType() + ": Please enter either Namespace, Class, or Function for level");
                Environment.Exit(-1);
            }
            return level;
        }
    }
}

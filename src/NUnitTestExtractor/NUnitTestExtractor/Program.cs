using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using System.IO;
using System.Reflection;
using NUnit;

namespace NUnitTestExtractor
{
    class Program
    {
        private static Options _options = new Options();

        private enum Level { Namespace, Class, Function, Null }

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
                        if (!Directory.Exists(_options.Output))
                        {
                            Directory.CreateDirectory(_options.Output);
                        }
                    }

                    if (_options.Level != null)
                    {
                        try
                        {
                            level = (Level)Enum.Parse(typeof(Level), _options.Level, true);

                            GetTests(_options.DLLs, level);
                        }
                        catch (ArgumentException e)
                        {
                            Console.Error.WriteLine(e.GetType() + ": Please enter either Namespace, Class, or Function for level");
                            Environment.Exit(-1);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Writes each DLL file path along with either a namespace, class or function which contains tests(dependent on level specified by user)
        /// </summary>
        /// <param name="name"> the name of the txt file that data is being appended to </param>
        /// <param name="outputDirectory"> the directory in that the file is being saved to </param>
        /// <param name="dllDirectories"> list of all dll directories which the user wishes to append to a file or stdout </param>
        /// <param name="level"> Specifies the level of granuality to use for the output tests entered by user. </param>
        private static void WriteTestDllAndName(StreamWriter writer, string dll, string data)
        {
            using (writer)
            {
                writer.Write(dll + "|" + data + Environment.NewLine);
            }           
        }
        /// <summary>
        /// Uses reflection in order to search user submitted dlls to find either namespaces, classes, or functions which contain tests, then uses WriteTestDllAndName() to
        /// write data to either stdout or a txt file
        /// </summary>
        /// <param name="dlls">The dlls which are submitted by user</param>
        /// <param name="level">Specifies the level of granuality to use for the output: either namepspace, class or function</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom")]
        private static void GetTests(IList<string> dlls, Level level)
        {
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
                            if (attr is NUnit.Framework.TestAttribute)
                            {                               
                                string value = "";
                                
                                switch (level)
                                {
                                    case Level.Namespace:
                                        value = type.Namespace;
                                        break;
                                    case Level.Class:
                                        value = methodInfo.DeclaringType.ToString();
                                        break;
                                    case Level.Function:
                                        string method = methodInfo.ToString().Substring(methodInfo.ToString().IndexOf(" ", StringComparison.OrdinalIgnoreCase) + 1);
                                        value = methodInfo.DeclaringType + "." + method;
                                        break;
                                }
                                StreamWriter writer;
                                if (_options.Output== null)
                                {
                                    writer = new StreamWriter(Console.OpenStandardOutput());
                                }
                                else
                                {
                                    writer = new StreamWriter(_options.Output+ "file.txt", true);
                                }
                                WriteTestDllAndName(writer, dll, value);
                            }
                        }
                    }
                }      
            }
        }
    }
}

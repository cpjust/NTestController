using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using System.IO;


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
                            WriteTestDllAndName("file", _options.Output, _options.DLLs, level);
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
        /// While reflection is not implemented method writes each dll verified to be existing to either a txt file or stdout along with the specified level
        /// </summary>
        /// <param name="name"> the name of the txt file that data is being appended to </param>
        /// <param name="outputDirectory"> the directory in that the file is being saved to </param>
        /// <param name="dllDirectories"> list of all dll directories which the user wishes to append to a file or stdout </param>
        /// <param name="level"> Specifies the level of granuality to use for the output tests entered by user </param>
        private static void WriteTestDllAndName(string name, string outputDirectory, IList<string> dllDirectories, Level level)
        {
            //user entered some dlls
            if (dllDirectories.Count != 0)
            {
                StreamWriter writer;

                if (outputDirectory == null)
                {
                    writer = new StreamWriter(Console.OpenStandardOutput());
                }
                else
                {
                    writer = new StreamWriter(outputDirectory + name +".txt", true);
                }
                foreach (string dll in dllDirectories)
                {
                    if (File.Exists(dll))
                    {
                        writer.Write("{0}|{1}", dll, level + Environment.NewLine);
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: Unable to append {0} as it cannot be found on the system", dll);
                    }
                }
                writer.Close();
            }
            else
            {
                Console.Error.WriteLine("Error: no dlls specified");
                Environment.Exit(-2);
            }
        }
    }
}

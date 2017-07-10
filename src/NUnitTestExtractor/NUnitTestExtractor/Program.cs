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
                            Console.WriteLine("created directory at {0}", _options.Output);
                        }
                    }

                    if (_options.Level != null)
                    {
                        try
                        {
                            level = (Level)Enum.Parse(typeof(Level), _options.Level, true);
                            WriteToTxtFile("file", _options.Output, _options.DLLs, level);
                        }
                        catch (ArgumentException e)
                        {
                            Console.WriteLine("ArgumentException: Please enter either Namespace, Class, or Function for level");
                        }
                    }
                }
            }
        }
        //While reflection is not implemented this method writes path of dll if it exists with specified level to txt file
        private static void WriteToTxtFile(string name, string outputDirectory, IList<string> dllDirectories, Level level)
        {
            //user entered some dlls
            if (dllDirectories.Count != 0)
            {
                if (outputDirectory == null)
                {
                    Console.WriteLine("No output directory specified, writing to stdout");
                    foreach (string dll in dllDirectories)
                    {
                        if (File.Exists(dll))
                        {
                            Console.Out.WriteLine("{0}|{1}", dll, level);
                        }
                        else
                        {
                            Console.Out.WriteLine("Error: {0} file cannot be found on system", dll);
                        }
                    }
                }
                else
                {
                    foreach (string dll in dllDirectories)
                    {
                        if (File.Exists(dll))
                        {
                            File.AppendAllText(outputDirectory + name + ".txt", dll + "|" + level + Environment.NewLine);
                        }
                        else
                        {
                            Console.WriteLine("Error: {0} file cannot be found on system", dll);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Error: no dlls specified");
            }
        }
    }
}

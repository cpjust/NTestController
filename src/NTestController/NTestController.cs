using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Logger;
using NTestController.Factories;

[assembly: CLSCompliant(true)]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
namespace NTestController
{
    public static class NTestControllerApp
    {
        #region Member variables

        private static Options _options = new Options();
        private static ILogger Log { get; } = ConsoleLogger.Instance;

        #endregion Member variables

        #region Public functions

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {
            using (var parser = new CommandLine.Parser(with => with.HelpWriter = Console.Error))
            {
                if (parser.ParseArgumentsStrict(args, _options, () => Environment.Exit(-1)))
                {
                    // Set logging level (default = INFO).
                    if (_options.VerboseLevel != null)
                    {
                        VerboseLevel logLevel;

                        if (Enum.TryParse<VerboseLevel>(_options.VerboseLevel, ignoreCase: true, result: out logLevel))
                        {
                            Log.LogLevel = logLevel;
                        }
                        else
                        {
                            Log.LogLevel = VerboseLevel.INFO;
                        }
                    }

                    // Delete & re-create output directory.
                    if (Directory.Exists(_options.OutputDirectory))
                    {
                        Directory.Delete(_options.OutputDirectory, recursive: true);
                    }

                    Directory.CreateDirectory(_options.OutputDirectory);

                    // Load plugins.
                    Dictionary<PluginType, IPlugin> plugins = GetPlugins(_options.ConfigFile);

                    // Execute Reader plugin.
                    plugins[PluginType.TestReader].Execute();

                    // Execute Setup plugin.

                    // Execute Test Executor plugin.

                    // Execute Cleanup plugin.

                    // Execute Reporter plugin.
                }
            }
        }

        #endregion Public functions

        #region Private functions

        private static Dictionary<PluginType, IPlugin> GetPlugins(string configFile)
        {
            try
            {
                var XMLDoc = new XmlDocument();
                XMLDoc.Load(configFile);

                XmlNode pluginsNode = XMLDoc.FirstChild.SelectSingleNode("plugins");
                var plugins = new Dictionary<PluginType, IPlugin>();

                foreach (XmlNode node in pluginsNode)
                {
                    IPlugin plugin = PluginFactory.GetPlugin(node, configFile);
                    plugins.Add(plugin.PluginType, plugin);
                }

                return plugins;
            }
            catch (Exception e)
            {
                Log.WriteError("\n{0}\n\n{1}", e.Message, _options.GetUsage());
                throw;
            }
        }

        private static XmlNode GetDefaultsXmlNode(string configFile)
        {
            try
            {
                var XMLDoc = new XmlDocument();
                XMLDoc.Load(configFile);

                XmlNode defaultsNode = XMLDoc.FirstChild.SelectSingleNode("defaults");
                return defaultsNode;
            }
            catch (Exception e)
            {
                Log.WriteError("\n{0}\n\n{1}", e.Message, _options.GetUsage());
                throw;
            }
        }

        private static IList<IPlatform> GetPlatforms(string configFile, XmlNode defaultsNode)
        {
            var platforms = new List<IPlatform>();

            try
            {
                var XMLDoc = new XmlDocument();
                XMLDoc.Load(configFile);

                XmlNode node = XMLDoc.FirstChild;

                while (node != null)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        XmlNode platformNode = XMLDoc.FirstChild.SelectSingleNode("platform");
                        IPlatform platform = PlatformFactory.CreatePlatform(platformNode, defaultsNode);
                        platforms.Add(platform);
                    }

                    node = node.NextSibling;
                }
            }
            catch (Exception e)
            {
                Log.WriteError("\n{0}\n", e.Message);
                throw;
            }

            return platforms;
        }

        #endregion Private functions
    }
}


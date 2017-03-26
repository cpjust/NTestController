using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Logger;
using NTestController.Factories;
using Utilities;

[assembly: CLSCompliant(true)]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
namespace NTestController
{
    public static class NTestControllerApp
    {
        #region Member variables

        private static Options _options = new Options();
        private static Dictionary<PluginType, IPlugin> _plugins;

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
                    _plugins = GetPlugins(_options.ConfigFile);

                    var xmlDoc = XmlUtils.LoadXmlDocument(_options.ConfigFile);
                    var defaultsNode = GetDefaultsXmlNode(xmlDoc);
                    var platforms = GetPlatforms(xmlDoc, defaultsNode);

                    // Execute Reader plugin.
                    var readerPlugin = _plugins[PluginType.TestReader] as IReaderPlugin;
                    readerPlugin.TestInputFile = _options.TestFile;
                    readerPlugin.Execute();

                    // Add all tests to queue.
                    var testQueue = new TestQueue();

                    foreach (var test in readerPlugin.Tests)
                    {
                        testQueue.EnqueueTestToRun(test);
                    }

                    // Execute Setup plugin.

                    // Execute Test Executor plugin.
                    var executorPlugin = _plugins[PluginType.TestExecutor] as IExecutorPlugin;
                    executorPlugin.Options = _options;
                    executorPlugin.TestQueue = testQueue;
                    ExecuteTests(executorPlugin, platforms);

                    // Execute Cleanup plugin.

                    // Execute Reporter plugin.
                    var reporterPlugin = _plugins[PluginType.TestReporter] as IReporterPlugin;
                    reporterPlugin.TestQueue = testQueue;
                    reporterPlugin.Execute();
                }
            }
        }

        #endregion Public functions

        #region Private functions

        private static void ExecuteTests(IExecutorPlugin executorPlugin, List<IPlatform> platforms)
        {
            ThrowIf.ArgumentNull(executorPlugin, nameof(executorPlugin));
            ThrowIf.ArgumentNull(platforms, nameof(platforms));

            // Hack: For now just use a single Platform.  TODO: Use all Platforms later.
            var firstPlatform = platforms[0];
            var plugins = new List<IExecutorPlugin> { executorPlugin };

            executorPlugin.Computer = firstPlatform.Computers[0];

            // We already have one plugin, so add 1 less than Computers.Count.
            for (int i = 1; i < firstPlatform.Computers.Count; ++i)
            {
                var newPlugin = executorPlugin.ClonePlugin();
                newPlugin.Computer = firstPlatform.Computers[i];

                plugins.Add(newPlugin);
            }

            // Run all plugins in parallel.
            Parallel.ForEach(plugins, plugin =>
            {
                plugin.Execute();
            });
        }

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
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        IPlugin plugin = PluginFactory.GetPlugin(node, configFile);
                        plugins.Add(plugin.PluginType, plugin);
                    }
                }

                return plugins;
            }
            catch (Exception e)
            {
                Log.WriteError("\n{0}\n\n{1}", e.Message, _options.GetUsage());
                throw;
            }
        }

        

        private static XmlNode GetDefaultsXmlNode(XmlDocument xmlDoc)
        {
            try
            {
                XmlNode defaultsNode = xmlDoc.FirstChild.SelectSingleNode("defaults");
                return defaultsNode;
            }
            catch (Exception e)
            {
                Log.WriteError("\nFailed to find the 'defaults' XML node!\n{0}", e.Message);
                throw;
            }
        }

        private static List<IPlatform> GetPlatforms(XmlDocument xmlDoc, XmlNode defaultsNode)
        {
            var platforms = new List<IPlatform>();

            try
            {
                var computerFactory = _plugins.Values.First().ComputerFactory;
                var platformFactory = _plugins.Values.First().PlatformFactory;

                var node = xmlDoc.FirstChild;

                while (node != null)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        var platformNode = xmlDoc.FirstChild.SelectSingleNode("platform");
                        var platform = platformFactory.CreatePlatform(platformNode, defaultsNode, computerFactory);
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


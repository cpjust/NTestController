using System;
using System.Collections.Generic;

namespace NTestController
{
    public enum PluginType
    {
        TestReader,
        EnvSetup,
        TestExecutor,
        EnvCleanup,
        TestReporter
    }

    public interface IPlugin
    {
        string Name { get; }
        PluginType PluginType { get; }

        /// <summary>
        /// Execute this plugin instance.
        /// </summary>
        bool Execute();
    }

    public interface IReaderPlugin : IPlugin
    {
        List<Test> Tests { get; }
    }

    public interface IReporterPlugin : IPlugin
    {
        TestQueue TestQueue { get; set; }
    }

    public interface IExecutorPlugin : IPlugin
    {
        IComputer Computer { get; set; }
        TestQueue TestQueue { get; set; }

        /// <summary>
        /// Clones this plugin.
        /// </summary>
        /// <returns>A copy of this plugin.</returns>
        IExecutorPlugin ClonePlugin();
    }

    public interface IPluginFactory
    {
        /// <summary>
        /// Gets the plugin.
        /// </summary>
        /// <param name="xmlConfig">The NTestController.xml config path and filename.</param>
        /// <returns>The plugin.</returns>
        IPlugin GetPlugin(string xmlConfig);
    }
}


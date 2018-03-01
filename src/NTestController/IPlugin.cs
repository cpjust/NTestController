using System;
using System.Collections.Generic;
using NTestController.Factories;

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
        IComputerFactory ComputerFactory { get; }
        IPlatformFactory PlatformFactory { get; }

        /// <summary>
        /// Execute this plugin instance.
        /// </summary>
        bool Execute();
    }

    public interface IReaderPlugin : IPlugin
    {
        string TestInputFile { get; set; }
        IList<Test> Tests { get; }
    }

    public interface IReporterPlugin : IPlugin
    {
        TestQueue TestQueue { get; set; }
    }

    public interface IExecutorPlugin : IPlugin
    {
        IReadOnlyOptions Options { get; set; }
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


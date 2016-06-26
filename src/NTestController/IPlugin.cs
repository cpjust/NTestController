using System;

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


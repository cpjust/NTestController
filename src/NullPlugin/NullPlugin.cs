using System;
using NTestController;
using Utilities;

namespace NullPlugin
{
    public class NullPluginFactory : IPluginFactory
    {
        public IPlugin GetPlugin(string xmlConfig)
        {
            IPlugin plugin = null;

            switch (xmlConfig)
            {
                case "TestReader":
                    plugin = new NullTestReaderPlugin(xmlConfig);
                    break;
                case "EnvSetup":
                    plugin = new NullEnvSetupPlugin(xmlConfig);
                    break;
                case "TestExecutor":
                    plugin = new NullTestExecutorPlugin(xmlConfig);
                    break;
                case "EnvCleanup":
                    plugin = new NullEnvCleanupPlugin(xmlConfig);
                    break;
                case "TestReporter":
                    plugin = new NullTestReporterPlugin(xmlConfig);
                    break;
                default:
                    plugin = new NullTestReaderPlugin(xmlConfig);
                    break;
            }

            return plugin;
        }
    }

    public abstract class NullPluginBase : IPlugin
    {
        protected string _name = null;
        protected PluginType _pluginType;
        private string _xmlConfig = null;

        public NullPluginBase(string xmlConfig)
        {
            _xmlConfig = xmlConfig;
        }

        #region Implements IPlugin

        public string Name { get; }
        public PluginType PluginType { get; }

        public bool Execute()
        {
            return (_xmlConfig != null);
        }

        #endregion Implements IPlugin
    }

    public class NullTestReaderPlugin : NullPluginBase
    {
        public NullTestReaderPlugin(string xmlConfig)
            : base(xmlConfig)
        {
            _name = "TestReader";
            _pluginType = PluginType.TestReader;
        }
    }

    public class NullEnvSetupPlugin : NullPluginBase
    {
        public NullEnvSetupPlugin(string xmlConfig)
            : base(xmlConfig)
        {
            _name = "EnvSetup";
            _pluginType = PluginType.EnvSetup;
        }
    }

    public class NullTestExecutorPlugin : NullPluginBase
    {
        public NullTestExecutorPlugin(string xmlConfig)
            : base(xmlConfig)
        {
            _name = "TestExecutor";
            _pluginType = PluginType.TestExecutor;
        }
    }

    public class NullEnvCleanupPlugin : NullPluginBase
    {
        public NullEnvCleanupPlugin(string xmlConfig)
            : base(xmlConfig)
        {
            _name = "EnvCleanup";
            _pluginType = PluginType.EnvCleanup;
        }
    }

    public class NullTestReporterPlugin : NullPluginBase
    {
        public NullTestReporterPlugin(string xmlConfig)
            : base(xmlConfig)
        {
            _name = "TestReporter";
            _pluginType = PluginType.TestReporter;
        }
    }
}


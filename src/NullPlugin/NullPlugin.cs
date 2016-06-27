using NTestController;

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
        private string _xmlConfig = null;

        protected NullPluginBase(string xmlConfig)
        {
            _xmlConfig = xmlConfig;
        }

        #region Implements IPlugin

        public string Name { get; internal set; }
        public PluginType PluginType { get; internal set; }

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
            Name = "TestReader";
            PluginType = PluginType.TestReader;
        }
    }

    public class NullEnvSetupPlugin : NullPluginBase
    {
        public NullEnvSetupPlugin(string xmlConfig)
            : base(xmlConfig)
        {
            Name = "EnvSetup";
            PluginType = PluginType.EnvSetup;
        }
    }

    public class NullTestExecutorPlugin : NullPluginBase
    {
        public NullTestExecutorPlugin(string xmlConfig)
            : base(xmlConfig)
        {
            Name = "TestExecutor";
            PluginType = PluginType.TestExecutor;
        }
    }

    public class NullEnvCleanupPlugin : NullPluginBase
    {
        public NullEnvCleanupPlugin(string xmlConfig)
            : base(xmlConfig)
        {
            Name = "EnvCleanup";
            PluginType = PluginType.EnvCleanup;
        }
    }

    public class NullTestReporterPlugin : NullPluginBase
    {
        public NullTestReporterPlugin(string xmlConfig)
            : base(xmlConfig)
        {
            Name = "TestReporter";
            PluginType = PluginType.TestReporter;
        }
    }
}


using NTestController;

namespace NUnitReporter
{
    public class PluginFactory : IPluginFactory
    {
        public IPlugin GetPlugin(string xmlConfig)
        {
            return new NUnitReporterPlugin(xmlConfig);
        }
    }
}

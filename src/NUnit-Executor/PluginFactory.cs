using NTestController;

namespace NUnitExecutor
{
    public class PluginFactory : IPluginFactory
    {
        public IPlugin GetPlugin(string xmlConfig)
        {
            return new NUnitExecutorPlugin(xmlConfig);
        }
    }
}

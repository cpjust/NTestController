using System;
using NTestController;

namespace NUnitReader
{
    public class PluginFactory : IPluginFactory
    {
        public IPlugin GetPlugin(string xmlConfig)
        {
            return new NUnitReaderPlugin(xmlConfig);
        }
    }
}


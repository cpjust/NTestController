using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

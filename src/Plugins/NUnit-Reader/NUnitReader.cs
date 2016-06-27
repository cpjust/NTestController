using System;
using NTestController;
using Utilities;

[assembly: CLSCompliant(true)]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
namespace NUnitReader
{
    public class NUnitReaderPlugin : IPlugin
    {
        public NUnitReaderPlugin(string xmlConfig)
        {
            ThrowIf.StringIsNullOrWhiteSpace(xmlConfig, nameof(xmlConfig));
        }

        #region Inherited from IPlugin

        public string Name { get { return nameof(NUnitReader); } }
        public PluginType PluginType { get { return PluginType.TestReader; } }

        /// <summary>
        /// Execute this plugin instance.
        /// </summary>
        public bool Execute()
        {
            throw new NotImplementedException();
        }

        #endregion Inherited from IPlugin
    }
}


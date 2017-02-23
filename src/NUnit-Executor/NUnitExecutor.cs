using System;
using NTestController;
using System.Collections.Generic;
using Utilities;

namespace NUnitExecutor
{
    public class NUnitExecutorPlugin : IPlugin
    {
        private IReadOnlyOptions _options;
        private IComputer _computer;
        private TestQueue _testQueue;
        private string _nunitPath;
        private string _xmlConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitExecutorPlugin"/> class.
        /// </summary>
        /// <param name="xmlConfig">NTestController.xml file path.</param>
        public NUnitExecutorPlugin(string xmlConfig)
        {
            ThrowIf.StringIsNullOrWhiteSpace(xmlConfig, nameof(xmlConfig));

            _xmlConfig = xmlConfig;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitExecutor.NUnitExecutorPlugin"/> class.
        /// </summary>
        /// <param name="options">Options from the command line.</param>
        /// <param name="computer">Target Computer where the tests will be run.</param>
        /// <param name="testQueue">Test queue.</param>
        /// <param name="nunitPath">Nunit path.</param>
        public NUnitExecutorPlugin(
            IReadOnlyOptions options,
            IComputer computer,
            TestQueue testQueue,
            string nunitPath)
        {
            _options = options;
            _computer = computer;
            _testQueue = testQueue;
            _nunitPath = nunitPath;
        }

        #region Inherited from IPlugin

        public string Name { get; }
        public PluginType PluginType { get; }

        /// <seealso cref="IPlugin.Execute()"/>
        public bool Execute()
        {
            var test = _testQueue.DequeueTestToRun();

            while (test != null)
            {
                RunTest(test);
                test = _testQueue.DequeueTestToRun();
            }

            return true;
        }

        #endregion Inherited from IPlugin

        private void RunTest(Test test)
        {
            // TODO: Run the test.

            // TODO: Add result to CompletedTests list.
        }
    }
}


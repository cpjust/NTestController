using NTestController;
using Utilities;

namespace NUnitExecutor
{
    public class NUnitExecutorPlugin : IExecutorPlugin
    {
        private IReadOnlyOptions _options;
        private string _nunitPath;
        private string _xmlConfig;

        #region Constructors

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
            Computer = computer;
            TestQueue = testQueue;
            _nunitPath = nunitPath;
        }

        #endregion Constructors

        #region Inherited from IExecutorPlugin

        public string Name { get; private set; }

        public PluginType PluginType
        {
            get { return PluginType.TestExecutor; }
        }

        public IComputer Computer { get; set; }
        public TestQueue TestQueue { get; set; }

        /// <seealso cref="IPlugin.Execute()"/>
        public bool Execute()
        {
            var test = TestQueue.DequeueTestToRun();

            while (test != null)
            {
                RunTest(test);
                test = TestQueue.DequeueTestToRun();
            }

            return true;
        }

        /// <seealso cref="IExecutorPlugin.ClonePlugin()"/>
        public IExecutorPlugin ClonePlugin()
        {
            var newPlugin = new NUnitExecutorPlugin(_xmlConfig);
            newPlugin._options = _options;
            newPlugin._nunitPath = _nunitPath;

            newPlugin.Name = Name;
            newPlugin.Computer = Computer;
            newPlugin.TestQueue = TestQueue;

            return newPlugin;
        }

        #endregion Inherited from IExecutorPlugin

        private void RunTest(Test test)
        {
            // TODO: Run the test.

            // TODO: Add result to CompletedTests list.
        }
    }
}


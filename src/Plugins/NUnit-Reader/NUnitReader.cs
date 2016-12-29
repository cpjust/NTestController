using System;
using NTestController;
using Utilities;
using System.Collections.Generic;
using System.IO;

[assembly: CLSCompliant(true)]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
namespace NUnitReader
{
    public class NUnitReaderPlugin : IPlugin
    {
        private string _testInputFile;
        private List<TestInput> _tests = new List<TestInput>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitReader.NUnitReaderPlugin"/> class.
        /// </summary>
        /// <param name="testInputFile">Test input file.</param>
        public NUnitReaderPlugin(string testInputFile)
        {
            ThrowIf.StringIsNullOrWhiteSpace(testInputFile, nameof(testInputFile));

            _testInputFile = testInputFile;

            ParseTestInputFile(_testInputFile);
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

        /// <summary>
        /// Parses the test input file into a list of TestInput objects.
        /// </summary>
        /// <param name="testInputFile">Test input file.</param>
        private void ParseTestInputFile(string testInputFile)
        {
            // Read input file and add to _tests.
            if (!File.Exists(testInputFile))
            {
                throw new FileNotFoundException(string.Format("Couldn't find file: {0}", testInputFile));
            }

            var fileLines = File.ReadAllLines(testInputFile);

            foreach (string fileLine in fileLines)
            {
                string line = fileLine.Trim();

                // Skip blank lines or commented out lines.
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                var lineParts = line.Split(new char[] {'|'}, count: 2);

                if (lineParts.Length != 2)
                {
                    throw new InvalidDataException(
                        string.Format("Expected 2 parts ('|' separated) but found {0} parts!", lineParts.Length));
                }

                var testInput = new TestInput(lineParts[0], lineParts[1]);

                _tests.Add(testInput);
            }
        }
    }
}


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

        public IList<Test> Tests { get; } = new List<Test>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitReader.NUnitReaderPlugin"/> class.
        /// </summary>
        /// <param name="testInputFile">Test input file.</param>
        public NUnitReaderPlugin(string testInputFile)
        {
            ThrowIf.StringIsNullOrWhiteSpace(testInputFile, nameof(testInputFile));

            _testInputFile = testInputFile;
        }

        #region Inherited from IPlugin

        public string Name { get { return nameof(NUnitReader); } }
        public PluginType PluginType { get { return PluginType.TestReader; } }

        /// <seealso cref="IPlugin.Execute()"/>
        public bool Execute()
        {
            // Read input file and add to Tests.
            if (!File.Exists(_testInputFile))
            {
                throw new FileNotFoundException(StringUtils.FormatInvariant("Couldn't find file: {0}", _testInputFile));
            }

            var fileLines = File.ReadAllLines(_testInputFile);

            ParseTestInputFile(fileLines);
            return true;
        }

        #endregion Inherited from IPlugin

        /// <summary>
        /// Parses the test input file into a list of NUnitTest objects.
        /// </summary>
        /// <param name="fileLines">An array of lines from the test input file.</param>
        private void ParseTestInputFile(string[] fileLines)
        {
            foreach (string fileLine in fileLines)
            {
                string line = fileLine.Trim();

                // Skip blank lines or commented out lines.
                if (string.IsNullOrWhiteSpace(line) || line.StartsWithInvariant("#"))
                {
                    continue;
                }

                var lineParts = line.Split(new char[] {'|'}, count: 2);

                if (lineParts.Length != 2)
                {
                    throw new InvalidDataException(
                        StringUtils.FormatInvariant("Expected 2 parts ('|' separated) but found {0} parts!", lineParts.Length));
                }

                var testInput = new NUnitTest(lineParts[0].Trim(), lineParts[1].Trim());

                Tests.Add(testInput);
            }
        }
    }
}


using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Logger;
using System.Text;
using System.Globalization;

namespace LoggerTests
{
    class TestLogger : BaseLogger, ILogger
    {
        #region Properties

        /// <summary>
        /// Gets the instance of the ConsoleLogger singleton.
        /// </summary>
        /// <value>The ConsoleLogger instance.</value>
        public static TestLogger Instance { get; } = new TestLogger();

        /// <summary>
        /// Gets the error writer.
        /// </summary>
        /// <value>The error writer.</value>
        public override TextWriter ErrorWriter { get; set; } = Console.Error;

        /// <summary>
        /// Gets the output writer.
        /// </summary>
        /// <value>The output writer.</value>
        public override TextWriter OutputWriter { get; set; } = Console.Out;

        #endregion Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger.ConsoleLogger"/> class.
        /// Made protected to make this class a singleton.
        /// </summary>
        protected TestLogger()
        {
        }

        public new bool ShouldWrite(VerboseLevel mode) => base.ShouldWrite(mode);
    }
    
    [TestFixture]
    public class LoggerTests
    {
        private const string STRING_TO_PRINT = "abcdefghijklmnopqrstuvwxyz0123456789";

        private TestLogger _logger = TestLogger.Instance;
        private StringBuilder _outputStringBuilder = new StringBuilder();
        private StringBuilder _errorStringBuilder = new StringBuilder();
        private TextWriter _oldOutputWriter;
        private TextWriter _oldErrorWriter;

        private List<VerboseLevel> AllLevels = new List<VerboseLevel>
        {
            VerboseLevel.NONE,
            VerboseLevel.ERROR,
            VerboseLevel.WARNING,
            VerboseLevel.INFO,
            VerboseLevel.DEBUG,
            VerboseLevel.TRACE
        };

        #region Setup and TearDown

        [SetUp]
        public void Setup()
        {
            // Backup the original output & error writers.
            _oldOutputWriter = _logger.OutputWriter;
            _oldErrorWriter = _logger.ErrorWriter;

            _logger.OutputWriter = new StringWriter(_outputStringBuilder);
            _logger.ErrorWriter = new StringWriter(_errorStringBuilder);

        }

        [TearDown]
        public void TearDown()
        {
            // Restore the original output & error writers.
            _logger.OutputWriter.Dispose();
            _logger.OutputWriter = _oldOutputWriter;
            _oldOutputWriter = null;

            _logger.ErrorWriter.Dispose();
            _logger.ErrorWriter = _oldErrorWriter;
            _oldErrorWriter = null;

            // Clear the StringBuilders for the next test.
            _outputStringBuilder.Clear();
            _errorStringBuilder.Clear();
        }

        #endregion Setup and TearDown

        #region ShouldWrite tests

        [TestCaseSource(nameof(AllLevels))]
        public void ShouldWrite_TrueIfLevelMatches(VerboseLevel levelToSet)
        {
            // *** Setup
            _logger.LogLevel = levelToSet;

            foreach (VerboseLevel level in AllLevels)
            {
                int intLevel = (int)level;
                int intLevelToSet = (int)levelToSet;

                // The level being requested should be contained in the level that was set.
                bool shouldWrite = ((intLevel & intLevelToSet) == intLevel);

                // If either level is set to NONE, then nothing should be written.
                if ((level == VerboseLevel.NONE) || (levelToSet == VerboseLevel.NONE))
                {
                    shouldWrite = false;
                }

                // *** Execute
                bool returnedValue = _logger.ShouldWrite(level);

                // *** Assert
                Assert.AreEqual(returnedValue, shouldWrite,
                    "ShouldWrite({0}) returned false with the log level set to {1}!", level.ToString(), levelToSet.ToString());
            }
        }

        #endregion ShouldWrite tests

        #region Write(string) tests

        [TestCaseSource(nameof(AllLevels))]
        public void WriteError_PrintsToStdOutUnlessLogLevelIsNone(VerboseLevel levelToSet)
        {
            bool writeToStdErr = true;
            var levelsToPrint = new List<VerboseLevel> { VerboseLevel.ERROR, VerboseLevel.WARNING, VerboseLevel.INFO, VerboseLevel.DEBUG, VerboseLevel.TRACE };
            WriteFunc_PrintsOnlyIfLogLevelIsInList(levelToSet, writeToStdErr, levelsToPrint, "WriteError", _logger.WriteError);
        }

        [TestCaseSource(nameof(AllLevels))]
        public void WriteWarning_PrintsToStdOutUnlessLogLevelIsErrorOrNone(VerboseLevel levelToSet)
        {
            bool writeToStdErr = true;
            var levelsToPrint = new List<VerboseLevel> { VerboseLevel.WARNING, VerboseLevel.INFO, VerboseLevel.DEBUG, VerboseLevel.TRACE };
            WriteFunc_PrintsOnlyIfLogLevelIsInList(levelToSet, writeToStdErr, levelsToPrint, "WriteWarning", _logger.WriteWarning);
        }

        [TestCaseSource(nameof(AllLevels))]
        public void WriteInfo_PrintsToStdOutOnlyIfLogLevelIsInfoOrDebugOrTrace(VerboseLevel levelToSet)
        {
            bool writeToStdErr = false;
            var levelsToPrint = new List<VerboseLevel> { VerboseLevel.INFO, VerboseLevel.DEBUG, VerboseLevel.TRACE };
            WriteFunc_PrintsOnlyIfLogLevelIsInList(levelToSet, writeToStdErr, levelsToPrint, "WriteInfo", _logger.WriteInfo);
        }

        [TestCaseSource(nameof(AllLevels))]
        public void WriteDebug_PrintsToStdOutOnlyIfLogLevelIsDebugOrTrace(VerboseLevel levelToSet)
        {
            bool writeToStdErr = false;
            var levelsToPrint = new List<VerboseLevel> { VerboseLevel.DEBUG, VerboseLevel.TRACE };
            WriteFunc_PrintsOnlyIfLogLevelIsInList(levelToSet, writeToStdErr, levelsToPrint, "WriteDebug", _logger.WriteDebug);
        }

        [TestCaseSource(nameof(AllLevels))]
        public void WriteTrace_PrintsToStdOutOnlyIfLogLevelIsTrace(VerboseLevel levelToSet)
        {
            bool writeToStdErr = false;
            var levelsToPrint = new List<VerboseLevel> { VerboseLevel.TRACE };
            WriteFunc_PrintsOnlyIfLogLevelIsInList(levelToSet, writeToStdErr, levelsToPrint, "WriteTrace", _logger.WriteTrace);
        }

        private void WriteFunc_PrintsOnlyIfLogLevelIsInList(
            VerboseLevel levelToSet,
            bool writeToStdErr,
            IList<VerboseLevel> levelsToPrint,
            string funcName,
            Action<string> function)
        {
            // *** Setup
            _logger.LogLevel = levelToSet;

            foreach (VerboseLevel level in AllLevels)
            {
                _errorStringBuilder.Clear();
                _outputStringBuilder.Clear();
                StringBuilder targetStr = _outputStringBuilder;
                StringBuilder emptyStr = _errorStringBuilder;
                string targetStrName = "stdout";
                string emptyStrName = "stderr";

                if (writeToStdErr)
                {
                    targetStr = _errorStringBuilder;
                    emptyStr = _outputStringBuilder;
                    targetStrName = "stderr";
                    emptyStrName = "stdout";
                }

                // *** Execute
                function(STRING_TO_PRINT);

                // *** Assert
                if (levelsToPrint.Contains(levelToSet))
                {
                    Assert.That(targetStr.ToString().Contains(STRING_TO_PRINT),
                        "{0} didn't print to {1} with LogLevel set to {2}!", funcName, targetStrName, levelToSet.ToString());
                }
                else
                {
                    Assert.IsEmpty(targetStr.ToString(),
                        "{0} wrote a string to {1} with LogLevel set to {2}!", funcName, targetStrName, levelToSet.ToString());
                }

                // Error & Warning should never print to stdout.
                // The others should never print to stderr.
                Assert.IsEmpty(emptyStr.ToString(),
                    "{0} wrote a string to {1}!", funcName, emptyStrName);
            }
        }

        #endregion Write(string) tests


        #region Write(string, args) tests

        [TestCaseSource(nameof(AllLevels))]
        public void WriteErrorWithArgs_PrintsToStdOutUnlessLogLevelIsNone(VerboseLevel levelToSet)
        {
            bool writeToStdErr = true;
            var levelsToPrint = new List<VerboseLevel> { VerboseLevel.ERROR, VerboseLevel.WARNING, VerboseLevel.INFO, VerboseLevel.DEBUG, VerboseLevel.TRACE };
            WriteFuncWithArgs_PrintsOnlyIfLogLevelIsInList(levelToSet, writeToStdErr, levelsToPrint, "WriteError", _logger.WriteError);
        }

        [TestCaseSource(nameof(AllLevels))]
        public void WriteWarningWithArgs_PrintsToStdOutUnlessLogLevelIsErrorOrNone(VerboseLevel levelToSet)
        {
            bool writeToStdErr = true;
            var levelsToPrint = new List<VerboseLevel> { VerboseLevel.WARNING, VerboseLevel.INFO, VerboseLevel.DEBUG, VerboseLevel.TRACE };
            WriteFuncWithArgs_PrintsOnlyIfLogLevelIsInList(levelToSet, writeToStdErr, levelsToPrint, "WriteWarning", _logger.WriteWarning);
        }

        [TestCaseSource(nameof(AllLevels))]
        public void WriteInfoWithArgs_PrintsToStdOutOnlyIfLogLevelIsInfoOrDebugOrTrace(VerboseLevel levelToSet)
        {
            bool writeToStdErr = false;
            var levelsToPrint = new List<VerboseLevel> { VerboseLevel.INFO, VerboseLevel.DEBUG, VerboseLevel.TRACE };
            WriteFuncWithArgs_PrintsOnlyIfLogLevelIsInList(levelToSet, writeToStdErr, levelsToPrint, "WriteInfo", _logger.WriteInfo);
        }

        [TestCaseSource(nameof(AllLevels))]
        public void WriteDebugWithArgs_PrintsToStdOutOnlyIfLogLevelIsDebugOrTrace(VerboseLevel levelToSet)
        {
            bool writeToStdErr = false;
            var levelsToPrint = new List<VerboseLevel> { VerboseLevel.DEBUG, VerboseLevel.TRACE };
            WriteFuncWithArgs_PrintsOnlyIfLogLevelIsInList(levelToSet, writeToStdErr, levelsToPrint, "WriteDebug", _logger.WriteDebug);
        }

        [TestCaseSource(nameof(AllLevels))]
        public void WriteTraceWithArgs_PrintsToStdOutOnlyIfLogLevelIsTrace(VerboseLevel levelToSet)
        {
            bool writeToStdErr = false;
            var levelsToPrint = new List<VerboseLevel> { VerboseLevel.TRACE };
            WriteFuncWithArgs_PrintsOnlyIfLogLevelIsInList(levelToSet, writeToStdErr, levelsToPrint, "WriteTrace", _logger.WriteTrace);
        }

        private void WriteFuncWithArgs_PrintsOnlyIfLogLevelIsInList(
            VerboseLevel levelToSet,
            bool writeToStdErr,
            IList<VerboseLevel> levelsToPrint,
            string funcName,
            Action<string, Object[]> function)
        {
            const string FORMAT_STRING = "Level is {0}, which equals number {1}.";

            // *** Setup
            _logger.LogLevel = levelToSet;

            foreach (VerboseLevel level in AllLevels)
            {
                _errorStringBuilder.Clear();
                _outputStringBuilder.Clear();
                StringBuilder targetStr = _outputStringBuilder;
                StringBuilder emptyStr = _errorStringBuilder;
                string targetStrName = "stdout";
                string emptyStrName = "stderr";
                Object[] args = { level.ToString(), (int)level };
                string stringToMatch = string.Format(CultureInfo.InvariantCulture, FORMAT_STRING, args);

                if (writeToStdErr)
                {
                    targetStr = _errorStringBuilder;
                    emptyStr = _outputStringBuilder;
                    targetStrName = "stderr";
                    emptyStrName = "stdout";
                }

                // *** Execute
                function(FORMAT_STRING, args);

                // *** Assert
                if (levelsToPrint.Contains(levelToSet))
                {
                    Assert.That(targetStr.ToString().Contains(stringToMatch),
                        "{0} didn't print to {1} with LogLevel set to {2}!", funcName, targetStrName, levelToSet.ToString());
                }
                else
                {
                    Assert.IsEmpty(targetStr.ToString(),
                        "{0} wrote a string to {1} with LogLevel set to {2}!", funcName, targetStrName, levelToSet.ToString());
                }

                // Error & Warning should never print to stdout.
                // The others should never print to stderr.
                Assert.IsEmpty(emptyStr.ToString(),
                    "{0} wrote a string to {1}!", funcName, emptyStrName);
            }
        }

        #endregion Write(string, args) tests

    }
}


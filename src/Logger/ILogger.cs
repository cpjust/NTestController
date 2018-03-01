using System;

namespace Logger
{
    /// <summary>
    /// Specific values for the verbose (logging) levels ( Error, Warning, Info, Debug, Trace )
    /// </summary>
    public enum VerboseLevel
    {
        NONE = 0,
        ERROR = 1,
        WARNING = 3,  // ERROR + WARNING
        INFO = 7,     // ERROR + WARNING + INFO
        DEBUG = 15,   // ERROR + WARNING + INFO + DEBUG
        TRACE = 31    // ERROR + WARNING + INFO + DEBUG + TRACE
    }

    public interface ILogger
    {
        #region Public Properties

        /// <summary>
        /// Get/set the level of info to display in the log.
        /// </summary>
        VerboseLevel LogLevel { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Prints a message to standard error only if the log level is Error,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="message">The string to be printed.</param>
        void WriteError(string message);

        /// <summary>
        /// Prints a formatted message to standard error only if the log level is Error,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="args">The arguments for the format string.</param>
        void WriteError(string format, params Object[] args);

        /// <summary>
        /// Prints a message to standard error if the log level is Warning or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="message">The string to be printed.</param>
        void WriteWarning(string message);

        /// <summary>
        /// Prints a formatted message to standard error if the log level is Warning or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="args">The arguments for the format string.</param>
        void WriteWarning(string format, params Object[] args);

        /// <summary>
        /// Prints a message to standard output if the log level is Info or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="message">The string to be printed.</param>
        void WriteInfo(string message);

        /// <summary>
        /// Prints a formatted message to standard output if the log level is Info or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="args">The arguments for the format string.</param>
        void WriteInfo(string format, params Object[] args);

        /// <summary>
        /// Prints a message to standard output if the log level is Debug or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="message">The string to be printed.</param>
        void WriteDebug(string message);

        /// <summary>
        /// Prints a formatted message to standard output if the log level is Debug or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="args">The arguments for the format string.</param>
        void WriteDebug(string format, params Object[] args);

        /// <summary>
        /// Prints a message to standard output if the log level is Trace or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="message">The string to be printed.</param>
        void WriteTrace(string message);

        /// <summary>
        /// Prints a formatted message to standard output if the log level is Trace or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="args">The arguments for the format string.</param>
        void WriteTrace(string format, params Object[] args);

        #endregion
    }
}


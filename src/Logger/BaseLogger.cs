﻿using System.Collections.Generic;
using System.IO;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading;

namespace Logger
{
    /// <summary>
    /// An abstract thread-safe logger to write to stdout or stderr depending on the log level chosen.
    /// </summary>
    public abstract class BaseLogger : ILogger
    {
        private readonly object _lock = new object();
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        #region Private Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger.Logger"/> class.
        /// Made protected to make this class a singleton.
        /// </summary>
        protected BaseLogger()
        {
        }

        /// <summary>
        /// Returns whether or not a message should be logged based on the current logging level.
        /// </summary>
        protected bool ShouldWrite(VerboseLevel mode)
        {
            int intLevel = (int)LogLevel;
            int intMode = (int)mode;
            int andResult = (intLevel & intMode);

            return ((andResult > 0) && (andResult == intMode));
        }

        /// <summary>
        /// Prints the specified message to the output stream if the current log level is >= level.
        /// This method is thread-safe.  All output is prepended with the date & time, and log level.
        /// </summary>
        /// <example>2016-05-21 19:10:01Z INFO: This is the message.</example>
        /// <param name="level">The logging level of this message.</param>
        /// <param name="writer">The stream to where the output is sent.</param>
        /// <param name="msg">The message to be printed.</param>
        protected void Write(VerboseLevel level, TextWriter writer, string msg)
        {
            Write(level, writer, msg, null);
        }

        /// <summary>
        /// Prints the specified message to the output stream if the current log level is >= level.
        /// This method is thread-safe.  All output is prepended with the date & time, and log level.
        /// </summary>
        /// <example>2016-05-21 19:10:01Z INFO: This is the message.</example>
        /// <param name="level">The logging level of this message.</param>
        /// <param name="writer">The stream to where the output is sent.</param>
        /// <param name="format">The format string to be printed.</param>
        /// <param name="args">(optional) The arguments for the format string.</param>
        protected void Write(VerboseLevel level, TextWriter writer, string format,  params Object[] args)
        {
            if (ShouldWrite(level))
            {
                string datetime = DateTime.Now.ToString("u");
                format = string.Format(CultureInfo.InvariantCulture, "{0} {1}: {2}", datetime, level.ToString(), format);

                lock (_lock)
                {
                    if (args == null) { writer.WriteLine(format); }
                    else { writer.WriteLine(format, args); }
                }
            }
        }

        /// <summary>
        /// (Untested) Prints the specified message to the output stream asynchronously if the current log level is >= level.
        /// This method is thread-safe.  All output is prepended with the date & time, and log level.
        /// </summary>
        /// <example>2016-05-21 19:10:01Z INFO: This is the message.</example>
        /// <param name="level">The logging level of this message.</param>
        /// <param name="writer">The stream to where the output is sent.</param>
        /// <param name="format">The format string to be printed.</param>
        /// <param name="args">(optional) The arguments for the format string.</param>
        protected async Task WriteAsync(VerboseLevel level, TextWriter writer, string format,  params Object[] args)
        {
            if (ShouldWrite(level))
            {
                string datetime = DateTime.Now.ToString("u");
                format = string.Format(CultureInfo.InvariantCulture, "{0} {1}: {2}", datetime, level.ToString(), format);

                if (args != null)
                {
                    format = string.Format(CultureInfo.InvariantCulture, format, args);
                }

                await _semaphore.WaitAsync();

                try
                {
                    await writer.WriteLineAsync(format);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        #endregion Private Methods


        #region Public Properties

        /// <summary>
        /// Gets the error writer.
        /// </summary>
        /// <value>The error writer.</value>
        public abstract TextWriter ErrorWriter { get; set; }

        /// <summary>
        /// Gets the output writer.
        /// </summary>
        /// <value>The output writer.</value>
        public abstract TextWriter OutputWriter { get; set; }

        #endregion Public Properties


        #region Inherited from ILogger

        /// <summary>
        /// Get/set the level of info to display in the log.
        /// </summary>
        public VerboseLevel LogLevel { get; set; }

        /// <summary>
        /// Prints a message to standard error only if the log level is Error,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="msg">The string to be printed.</param>
        public void WriteError(string msg)
        {
            Write(VerboseLevel.ERROR, ErrorWriter, msg);
        }

        /// <summary>
        /// Prints a formatted message to standard error only if the log level is Error,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="args">The arguments for the format string.</param>
        /// <param name="msg">Message.</param>
        public void WriteError(string format, params Object[] msg)
        {
            Write(VerboseLevel.ERROR, ErrorWriter, format, msg);
        }

        /// <summary>
        /// Prints a message to standard error if the log level is Warning or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="msg">The string to be printed.</param>
        public void WriteWarning(string msg)
        {
            Write(VerboseLevel.WARNING, ErrorWriter, msg);
        }

        /// <summary>
        /// Prints a formatted message to standard error if the log level is Warning or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="args">The arguments for the format string.</param>
        /// <param name="msg">Message.</param>
        public void WriteWarning(string format, params Object[] msg)
        {
            Write(VerboseLevel.WARNING, ErrorWriter, format, msg);
        }

        /// <summary>
        /// Prints a message to standard output if the log level is Info or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="msg">The string to be printed.</param>
        public void WriteInfo(string msg)
        {
            Write(VerboseLevel.INFO, OutputWriter, msg);
        }

        /// <summary>
        /// Prints a formatted message to standard output if the log level is Info or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="args">The arguments for the format string.</param>
        public void WriteInfo(string format, params Object[] args)
        {
            Write(VerboseLevel.INFO, OutputWriter, format, args);
        }

        /// <summary>
        /// Prints a message to standard output if the log level is Debug or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="msg">The string to be printed.</param>
        public void WriteDebug(string msg)
        {
            Write(VerboseLevel.DEBUG, OutputWriter, msg);
        }

        /// <summary>
        /// Prints a formatted message to standard output if the log level is Debug or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="args">The arguments for the format string.</param>
        /// <param name="msg">Message.</param>
        public void WriteDebug(string format, params Object[] msg)
        {
            Write(VerboseLevel.DEBUG, OutputWriter, format, msg);
        }

        /// <summary>
        /// Prints a message to standard output if the log level is Trace or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="msg">The string to be printed.</param>
        public void WriteTrace(string msg)
        {
            Write(VerboseLevel.TRACE, OutputWriter, msg);
        }

        /// <summary>
        /// Prints a formatted message to standard output if the log level is Trace or higher,
        /// and prepends the date/time and requested log level.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="args">The arguments for the format string.</param>
        /// <param name="msg">Message.</param>
        public void WriteTrace(string format, params Object[] msg)
        {
            Write(VerboseLevel.TRACE, OutputWriter, format, msg);
        }

        #endregion Inherited from ILogger
    }
}

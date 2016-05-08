using System;
using System.IO;

namespace Logger
{
    public class ConsoleLogger : BaseLogger, ILogger
    {
        #region Properties
        
        /// <summary>
        /// Gets the instance of the ConsoleLogger singleton.
        /// </summary>
        /// <value>The ConsoleLogger instance.</value>
        public static ILogger Instance { get; } = new ConsoleLogger();

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
        protected ConsoleLogger()
        {
        }
    }
}


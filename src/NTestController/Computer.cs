using System;
using System.Collections.Generic;

namespace NTestController
{
    public class Credentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Computer : IComputer
    {
        #region Members from IComputer

        /// <see cref="IComputer.Credentials"/>
        public Credentials Credentials { get; } = new Credentials();

        /// <see cref="IComputer.Hostname"/>
        public string Hostname { get; set; }

        /// <see cref="IComputer.Timeout"/>
        public int Timeout { get; set; }

        /// <see cref="IComputer.EnvironmentVariables"/>
        public Dictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>();

        /// <see cref="IComputer.OutputPath"/>
        public string OutputPath { get; set; }

        /// <see cref="IComputer.WorkingDirectory"/>
        public string WorkingDirectory { get; set; }

        #endregion Members from IComputer
    }
}


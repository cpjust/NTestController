using System;

namespace NTestController
{
    public interface IPlatform
    {
        /// <summary>
        /// The Operating System for this group of computers.  Ex. Windows, Linux...
        /// </summary>
        string OS { get; set; }

        /// <summary>
        /// The OS Version for this group of computers.  Ex. 2008R2, 2012...
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// The CPU type.  Ex.  x86 or x64.
        /// </summary>
        string CPU { get; set; }
    }
}


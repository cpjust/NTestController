using System.Collections.Generic;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CPU")]
        string CPU { get; set; }

        /// <summary>
        /// The list of Computers for this Platform.
        /// </summary>
        IList<IComputer> Computers { get; }
    }
}


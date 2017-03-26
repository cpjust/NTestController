using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTestController;

namespace NUnitReader
{
    public class NUnitComputer : Computer, IComputer
    {
        /// <summary>
        /// Gets or sets the NUnit path for this Computer.
        /// </summary>
        /// <value>The NUnit path for this Computer.</value>
        public string NunitPath { get; set; }   // TODO: Move this to a NUnitComputer class.
    }
}

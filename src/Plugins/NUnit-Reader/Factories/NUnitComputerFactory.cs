using System.Xml;
using NTestController;
using NTestController.Factories;
using Utilities;

namespace NUnitReader
{
    public class NUnitComputerFactory : IComputerFactory
    {
        /// <summary>
        /// Creates an IComputer object with the data from the specified XmlNodes.
        /// </summary>
        /// <param name="computerNode">The Computer node from the NTestController.xml file which the newly created IComputer will represent.</param>
        /// <param name="defaultsNode">The Defaults node from the NTestController.xml file that contains any required attributes not specified in the Computer node.</param>
        /// <param name="computer">Unused.</param>
        /// <returns>An IComputer object with the data from the specified XmlNodes.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        public IComputer CreateComputer(XmlNode computerNode, XmlNode defaultsNode, IComputer computer)
        {
            ThrowIf.ArgumentNull(computerNode, nameof(computerNode));
            ThrowIf.ArgumentNull(defaultsNode, nameof(defaultsNode));

            var nunitNode = XmlUtils.GetChildNodeOrDefault(computerNode, defaultsNode, "nunit");
            var nunitComputer = new NUnitComputer();
            var computerFactory = new ComputerFactory();

            computerFactory.CreateComputer(computerNode, defaultsNode, nunitComputer);
            nunitComputer.NunitPath = XmlUtils.GetXmlAttribute(nunitNode, "path");

            return nunitComputer;
        }
    }
}

using System.Xml;
using Utilities;

namespace NTestController.Factories
{
    public class PlatformFactory : IPlatformFactory
    {
        /// <seealso cref="IPlatformFactory.CreatePlatform(XmlNode, XmlNode, IComputerFactory, IPlatform)"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        public IPlatform CreatePlatform(
            XmlNode platformNode,
            XmlNode defaultsNode,
            IComputerFactory computerFactory,
            IPlatform platform = null)
        {
            ThrowIf.ArgumentNull(platformNode, nameof(platformNode));
            ThrowIf.ArgumentNull(computerFactory, nameof(computerFactory));

            // TODO: Call the Platform factory of the Reader DLL so we can create specific Platform & Computer types (ex. NUnitComputer).
            platform = platform ?? new Platform();
            platform.CPU = XmlUtils.GetXmlAttribute(platformNode, "cpu");
            platform.OS = XmlUtils.GetXmlAttribute(platformNode, "os");
            platform.Version = XmlUtils.GetXmlAttribute(platformNode, "version");

            var node = platformNode.FirstChild;

            while (node != null)
            {
                if ((node.NodeType == XmlNodeType.Element) && (node.Name == "computer"))
                {
                    XmlNode computerNode = node;

                    IComputer computer = computerFactory.CreateComputer(computerNode, defaultsNode);
                    platform.Computers.Add(computer);
                }

                node = node.NextSibling;
            }

            return platform;
        }
    }
}


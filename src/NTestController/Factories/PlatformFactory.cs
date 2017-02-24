using System.Xml;
using Utilities;

namespace NTestController.Factories
{
    public static class PlatformFactory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        public static IPlatform CreatePlatform(XmlNode platformNode, XmlNode defaultsNode)
        {
            // TODO: Call the Platform factory of the Reader DLL so we can create specific Platform & Computer types (ex. NUnitComputer).
            IPlatform platform = new Platform();
            platform.CPU = XmlUtils.GetXmlAttribute(platformNode, "cpu");
            platform.OS = XmlUtils.GetXmlAttribute(platformNode, "os");
            platform.Version = XmlUtils.GetXmlAttribute(platformNode, "version");

            var node = platformNode.FirstChild;

            while (node != null)
            {
                if ((node.NodeType == XmlNodeType.Element) && (node.Name == "computer"))
                {
                    XmlNode computerNode = node;

                    if (computerNode != null)
                    {
                        IComputer computer = ComputerFactory.CreateComputer(computerNode, defaultsNode);
                        platform.Computers.Add(computer);
                    }
                }

                node = node.NextSibling;
            }

            return platform;
        }
    }
}


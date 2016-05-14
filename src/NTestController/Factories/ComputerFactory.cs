using System.Xml;

namespace NTestController.Factories
{
    public class ComputerFactory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        public ComputerFactory(XmlNode computerNode, XmlNode defaultsNode)
        {
        }
    }
}


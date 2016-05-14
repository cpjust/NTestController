using System;
using System.Xml;
using NTestController;

namespace NUnitReader
{
    public class PlatformFactory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        public IPlatform CreatePlatform(XmlNode platformNode, XmlNode defaultsNode)
        {
            throw new NotImplementedException();
        }
    }
}


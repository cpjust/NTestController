using System;
using System.Xml;
using NTestController;

namespace NUnitReader
{
    public class PlatformFactory  // TODO: Is this class needed?
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "platformNode")]   // Will need this later.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "defaultsNode")]   // Will need this later.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        public IPlatform CreatePlatform(XmlNode platformNode, XmlNode defaultsNode)
        {
            throw new NotImplementedException();
        }
    }
}


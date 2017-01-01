using System;
using System.Xml;

namespace NTestController.Factories
{
    public static class PlatformFactory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        public static IPlatform CreatePlatform(XmlNode platformNode, XmlNode defaultsNode)
        {
            // TODO: Call the Platform factory of the Reader DLL.
            throw new NotImplementedException();  // TODO: WTF?  The comment above doesn't make sense.  Why does the Reader need a Platform Factory?
        }
    }
}


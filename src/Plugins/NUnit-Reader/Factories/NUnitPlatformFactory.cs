using System;
using System.Xml;
using NTestController;
using NTestController.Factories;

namespace NUnitReader.Factories
{
    public class NUnitPlatformFactory : IPlatformFactory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        public IPlatform CreatePlatform(
            XmlNode platformNode,
            XmlNode defaultsNode,
            IComputerFactory computerFactory, 
            IPlatform platform = null)
        {
            computerFactory = computerFactory ?? new NUnitComputerFactory() as IComputerFactory;

            var factory = new PlatformFactory();
            return factory.CreatePlatform(platformNode, defaultsNode, computerFactory, platform);
        }
    }
}


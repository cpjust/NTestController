using System.Xml;

namespace NTestController.Factories
{
    public interface IPlatformFactory
    {
        /// <summary>
        /// Creates an IPlatform object with the data from the specified XmlNodes.
        /// </summary>
        /// <param name="platformNode">The Platform node from the NTestController.xml file which the newly created IPlatform will represent.</param>
        /// <param name="defaultsNode">The Defaults node from the NTestController.xml file that contains any required attributes not specified in the Platform node.</param>
        /// <param name="computerFactory">The ComputerFactory to use when creating Computers.</param>
        /// <param name="platform">(optional) Pass a platform object whose properties will be set.  By default a new platform is created.</param>
        /// <returns>An IPlatform object with the data from the specified XmlNodes.</returns>
        IPlatform CreatePlatform(
            XmlNode platformNode,
            XmlNode defaultsNode,
            IComputerFactory computerFactory,
            IPlatform platform = null);
    }
}

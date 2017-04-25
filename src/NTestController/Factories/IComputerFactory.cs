using System.Xml;

namespace NTestController.Factories
{
    public interface IComputerFactory
    {
        /// <summary>
        /// Creates an IComputer object with the data from the specified XmlNodes.
        /// </summary>
        /// <param name="computerNode">The Computer node from the NTestController.xml file which the newly created IComputer will represent.</param>
        /// <param name="defaultsNode">The Defaults node from the NTestController.xml file that contains any required attributes not specified in the Computer node.</param>
        /// <param name="computer">(optional) Pass a computer object whose properties will be set.  By default a new computer is created.</param>
        /// <returns>An IComputer object with the data from the specified XmlNodes.</returns>
        IComputer CreateComputer(
            XmlNode computerNode,
            XmlNode defaultsNode,
            IComputer computer = null);
    }
}

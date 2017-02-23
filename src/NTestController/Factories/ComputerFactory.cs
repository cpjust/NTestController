using System.Xml;
using Utilities;

namespace NTestController.Factories
{
    public static class ComputerFactory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        public static IComputer CreateComputer(XmlNode computerNode, XmlNode defaultsNode)
        {
            ThrowIf.ArgumentNull(computerNode, nameof(computerNode));
            ThrowIf.ArgumentNull(defaultsNode, nameof(defaultsNode));

            IComputer computer = new Computer();

            computer.Hostname = XmlUtils.GetXmlAttribute(computerNode, "hostname");

            var defaultComputerNode = defaultsNode.FirstChild.SelectSingleNode("computer");

            computer.Timeout = int.Parse(XmlUtils.GetXmlAttributeOrDefault(computerNode, defaultComputerNode, "timeout"));

            // Get values from all the sub-nodes.
            var envVarNode = XmlUtils.GetChildNodeOrDefault(computerNode, defaultsNode, "env_var"); // TODO: The <env_var> node should be moved inside a <env_vars> node.
            var nunitNode = XmlUtils.GetChildNodeOrDefault(computerNode, defaultsNode, "nunit");
            var testResultsNode = XmlUtils.GetChildNodeOrDefault(computerNode, defaultsNode, "test_results");
            var workingDirNode = XmlUtils.GetChildNodeOrDefault(computerNode, defaultsNode, "working_dir");
            var credentialsNode = XmlUtils.GetChildNodeOrDefault(computerNode, defaultsNode, "credentials");

            string envVarName = XmlUtils.GetXmlAttribute(envVarNode, "name");
            string envVarValue = XmlUtils.GetXmlAttribute(envVarNode, "value");
            computer.EnvironmentVariables.Add(envVarName, envVarValue);

            computer.NunitPath = XmlUtils.GetXmlAttribute(nunitNode, "path");
            computer.OutputPath = XmlUtils.GetXmlAttribute(testResultsNode, "path");
            computer.WorkingDirectory = XmlUtils.GetXmlAttribute(workingDirNode, "path");

            computer.Credentials.Username = XmlUtils.GetXmlAttribute(credentialsNode, "username");
            computer.Credentials.Password = XmlUtils.GetXmlAttribute(credentialsNode, "password");

            return computer;
        }
    }
}


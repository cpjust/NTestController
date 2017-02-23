using System;
using System.Xml;

namespace Utilities
{
    public static class XmlUtils
    {
        /// <summary>
        /// Gets the XML attribute from the specified XmlNode.
        /// </summary>
        /// <param name="node">The XmlNode containing the attribute you want.</param>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>The xml attribute value.</returns>
        /// <exception cref="XmlException">There was an error parsing the XmlNode.</exception>
        public static string GetXmlAttribute(XmlNode node, string attributeName)
        {
            ThrowIf.ArgumentNull(node, nameof(XmlNodeType));
            ThrowIf.StringIsNullOrWhiteSpace(attributeName, nameof(attributeName));

            if (node.Attributes?[attributeName] == null)
            {
                throw new XmlException(StringUtils.FormatInvariant("No XML attribute named '{0}' was found!", attributeName));
            }

            return node.Attributes[attributeName].Value;
        }

        /// <summary>
        /// Gets the XML attribute from the specified XmlNode or from the defaults XmlNode.
        /// </summary>
        /// <param name="node">The Xml node to check first for the attribute.</param>
        /// <param name="defaultsNode">The default XmlNode to get the attribute from if not present in main node.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <returns>The attribute value.</returns>
        /// <exception cref="XmlException">There was an error parsing the XmlNode.</exception>
        public static string GetXmlAttributeOrDefault(XmlNode node, XmlNode defaultsNode, string attributeName)
        {
            if (node.Attributes?[attributeName] != null)
            {
                return GetXmlAttribute(node, attributeName);
            }

            return GetXmlAttribute(defaultsNode, attributeName);
        }

        /// <summary>
        /// Gets the XmlNode with the specified name from the main node, otherwise from defaultsNode.
        /// </summary>
        /// <param name="node">The main node to check first for the XmlNode.</param>
        /// <param name="defaultsNode">The default XmlNode to get the XmlNode from if not present in the main node.</param>
        /// <param name="nodeName">The name of the XmlNode to get.</param>
        /// <returns>The XmlNode requested.</returns>
        /// <exception cref="XmlException">There was an error parsing the XmlNode.</exception>
        public static XmlNode GetChildNodeOrDefault(XmlNode node, XmlNode defaultsNode, string nodeName)
        {
            var childNode = node.FirstChild.SelectSingleNode(nodeName) ?? defaultsNode.FirstChild.SelectSingleNode(nodeName);

            if (childNode == null)
            {
                throw new XmlException(StringUtils.FormatInvariant("No XmlNode named '{0}' was found!", nodeName));
            }

            return childNode;
        }
    }
}


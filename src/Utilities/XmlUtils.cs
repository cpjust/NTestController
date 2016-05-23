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
        public static string GetXmlAttribute(XmlNode node, string attributeName)
        {
            ThrowIf.ArgumentNull(node, nameof(XmlNodeType));
            ThrowIf.StringIsNullOrWhiteSpace(attributeName, nameof(attributeName));

            if (node.Attributes[attributeName] == null)
            {
                throw new ArgumentException(StringUtils.FormatInvariant("No XML attribute named '{0}' was found!", attributeName));
            }

            return node.Attributes[attributeName].Value;
        }
    }
}


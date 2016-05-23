using System;
using System.Xml;
using NUnit.Framework;
using Utilities;

namespace UtilitiesTests
{
    [TestFixture]
    public static class XmlUtilsTests
    {
        [TestCase]
        public static void GetXmlAttribute_ValidArgs_ValueIsReturned()
        {
            const string attrName = "myAttr";
            const string attrValue = "myValue";
            string xmlString = StringUtils.FormatInvariant("<something {0}=\"{1}\" />", attrName, attrValue);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            XmlNode node = doc.FirstChild;

            string value = XmlUtils.GetXmlAttribute(node, attrName);

            Assert.AreEqual(attrValue, value, "{0}.{1}(node, \"{2}\") should return '{3}' when node = '{4}'!",
                nameof(XmlUtils), nameof(XmlUtils.GetXmlAttribute), attrName, attrValue, xmlString);
        }

        [TestCase]
        public static void GetXmlAttribute_NonExistentAttribute_Exception()
        {
            const string attrName = "myAttr";
            const string attrValue = "myValue";
            string xmlString = StringUtils.FormatInvariant("<something {0}=\"{1}\" />", attrName, attrValue);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            XmlNode node = doc.FirstChild;

            Assert.Throws<ArgumentException>(() =>
            {
                XmlUtils.GetXmlAttribute(node, "foo");
            }, "{0}.{1}() should throw a {2} if passed an attributeName argument that doesn't exist in the XmlNode!",
                nameof(XmlUtils), nameof(XmlUtils.GetXmlAttribute), nameof(ArgumentException));
        }
        
        [TestCase]
        public static void GetXmlAttribute_NullXmlNode_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                XmlUtils.GetXmlAttribute(null, "foo");
            }, "{0}.{1}() should throw a {2} if passed a null XmlNode argument!",
                nameof(XmlUtils), nameof(XmlUtils.GetXmlAttribute), nameof(ArgumentNullException));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\n")]
        public static void GetXmlAttribute_NullOrWhiteSpaceStringArg_ArgumentNullException(string attributeName)
        {
            XmlNode node = new XmlDocument();

            Assert.Throws<ArgumentNullException>(() =>
            {
                XmlUtils.GetXmlAttribute(node, attributeName);
            }, "{0}.{1}() should throw a {2} if passed a null attributeName argument!",
                nameof(XmlUtils), nameof(XmlUtils.GetXmlAttribute), nameof(ArgumentNullException));
        }
    }
}


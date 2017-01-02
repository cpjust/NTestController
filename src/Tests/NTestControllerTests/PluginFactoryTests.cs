using NUnit.Framework;
using System;
using NTestController;
using System.Xml;
using System.IO;
using Utilities;

namespace NTestControllerTests
{
    [TestFixture]
    public static class PluginFactoryTests
    {
        #region Test Data

        private const string XML_CONFIG = @"<ntest_controller>
    <plugins>
        <plugin type=""TestReader""   path=""NullPlugin.dll"" />
        <plugin type=""EnvSetup""     path=""NullPlugin.dll"" />
        <plugin type=""TestExecutor"" path=""NullPlugin.dll"" />
        <plugin type=""EnvCleanup""   path=""NullPlugin.dll"" />
        <plugin type=""TestReporter"" path=""NullPlugin.dll"" />
    </plugins>

    <defaults>
        <computer timeout=""600"" />
        <credentials username=""Administrator"" password=""password"" />
        <nunit path=""C:\Program Files (x86)\NUnit 2.6.4\bin\nunit-console.exe"" />
        <test_results path="".\Test_Results"" />
        <working_dir path="".\bin"" />
    </defaults>

    <platform os=""windows"" version=""2012R2"" cpu=""x64"">
        <computer hostname=""TestVM1"" timeout=""600"">
            <env_var name=""CONFIG_FILE"" value="".\conf\TestVM1.conf"" />
        </computer>
    </platform>
</ntest_controller>";

        #endregion Test Data

        #region LoadPlugin Tests

        [TestCase("NullPlugin.dll", null)]
        [TestCase("NullPlugin.dll", "")]
        [TestCase("NullPlugin.dll", " ")]
        [TestCase("NullPlugin.dll", "\t")]
        [TestCase("NullPlugin.dll", "\n")]
        [TestCase(null, "NTestController.xml")]
        [TestCase("", "NTestController.xml")]
        [TestCase(" ", "NTestController.xml")]
        [TestCase("\t", "NTestController.xml")]
        [TestCase("\n", "NTestController.xml")]
        public static void LoadPlugin_NullOrWhiteSpaceArgs_ArgumentNullException(string dllFile, string xmlConfig)
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                PluginFactory.LoadPlugin(dllFile, xmlConfig);
            }, "{0}.{1}() should throw a {2} when null arguments are passed in!",
                nameof(PluginFactory), nameof(PluginFactory.LoadPlugin), nameof(ArgumentNullException));
        }

        [TestCase]
        public static void LoadPlugin_MissingDll_DllNotFoundException()
        {
            Assert.Throws<DllNotFoundException>(() =>
            {
                PluginFactory.LoadPlugin("foo.dll", "bar.xml");
            }, "{0}.{1}() should throw a {2} when passed a non-existent DLL!",
                nameof(PluginFactory), nameof(PluginFactory.LoadPlugin), nameof(DllNotFoundException));
        }

        [TestCase]
        public static void LoadPlugin_ValidArgs_ReturnsIPlugin()
        {
            IPlugin plugin = null;

            Assert.DoesNotThrow(() =>
            {
                plugin = PluginFactory.LoadPlugin("NullPlugin.dll", "TestReader");
            }, "{0}.{1}() should not throw an exception when passed a valid DLL file!",
                nameof(PluginFactory), nameof(PluginFactory.LoadPlugin));

            Assert.NotNull(plugin, "{0}.{1}() returned null when passed a valid DLL file!",
                nameof(PluginFactory), nameof(PluginFactory.LoadPlugin));
        }

        [TestCase]
        public static void LoadPlugin_DllDoesNotImplementIPlugin_EntryPointNotFoundException()
        {
            Assert.Throws<EntryPointNotFoundException>(() =>
            {
                PluginFactory.LoadPlugin("Logger.dll", "TestReader");
            }, "{0}.{1}() should throw a {2} when passed a DLL that doesn't implement the IPlugin interface!",
                nameof(PluginFactory), nameof(PluginFactory.LoadPlugin), nameof(EntryPointNotFoundException));
        }

        #endregion LoadPlugin Tests

        #region GetPlugin Tests

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\n")]
        public static void GetPlugin_NullOrWhiteSpaceXmlConfig_ArgumentNullException(string xmlConfig)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<plugin type=\"TestReader\" path=\"NullPlugin.dll\" />");
            XmlNode node = doc.FirstChild;
            
            Assert.Throws<ArgumentNullException>(() =>
            {
                PluginFactory.GetPlugin(node, xmlConfig);
            }, "{0}.{1}() should throw a {2} when a null or white space NTestController.xml file is passed in!",
                nameof(PluginFactory), nameof(PluginFactory.GetPlugin), nameof(ArgumentNullException));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("InvalidPluginType")]
        public static void GetPlugin_InvalidPluginTypeInXmlConfig_ArgumentException(string xmlConfig)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(StringUtils.FormatInvariant("<plugin type=\"{0}\" path=\"NullPlugin.dll\" />", xmlConfig));
            XmlNode node = doc.FirstChild;

            Assert.Throws<ArgumentException>(() =>
            {
                PluginFactory.GetPlugin(node, xmlConfig);
            }, "{0}.{1}() should throw a {2} when the NTestController.xml file contains an invalid plugin type!",
                nameof(PluginFactory), nameof(PluginFactory.GetPlugin), nameof(ArgumentException));
        }

        [TestCase("<plugin path=\"NullPlugin.dll\" />")]
        [TestCase("<plugin type=\"TestReader\" />")]
        public static void GetPlugin_InvalidXmlConfig_XmlException(string xmlConfig)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlConfig);
            XmlNode node = doc.FirstChild;

            Assert.Throws<XmlException>(() =>
            {
                PluginFactory.GetPlugin(node, xmlConfig);
            }, "{0}.{1}() should throw a {2} when the NTestController.xml file contains an invalid plugin node!",
                nameof(PluginFactory), nameof(PluginFactory.GetPlugin), nameof(XmlException));
        }

        [TestCase]
        public static void GetPlugin_WrongPluginType_TypeLoadException()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<plugin type=\"TestReporter\" path=\"NUnit-Reader.dll\" />");
            XmlNode node = doc.FirstChild;

            string xmlConfig = XML_CONFIG.Replace("NullPlugin", "NUnit-Reader.dll");

            Assert.Throws<TypeLoadException>(() =>
            {
                PluginFactory.GetPlugin(node, xmlConfig);
            }, "{0}.{1}() should throw a {2} when the DLL doesn't contain the type of plugin specified in the NTestController.xml file!",
                nameof(PluginFactory), nameof(PluginFactory.GetPlugin), nameof(TypeLoadException));
        }

        [TestCase]
        public static void GetPlugin_NullXmlNode_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                PluginFactory.GetPlugin(null, "NullPlugin.dll");
            }, "{0}.{1}() should throw a {2} when a null XmlNode is passed in!",
                nameof(PluginFactory), nameof(PluginFactory.GetPlugin), nameof(ArgumentNullException));
        }

        [TestCase]
        public static void GetPlugin_ValidArgs_ReturnsIPlugin()
        {
            IPlugin plugin = null;
            string xmlConfigPath = "GetPlugin_ValidArgs_ReturnsIPlugin.xml";

            // If the XML file was left over from a previous run, delete it.
            if (File.Exists(xmlConfigPath))
            {
                File.Delete(xmlConfigPath);
            }

            try
            {
                // Create the XML file.
                using (StreamWriter writer = File.CreateText(xmlConfigPath))
                {
                    writer.Write(XML_CONFIG);
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<plugin type=\"TestReader\" path=\"NullPlugin.dll\" />");
                XmlNode node = doc.FirstChild;

                Assert.DoesNotThrow(() =>
                {
                    plugin = PluginFactory.GetPlugin(node, xmlConfigPath);
                }, "{0}.{1}() should not throw an exception when passed a valid DLL & XML Config file!",
                    nameof(PluginFactory), nameof(PluginFactory.GetPlugin));

                Assert.NotNull(plugin, "{0}.{1}() returned null when passed a valid DLL & XML Config file!",
                    nameof(PluginFactory), nameof(PluginFactory.GetPlugin));
            }
            finally
            {
                // Delete the XML file.
                if (File.Exists(xmlConfigPath))
                {
                    File.Delete(xmlConfigPath);
                }
            }
        }

        #endregion GetPlugin Tests
    }
}


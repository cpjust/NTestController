using NUnit.Framework;
using System;
using NTestController;

namespace NTestControllerTests
{
    [TestFixture]
    public class PluginFactoryTests
    {
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
        public void LoadPlugin_NullOrWhiteSpaceArgs_ArgumentNullException(string dllFile, string xmlConfig)
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                PluginFactory.LoadPlugin(dllFile, xmlConfig);
            }, "{0}.{1}() should throw a {2} when null arguments are passed in!",
                nameof(PluginFactory), nameof(PluginFactory.LoadPlugin), nameof(ArgumentNullException));
        }

        [TestCase]
        public void LoadPlugin_MissingDll_DllNotFoundException()
        {
            Assert.Throws<DllNotFoundException>(() =>
            {
                PluginFactory.LoadPlugin("foo.dll", "bar.xml");
            }, "{0}.{1}() should throw a {2} when passed a non-existent DLL!",
                nameof(PluginFactory), nameof(PluginFactory.LoadPlugin), nameof(DllNotFoundException));
        }

        [TestCase]
        public void LoadPlugin_ValidArgs_ReturnsIPlugin()
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
    }
}


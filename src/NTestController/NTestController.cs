﻿using System;
using System.Xml;
using System.Collections.Generic;
using Logger;

namespace NTestController
{
    public class NTestController
    {
        #region Member variables

        private static Options _options = new Options();
        private static ILogger Log { get; } = ConsoleLogger.Instance;

        #endregion Member variables

        #region Public functions

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {
            using (var parser = new CommandLine.Parser(with => with.HelpWriter = Console.Error))
            {
                if (parser.ParseArgumentsStrict(args, _options, () => Environment.Exit(-1)))
                {
                    // Set logging level.

                    // Create output directory.

                    // Load plugins.

                    // Execute Reader plugin.

                    // Execute Setup plugin.

                    // Execute Test Executor plugin.

                    // Execute Cleanup plugin.

                    // Execute Reporter plugin.
                }
            }
        }

        #endregion Public functions

        #region Private functions

        private static XmlNode GetDefaultsXmlNode(string configFile)
        {
            try
            {
                var XMLDoc = new XmlDocument();
                XMLDoc.Load(configFile);

                XmlNode defaultsNode = XMLDoc.FirstChild.SelectSingleNode("defaults");
                return defaultsNode;
            }
            catch (Exception e)
            {
                Log.WriteError("\n{0}\n\n{1}", e.Message, _options.GetUsage());
                throw;
            }
        }

        private static IList<IPlatform> GetPlatforms(string configFile, XmlNode defaultsNode)
        {
            var platforms = new List<IPlatform>();

            try
            {
                var XMLDoc = new XmlDocument();
                XMLDoc.Load(configFile);

                XmlNode node = XMLDoc.FirstChild;

                while (node != null)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        XmlNode platformNode = XMLDoc.FirstChild.SelectSingleNode("platform");
                        IPlatform platform = PlatformFactory.CreatePlatform(platformNode, defaultsNode);
                        platforms.Add(platform);
                    }

                    node = node.NextSibling;
                }
            }
            catch (Exception e)
            {
                Log.WriteError("\n{0}\n", e.Message);
                throw;
            }

            return platforms;
        }

        #endregion Private functions
    }
}

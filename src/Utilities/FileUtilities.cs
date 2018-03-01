using System;
using System.IO;

namespace Utilities
{
    public static class FileUtilities
    {
        /// <summary>
        /// Creates a temp file and returns its path and filename.
        /// </summary>
        /// <returns>The temp file path and filename.</returns>
        public static string CreateTempFile()
        {
            // Get the full name of the newly created Temporary file. 
            // Note that the GetTempFileName() method actually creates
            // a 0-byte file and returns the name of the created file.
            string fileName = Path.GetTempFileName();

            // Create a FileInfo object to set the file's attributes.
            FileInfo fileInfo = new FileInfo(fileName);

            // Set the Attribute property of this file to Temporary. 
            // Although this is not completely necessary, the .NET Framework is able 
            // to optimize the use of Temporary files by keeping them cached in memory.
            fileInfo.Attributes = FileAttributes.Temporary;

            return fileName;
        }

        /// <summary>
        /// Appends the line of text to the file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <param name="line">The line of text to write.</param>
        public static void AppendLineToFile(string filename, string line)
        {
            // Write to the file.
            StreamWriter streamWriter = File.AppendText(filename);
            streamWriter.WriteLine(line);
            streamWriter.Flush();
            streamWriter.Close();
        }
    }
}

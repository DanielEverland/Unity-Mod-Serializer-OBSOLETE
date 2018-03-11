using System;
using UMS.Runtime.Deserialization;

namespace UMS.Runtime.Core
{
    public class StaticObjects
    {
        public static string FolderPath { get { return "Static Objects/"; } }

        /// <summary>
        /// Determines whether a file has been loaded
        /// </summary>
        /// <param name="localPath">Relative to the Static Objects folder in a mod</param>
        public static bool Contains(string localPath)
        {
            return Deserializer.SerializedData.ContainsKey(FolderPath + localPath);
        }

        /// <summary>
        /// Returns the byte array of a loaded file
        /// </summary>
        /// <param name="localPath">Relative to the Static Objects folder in a mod</param>
        public static byte[] GetObject(string localPath)
        {
            if (localPath.StartsWith(FolderPath))
                localPath = localPath.Remove(0, localPath.IndexOf('/') + 1);

            if (!Contains(localPath))
                throw new NullReferenceException();

            return Deserializer.SerializedData[FolderPath + localPath];
        }

        /// <summary>
        /// Adds a file to the Static Objects folder. Must be called during serialization
        /// </summary>
        /// <param name="path">Path relative to Static Objects folder</param>
        public static string AddObject(string path, byte[] data)
        {
            string fullPath = FolderPath + path;

            Serializer.AddExtraFile(fullPath, data);

            return fullPath;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocFileCreator.DataObjects;
using Newtonsoft.Json;
using ZimLabs.CoreLib;
using ZimLabs.CoreLib.Extensions;

namespace LocFileCreator
{
    /// <summary>
    /// Provides several different helper functions
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// Contains the path of the file, which contains the image types
        /// </summary>
        private static readonly string ImageTypesFile = Path.Combine(Core.GetBaseFolder(), "ImageTypes.json");

        /// <summary>
        /// Converts a file size into a read able format (byte > kb, mb, gb)
        /// </summary>
        /// <param name="size">The file size</param>
        /// <returns>The formatted file size</returns>
        public static string ToFileSizeString(this long size)
        {
            switch (size)
            {
                case var _ when size < 1024:
                    return $"{size} Bytes";
                case var _ when size >= 1024 && size < Math.Pow(1024, 2):
                    return $"{size / 1024:N2} KB";
                case var _ when size >= Math.Pow(1024, 2) && size < Math.Pow(1024, 3):
                    return $"{size / Math.Pow(1024, 2):N2} MB";
                case var _ when size >= Math.Pow(1024, 3):
                    return $"{size / Math.Pow(1024, 3):N2} GB";
                default:
                    return size.ToString();
            }
        }

        /// <summary>
        /// Loads all "valid" files of the given directory
        /// </summary>
        /// <param name="directory">The path of the directory which contains the image files</param>
        /// <returns>The list with the files</returns>
        public static List<FileEntry> LoadFiles(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException(nameof(directory));

            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"The given directory '{directory}' doesn't exist.");

            var dirInfo = new DirectoryInfo(directory);

            var files = dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);

            return GetValidFiles(files);
        }

        /// <summary>
        /// Checks all found files and returns only the "valid" files (valid file types are stored in the "ImageTypes.json" file)
        /// </summary>
        /// <param name="files">The list with the files which were found in the desired directory</param>
        /// <returns>The list with the valid files</returns>
        private static List<FileEntry> GetValidFiles(FileInfo[] files)
        {
            if (files == null)
                return null;

            if (!files.Any())
                return new List<FileEntry>();

            var imagesTypes = LoadValidImageTypes();

            return files.Where(w => imagesTypes.Any(a => a.EqualsIgnoreCase(w.Extension))).Select(s => (FileEntry) s)
                .ToList();
        }

        /// <summary>
        /// Loads all valid file types (the types are stored in the "ImageTypes.json" file)
        /// </summary>
        /// <returns>The list with the valid image types</returns>
        private static List<string> LoadValidImageTypes()
        {
            if (!File.Exists(ImageTypesFile))
                return new List<string> {".jpg", ".png", ".dds"};

            var content = File.ReadAllText(ImageTypesFile);

            var data = JsonConvert.DeserializeObject<List<string>>(content);

            return data.Select(s => s.StartsWith(".") ? s : $".{s}").ToList();
        }

        /// <summary>
        /// Creates the loc file for the given files
        /// </summary>
        /// <param name="files">The list with the files</param>
        /// <param name="sourceDir">The path of the source directory</param>
        /// <param name="locPath">The path of the place, where the files are stored</param>
        /// <returns>true when successful, otherwise false</returns>
        public static bool CreateLocFile(List<FileEntry> files, string sourceDir, string locPath)
        {
            foreach (var file in files)
            {
                var filenameWithLoc = $"{file.Name}.loc";
                var link = locPath.EndsWith("/") ? $"{locPath}{file.Name}" : $"{locPath}/{file.Name}";
                var destFile = Path.Combine(sourceDir, filenameWithLoc);

                using var writer = new StreamWriter(destFile, false);

                writer.Write(link);

                if (!File.Exists(destFile))
                    return false;
            }

            return true;
        }
    }
}

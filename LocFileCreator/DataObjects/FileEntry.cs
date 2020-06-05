using System;
using System.IO;
using ZimLabs.WpfBase;

namespace LocFileCreator.DataObjects
{
    /// <summary>
    /// Represents a single file
    /// </summary>
    internal sealed class FileEntry : ObservableObject
    {
        /// <summary>
        /// Gets the name of the file
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the extension of the file (with the dot)
        /// </summary>
        public string Extension { get; }

        /// <summary>
        /// Gets the path of the file
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the size of the file in bytes
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// Gets the file size as formatted string
        /// </summary>
        public string SizeString => Size.ToFileSizeString();

        /// <summary>
        /// Backing field for <see cref="Selected"/>
        /// </summary>
        private bool _selected;

        /// <summary>
        /// Gets or sets the value which indicates if the file was selected
        /// </summary>
        public bool Selected
        {
            get => _selected;
            set => SetField(ref _selected, value);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileEntry"/>
        /// </summary>
        /// <param name="file">The file</param>
        private FileEntry(FileInfo file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            Name = file.Name;
            Extension = file.Extension;
            Path = file.FullName;
            Size = file.Length;
        }

        /// <summary>
        /// Converts a <see cref="FileInfo"/> object into a <see cref="FileEntry"/> object
        /// </summary>
        /// <param name="file">The original object</param>
        public static explicit operator FileEntry(FileInfo file)
        {
            return new FileEntry(file);
        }
    }
}

//-------------------------------------------------------------------------------------
//  Copyright 2012 Rutger Spruyt
//
//  This file is part of C# Comicviewer.
//
//  csharp comicviewer is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  csharp comicviewer is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with csharp comicviewer.  If not, see <http://www.gnu.org/licenses/>.
//-------------------------------------------------------------------------------------

using System.IO;
using CSharpComicLoader;
using CSharpComicLoader.File;

namespace CSharpComicViewer.File
{
    /// <summary>
    /// Implementation of the IFileLoader.
    /// </summary>
    public class FileLoader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileLoader"/> class.
        /// </summary>
        public FileLoader()
        {
            PageType = PageType.Archive;
        }

        /// <summary>
        /// Gets or sets the type of the page.
        /// </summary>
        /// <value>
        /// The type of the page.
        /// </value>
        public PageType PageType { get; set; }

        /// <summary>
        /// Gets the loaded file data.
        /// </summary>
        public LoadedFilesData LoadedFileData { get; private set; }

        /// <summary>
        /// Gets the error.
        /// </summary>
        public string Error { get; private set; }

        private ArchiveLoadAsync _asyncLoader;

        /// <summary>
        /// Loads the specified files.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns>Returns <c>true</c> if successful; Otherwise <c>false</c>.</returns>
        public bool Load(string[] files)
        {
            Error = string.Empty;

            PageType = Utils.ValidateImageFileExtension(files[0]) ? PageType.Image : PageType.Archive;
            for (int i = 1; i < files.Length; i++ )
            {
                if (Utils.ValidateImageFileExtension(files[i]) && PageType != PageType.Image)
                {
                    Error = "Please select only archives or only images.";
                    return true;
                }
            }

            if (PageType == PageType.Archive)
            {
                //var archiveLoader = new ArchiveLoader();
                //LoadedFileData = archiveLoader.LoadComicBook(files);
                if (_asyncLoader == null)
                    _asyncLoader = new ArchiveLoadAsync();
                LoadedFileData = _asyncLoader.LoadComicBook(files);
            }
            else if (PageType == PageType.Image)
            {
                var imageLoader = new ImageLoader();
                // Q&D: if the user selects only one image, load all images in the folder
                if (files.Length == 1)
                    LoadedFileData = imageLoader.LoadComicBook(Path.GetDirectoryName(files[0]));
                else
                    LoadedFileData = imageLoader.LoadComicBook(files);
            }

            Error = LoadedFileData.Error;
            return string.IsNullOrEmpty(Error);
        }
    }

    /// <summary>
    /// The type of page that is loaded.
    /// </summary>
    public enum PageType
    {
        /// <summary>
        /// An archive (.zip,.rar,.cbz,.cbr)
        /// </summary>
        Archive = 0,

        /// <summary>
        /// Images (.jpg,.bmp,.png)
        /// </summary>
        Image
    }
}

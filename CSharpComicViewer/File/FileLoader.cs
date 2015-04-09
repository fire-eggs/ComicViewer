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

using System;
using System.Collections;
using CSharpComicLoader.Comic;
using CSharpComicLoader.File;

namespace CSharpComicViewer
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
			this.PageType = PageType.Archive;
		}

		/// <summary>
		/// Gets or sets the type of the page.
		/// </summary>
		/// <value>
		/// The type of the page.
		/// </value>
		public PageType PageType
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the loaded file data.
		/// </summary>
		public LoadedFilesData LoadedFileData { get; private set; }

		/// <summary>
		/// Gets the error.
		/// </summary>
		public string Error { get; private set; }

		/// <summary>
		/// Loads the specified files.
		/// </summary>
		/// <param name="files">The files.</param>
		/// <returns>Returns <c>true</c> if successful; Otherwise <c>false</c>.</returns>
		public bool Load(string[] files)
		{
			this.PageType = PageType.Archive;
			bool returnValue;

			//Check for file types.
			foreach (string file in files)
			{
				int startExtension = file.ToLower().LastIndexOf('.');
				if (startExtension < 0)
				{
					//File does not have an extension so skip it.
					break;
				}

				string extension = file.ToLower().Substring(startExtension + 1);
				SupportedImages empty;
				if (Enum.TryParse<SupportedImages>(extension, true, out empty))
				{
					this.PageType = PageType.Image;
					break;
				}
				else if (this.PageType == PageType.Image)
				{
					this.Error = "Please select only archives or only images.";
				}
			}

			if (string.IsNullOrEmpty(this.Error))
			{
				if (PageType == PageType.Archive)
				{
					ArchiveLoader archiveLoader = new ArchiveLoader();
					LoadedFileData = archiveLoader.LoadComicBook(files);

					this.Error = LoadedFileData.Error;
				}
				else if (PageType == PageType.Image)
				{
					ImageLoader imageLoader = new ImageLoader();
					LoadedFileData = imageLoader.LoadComicBook(files);

					this.Error = LoadedFileData.Error;
				}
			}

			returnValue = string.IsNullOrEmpty(this.Error) ? true : false;
			return returnValue;
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

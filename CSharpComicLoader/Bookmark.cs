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
using System.Xml;
using System.Xml.Serialization;

namespace CSharpComicLoader
{
	/// <summary>
	/// A Bookmark
	/// </summary>
	[XmlRoot("Bookmark")]
	public class Bookmark
	{
		/// <summary>
		/// The files
		/// </summary>
		private string[] files;

		/// <summary>
		/// Current file being read.
		/// </summary>
		private int currentFile;

		/// <summary>
		/// Current page of file being read.
		/// </summary>
		private int currentPageOfFile;

		/// <summary>
		/// Initializes a new instance of the <see cref="Bookmark"/> class.
		/// </summary>
		public Bookmark()
		{
			//Needed for serialize.
		}

		/// <summary>
		/// Initializes a new instance of the Bookmark class.
		/// </summary>
		/// <param name="files">The files.</param>
		/// <param name="currentFile">Current file being read.</param>
		/// <param name="currentPageOfFile">Current page of file being read.</param>
		public Bookmark(string[] files, int currentFile, int currentPageOfFile)
		{
			this.Files = files;
			this.FileNumber = currentFile;
			this.PageNumber = currentPageOfFile;
		}

		/// <summary>
		/// Gets or sets the files
		/// </summary>
		[XmlArray("Files")]
		public string[] Files
		{
			get { return files; }
			set { files = value; }
		}

		/// <summary>
		/// Gets or sets current file being read.
		/// </summary>
		[XmlElement("CurrentFile")]
		public int FileNumber
		{
			get { return currentFile; }
			set { currentFile = value; }
		}

		/// <summary>
		/// Gets or sets current page of file being read.
		/// </summary>
		[XmlElement("CurrentPage")]
		public int PageNumber
		{
			get { return currentPageOfFile; }
			set { currentPageOfFile = value; }
		}

		/// <summary>
		/// Gets the name of the current file.
		/// </summary>
		[XmlIgnore]
		public string CurrentFileName
		{
			get { return GetCurrentFileName(); }
		}

		/// <summary>
		/// Gets the location of the directory of the current file.
		/// </summary>
		[XmlIgnore]
		public string CurrentFileDirectoryLocation
		{
			get { return GetCurrentFileDirectoryLocation(); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether bookmark manager should delet this bookmark.
		/// </summary>
		[XmlIgnore]
		public bool Delete { get; set; }

		/// <summary>
		/// Get the directory location of the CurrentFile.
		/// </summary>
		/// <returns>directory location of the current file.</returns>
		public string GetCurrentFileDirectoryLocation()
		{
			string filePath = files[currentFile];
			string[] filePathSplit = filePath.Split('\\');
			string directory = "";
			for (int i = 0; i < filePathSplit.Length - 1; i++)
			{
				directory += filePathSplit[i] + "\\";
			}

			return directory;
		}

		/// <summary>
		/// Get the file name of the CurrentFile.
		/// </summary>
		/// <returns>Filename of current file.</returns>
		private string GetCurrentFileName()
		{
			string filePath = files[currentFile];
			string[] filePathSplit = filePath.Split('\\');
			string fileNameWithExtension = filePathSplit[filePathSplit.Length - 1];
			filePathSplit = fileNameWithExtension.Split('.');
			string filename = "";
			for (int i = 0; i < filePathSplit.Length - 1; i++)
			{
				filename += filePathSplit[i];
			}

			return filename;
		}
	}
}

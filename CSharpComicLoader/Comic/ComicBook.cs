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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpComicLoader.Comic
{
	/// <summary>
	/// A comic book object.
	/// </summary>
	public class ComicBook : List<ComicFile>
	{
		private ComicFile _currentFile;

		/// <summary>
		/// Gets or sets the current file.
		/// </summary>
		/// <value>
		/// The current file.
		/// </value>
		public ComicFile CurrentFile
		{
			get
			{
				if (_currentFile == null && this.Count > 0)
				{
					_currentFile = this[0];
				}

				return _currentFile;
			}
			set
			{
				_currentFile = value;
			}
		}

		public int CurrentFileNumber
		{
			get
			{
				return CurrentFile != null ? this.IndexOf(CurrentFile) + 1 : -1;
			}
		}

		/// <summary>
		/// Gets the current page number.
		/// </summary>
		public int CurrentPageNumber
		{
			get
			{
				int result = this.Count > 0 ? 0 : -1;

				foreach (ComicFile file in this)
				{
					if (file == CurrentFile)
					{
						foreach (byte[] page in file)
						{
							if (page == file.CurrentPage)
							{
								result++;
								break;
							}
							else
							{
								result++;
							}
						}
						break;
					}
					else
					{
						result += file.TotalPages;
					}

				}

				return result;
			}
		}

		/// <summary>
		/// Gets the total pages.
		/// </summary>
		public int TotalPages
		{
			get
			{
				int result = 0;

				foreach (ComicFile file in this)
				{
					result += file.Count;
				}

				return result;
			}
		}

		/// <summary>
		/// Gets the total files.
		/// </summary>
		public int TotalFiles
		{
			get
			{
				return this.Count;
			}
		}

		/// <summary>
		/// Get next the page.
		/// </summary>
		/// <returns>The next page as byte[]</returns>
		public byte[] NextPage()
		{
			byte[] result = null;

			if (CurrentFile != null && CurrentPageNumber != TotalPages)
			{
				result = CurrentFile.NextPage();
			}

			if (result == null)
			{
				result = NextFile();
			}

			return result;
		}

		/// <summary>
		/// Gets the first page of the next file.
		/// </summary>
		/// <returns>A byte[] if successful, null if there is no next file</returns>
		public byte[] NextFile()
		{
			byte[] result = null;

			if (CurrentFile != null)
			{
				if (this.IndexOf(CurrentFile) != this.Count - 1)
				{
					CurrentFile = this[this.IndexOf(CurrentFile) + 1];
					result = CurrentFile[0];
				}
			}

			return result;
		}

		/// <summary>
		/// Get the previous page.
		/// </summary>
		/// <returns>The previous page as byte[]</returns>
		public byte[] PreviousPage()
		{
			byte[] result = null;

			if (CurrentFile != null && CurrentPageNumber != 0)
			{
				result = CurrentFile.PreviousPage();
			}

			if (result == null)
			{
				result = PreviousFile();
			}

			return result;
		}

		/// <summary>
		/// Gets the last page of the previous file.
		/// </summary>
		/// <returns>A byte[] if successful, null if there is no previous file</returns>
		public byte[] PreviousFile()
		{
			byte[] result = null;

			if (CurrentFile != null)
			{
				if (this.IndexOf(CurrentFile) != 0)
				{
					CurrentFile = this[this.IndexOf(CurrentFile) - 1];
					result = CurrentFile[CurrentFile.Count - 1];
				}
			}

			return result;
		}

		/// <summary>
		/// Gets the bookmark.
		/// </summary>
		/// <returns>The bookmark.</returns>
		public Bookmark GetBookmark()
		{
			Bookmark result;
			List<string> fileLocations = new List<string>();
			foreach (ComicFile comicFile in this)
			{
				fileLocations.Add(comicFile.Location);
			}

			result = new Bookmark(fileLocations.ToArray(), this.IndexOf(CurrentFile), CurrentFile.CurrentPageNumber - 1);
			return result;
		}

		/// <summary>
		/// Get a page (image) of the ComicBook.
		/// </summary>
		/// <param name="pageNumber">Index number of page from the current ComicFile.</param>
		/// <returns></returns>
		public byte[] GetPage(int pageNumber)
		{
			byte[] result = null;

			if (CurrentFile != null && pageNumber < CurrentFile.Count)
			{
				CurrentFile.CurrentPage = CurrentFile[pageNumber];
				result = CurrentFile.CurrentPage;
			}

			return result;
		}

		/// <summary>
		/// Get a page (image) of the ComicBook.
		/// </summary>
		/// <param name="fileNumber">Index number of the ComicFile.</param>
		/// <param name="pageNumber">Index number of page from the ComicFile.</param>
		/// <returns>The requested image.</returns>
		public byte[] GetPage(int fileNumber, int pageNumber)
		{
			byte[] result = null;

			if (TotalFiles > 0 && fileNumber < this.Count)
			{
				if (pageNumber < this[fileNumber].Count)
				{

					CurrentFile = this[fileNumber];
					CurrentFile.CurrentPage = CurrentFile[pageNumber];

					result = CurrentFile.CurrentPage;
				}
			}

			return result;
		}
	}

}

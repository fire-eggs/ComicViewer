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

using System.Collections.Generic;
using System.Linq;

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
				if (_currentFile == null && Count > 0)
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
				return CurrentFile != null ? IndexOf(CurrentFile) + 1 : -1;
			}
		}

		/// <summary>
		/// Gets the current page number.
		/// </summary>
		public int CurrentPageNumber
		{
			get
			{
				int result = Count > 0 ? 0 : -1;

				foreach (ComicFile file in this)
				{
				    if (file == CurrentFile)
					{
						foreach (byte[] page in file)
						{
                            result++;
                            if (page == file.CurrentPage)
							{
								break;
							}
						}
					    break;
					}
				    result += file.TotalPages;
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
			    return this.Sum(file => file.Count);
			}
		}

		/// <summary>
		/// Gets the total files.
		/// </summary>
		public int TotalFiles
		{
			get
			{
				return Count;
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

		    return result ?? NextFile();
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
				if (IndexOf(CurrentFile) != Count - 1)
				{
					CurrentFile = this[IndexOf(CurrentFile) + 1];
					result = CurrentFile[0];
				}
			}

			return result;
		}

	    /// <summary>
	    /// Get the previous page.
	    /// </summary>
	    /// <param name="doublePage"> </param>
	    /// <returns>The previous page as byte[]</returns>
	    public byte[] PreviousPage(bool doublePage)
		{
			byte[] result = null;

			if (CurrentFile != null && CurrentPageNumber != 0)
			{
				result = CurrentFile.PreviousPage(doublePage);
			}

	        return result ?? PreviousFile();
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
				if (IndexOf(CurrentFile) != 0)
				{
					CurrentFile = this[IndexOf(CurrentFile) - 1];
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
		    if (CurrentFile == null)
                return null;

			return new Bookmark(this.Select(comicFile => comicFile.Location).ToArray(), IndexOf(CurrentFile), CurrentFile.CurrentPageNumber - 1);
		}

		/// <summary>
		/// Get a page (image) of the ComicBook.
		/// </summary>
		/// <param name="pageIndex">Index number of page from the current ComicFile.</param>
		/// <returns></returns>
		public byte[] GetPage(int pageIndex)
		{
            if (CurrentFile == null || TotalFiles == 0)
                return null;
			return CurrentFile.SetIndex(pageIndex);
		}

		/// <summary>
		/// Get a page (image) of the ComicBook.
		/// </summary>
		/// <param name="fileIndex">Index number of the ComicFile.</param>
		/// <param name="pageIndex">Index number of page from the ComicFile.</param>
		/// <returns>The requested image.</returns>
		public byte[] GetPage(int fileIndex, int pageIndex)
		{
            if (TotalFiles == 0 || fileIndex >= Count || fileIndex < 0)
                return null;
            CurrentFile = this[fileIndex];
            return CurrentFile.SetIndex(pageIndex);
		}
	}

}

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
	/// A comic file object. This corresponds to an actual file, either an archive or an image.
	/// </summary>
	public class ComicFile : List<byte[]>
	{
		private byte[] _currentPage;

		/// <summary>
		/// Gets or sets the current page.
		/// </summary>
		/// <value>
		/// The current page.
		/// </value>
		public byte[] CurrentPage
		{
			get
			{
				if (_currentPage == null && this.Count > 0)
				{
					_currentPage = this[0];
				}

				return _currentPage;
			}
			set
			{
				_currentPage = value;
			}
		}

		/// <summary>
		/// Gets or sets the info text.
		/// </summary>
		/// <value>
		/// The info text.
		/// </value>
		public string InfoText { get; set; }

		/// <summary>
		/// Gets or sets the file location.
		/// </summary>
		/// <value>
		/// The file location.
		/// </value>
		public string Location { get; set; }


		/// <summary>
		/// Gets the name of the file.
		/// </summary>
		/// <value>
		/// The name of the file.
		/// </value>
		public string FileName
		{
			get
			{
				string filePath = Location;
				string[] filePathSplit = filePath.Split('\\');
				string fileNameWithExtension = filePathSplit[filePathSplit.Length - 1];
				return fileNameWithExtension;
			}
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public string Name { get; set; }

		/// <summary>
		/// Gets the current page number.
		/// </summary>
		public int CurrentPageNumber
		{
			get
			{
				int result = this.Count > 0 ? 0 : -1;

				foreach (byte[] page in this)
				{
					if (page == CurrentPage)
					{
						result++;
						break;
					}
					else
					{
						result++;
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
				return this.Count;
			}
		}

		/// <summary>
		/// Gets the next page.
		/// </summary>
		/// <returns>The next page as byte[]</returns>
		public byte[] NextPage()
		{
			byte[] result = null;

			if (CurrentPage != null && CurrentPageNumber != TotalPages)
			{
				result = this[this.IndexOf(CurrentPage) + 1];
				CurrentPage = result;
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

			if (CurrentPage != null && CurrentPageNumber != 1)
			{
				result = this[this.IndexOf(CurrentPage) - 1];
				CurrentPage = result;
			}

			return result;
		}
	}
}

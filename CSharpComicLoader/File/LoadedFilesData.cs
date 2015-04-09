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
using CSharpComicLoader.Comic;

namespace CSharpComicLoader.File
{
	/// <summary>
	/// The object that is the result of a file load.
	/// </summary>
	public class LoadedFilesData
	{
		/// <summary>
		/// Gets or sets the comic book.
		/// </summary>
		/// <value>
		/// The comic book.
		/// </value>
		public ComicBook ComicBook { get; set; }

		/// <summary>
		/// Gets or sets the error.
		/// </summary>
		/// <value>
		/// The error.
		/// </value>
		public string Error { get; set; }

		/// <summary>
		/// Gets a value indicating whether this instance has file.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance has file; otherwise, <c>false</c>.
		/// </value>
		public bool HasFile
		{
			get
			{
				if (ComicBook != null)
				{
					return ComicBook.Count > 0;
				}
				else
				{
					return false;
				}
			}
		}
	}
}

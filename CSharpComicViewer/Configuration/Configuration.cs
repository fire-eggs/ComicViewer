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
using System.Collections.Generic;
using CSharpComicLoader;

namespace CSharpComicViewer.Configuration
{
	/// <summary>
	/// Configuration that is saved to an xml file
	/// </summary>
	public class Configuration
	{
		/// <summary>
		/// Gets or sets the resume.
		/// </summary>
		/// <value>
		/// The resume data.
		/// </value>
		public Bookmark Resume { get; set; }

		/// <summary>
		/// Gets or sets the bookmarks.
		/// </summary>
		/// <value>
		/// The bookmarks.
		/// </value>
		public List<Bookmark> Bookmarks { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to override height.
		/// </summary>
		/// <value>
		///   <c>true</c> if height should be overridden; otherwise, <c>false</c>.
		/// </value>
		public bool OverideHeight { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to override width.
		/// </summary>
		/// <value>
		///   <c>true</c> if width should be overridden; otherwise, <c>false</c>.
		/// </value>
		public bool OverideWidth { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Configuration"/> is windowed.
		/// </summary>
		/// <value>
		///   <c>true</c> if windowed; otherwise, <c>false</c>.
		/// </value>
		public bool Windowed { get; set; }
	}
}

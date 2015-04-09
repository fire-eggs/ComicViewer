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
using CSharpComicLoader.File;

namespace CSharpComicLoader
{
	/// <summary>
	/// Utilities
	/// </summary>
	public static class Utils
	{
		/// <summary>
		/// Validates the archive file extension.
		/// </summary>
		/// <param name="FilePath">The file path.</param>
		/// <returns></returns>
		public static bool ValidateArchiveFileExtension(string FilePath)
		{
			bool returnValue = false;

			string[] supportedExtensions = Enum.GetNames(typeof(SupportedArchives));

			foreach (string extension in supportedExtensions)
			{
				if (FilePath.EndsWith("." + extension, StringComparison.OrdinalIgnoreCase))
				{
					returnValue = true;
					break;
				}
			}

			return returnValue;
		}

		/// <summary>
		/// Validates the image file extension.
		/// </summary>
		/// <param name="FilePath">The file path.</param>
		/// <returns>true if the file extension is a supported image extension</returns>
		public static bool ValidateImageFileExtension(string FilePath)
		{
			string[] supportedExtensions = Enum.GetNames(typeof(SupportedImages));
		    string ext = System.IO.Path.GetExtension(FilePath);
            if (string.IsNullOrEmpty(ext))
                return false;
            ext = ext.Substring(1).ToLower(); // skip the leading '.'

			foreach (string extension in supportedExtensions)
			{
                if (extension.CompareTo(ext) == 0)
				{
				    return true;
				}
			}
		    return false;
		}

		/// <summary>
		/// Validates the text file extension.
		/// </summary>
		/// <param name="FilePath">The file path.</param>
		/// <returns></returns>
		public static bool ValidateTextFileExtension(string FilePath)
		{
			bool returnValue = false;

			string[] supportedExtensions = Enum.GetNames(typeof(SupportedTextFiles));

			foreach (string extension in supportedExtensions)
			{
				if (FilePath.EndsWith("." + extension, StringComparison.OrdinalIgnoreCase))
				{
					returnValue = true;
					break;
				}
			}

			return returnValue;
		}

		/// <summary>
		/// Gets the file loader filter.
		/// </summary>
		public static string FileLoaderFilter
		{
			get
			{
				string returnValue = "";

				string[] supportedArchives = Enum.GetNames(typeof(SupportedArchives));
				string[] supportedImages = Enum.GetNames(typeof(SupportedImages));

				//Add Archives to filter
				returnValue += "Supported archive formats (";

				foreach (string extension in supportedArchives)
				{
					returnValue += "*." + extension + ";";
				}

				returnValue += ")|";

				foreach (string extension in supportedArchives)
				{
					returnValue += "*." + extension + ";";
				}

				//Add separator
				returnValue += "|";

				//Add Images to filter
				returnValue += "Supported image formats (";

				foreach (string extension in supportedImages)
				{
					returnValue += "*." + extension + ";";
				}

				returnValue += ")|";

				foreach (string extension in supportedImages)
				{
					returnValue += "*." + extension + ";";
				}

				//Add *.*
				returnValue += "|All files (*.*)|*.*";

				return returnValue;
			}
		}
	}
}

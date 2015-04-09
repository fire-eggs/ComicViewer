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
using System.IO;
using System.Linq;

namespace CSharpComicLoader
{
	/// <summary>
	/// Find and locate next and previous usable files(archives) in current dir
	/// </summary>
	public class FileNextPrevious
	{
		/// <summary>
		/// Gets the next file in directory.
		/// </summary>
		/// <param name="currentFilePath">The current file path.</param>
		/// <returns></returns>
		public string GetNextFileInDirectory(string currentFilePath)
		{
			string returnValue = "";

			string directory = Directory.GetParent(currentFilePath).FullName;
			List<string> supportedFiles = Directory.GetFiles(directory).Where(file => Utils.ValidateArchiveFileExtension(file)).ToList();
			supportedFiles.Sort();

			int currentFileIndex = supportedFiles.IndexOf(currentFilePath);

			if (currentFileIndex != -1 && currentFileIndex <= supportedFiles.Count)
			{
				returnValue = supportedFiles[currentFileIndex + 1];
			}

			return returnValue;
		}

		/// <summary>
		/// Gets the previous file in directory.
		/// </summary>
		/// <param name="currentFilePath">The current file path.</param>
		/// <returns></returns>
		public string GetPreviousFileInDirectory(string currentFilePath)
		{
			string returnValue = "";

			string directory = Directory.GetParent(currentFilePath).FullName;
			List<string> supportedFiles = Directory.GetFiles(directory).Where(file => Utils.ValidateArchiveFileExtension(file)).ToList();
			supportedFiles.Sort();

			int currentFileIndex = supportedFiles.IndexOf(currentFilePath);

			if (currentFileIndex != -1 && currentFileIndex != 0)
			{
				returnValue = supportedFiles[currentFileIndex - 1];
			}

			return returnValue;
		}
	}
}

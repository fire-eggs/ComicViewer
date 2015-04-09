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
using System.IO;
using System.Linq;
using CSharpComicLoader.Comic;

namespace CSharpComicLoader.File
{
	/// <summary>
	/// Class used to load loose images.
	/// </summary>
	public class ImageLoader : IFileLoader
	{
		/// <summary>
		/// Gets or sets the total files (images).
		/// </summary>
		/// <value>
		/// The total files.
		/// </value>
		public int TotalFiles { get; set; }


		/// <summary>
		/// Gets or sets the loaded files (images).
		/// </summary>
		/// <value>
		/// The loaded files.
		/// </value>
		public int LoadedFiles { get; set; }

		/// <summary>
		/// Loads the comic book.
		/// </summary>
		/// <param name="files">The files.</param>
		/// <returns></returns>
		public LoadedFilesData LoadComicBook(string[] files)
		{
			LoadedFiles = 0;
			LoadedFilesData returnValue = new LoadedFilesData();
			returnValue.ComicBook = new ComicBook();
			Comic.ComicFile comicFile = new Comic.ComicFile();

			Array.Sort(files);
			FileStream fs;
			bool NextFile = false;

			foreach (string image in files)
			{
				if (!System.IO.File.Exists(image))
				{
					returnValue.Error = "One or more images where not found";
					return returnValue;
				}
			}

			try
			{
				TotalFiles = files.Length;
				foreach (string file in files)
				{
					//open archive

					for (int x = 0; x < Enum.GetNames(typeof(SupportedImages)).Length; x++)
					{
						//if it is an image add it to array list
						if (Utils.ValidateImageFileExtension(file))
						{

							fs = System.IO.File.OpenRead(file);
							fs.Position = 0;
							try
							{
								byte[] b = new byte[fs.Length];
								fs.Read(b, 0, b.Length);
								comicFile.Add(b);
							}
							catch (Exception)
							{
								fs.Close();
								returnValue.Error = "One or more files are corrupted, and where skipped";
								return returnValue;
							}
							fs.Close();
							NextFile = true;
						}

						if (NextFile)
						{
							NextFile = false;
							x = Enum.GetNames(typeof(SupportedImages)).Length;
						}
					}

					//Add a ComicFile
					if (comicFile.Count > 0)
					{
						comicFile.Location = file;
						returnValue.ComicBook.Add(comicFile);
					}
					comicFile = new ComicFile();
					LoadedFiles++;
				}

				//return the ComicBook on success
				return returnValue;
			}
			catch (Exception e)
			{
				//show error and return nothing
				returnValue.Error = e.Message;
				return returnValue;
			}
		}
	}
}

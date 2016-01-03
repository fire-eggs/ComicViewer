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
using System.IO;
using CSharpComicLoader.Comic;

// KBR TODO async loader
// KBR TODO verify there are no file locks

namespace CSharpComicLoader.File
{
	/// <summary>
	/// Class used to load loose images.
	/// </summary>
	public class ImageLoader : IFileLoader
	{
		/// <summary>
		/// Loads the comic book.
		/// </summary>
		/// <param name="files">The files.</param>
		/// <returns>A ComicBook where the files are part of a single ComicFile.</returns>
		public LoadedFilesData LoadComicBook(string[] files)
		{
			var returnValue = new LoadedFilesData();
			returnValue.ComicBook = new ComicBook();
			var comicFile = new ComicFile();

			try
			{
				foreach (string file in files)
				{
                    if (!System.IO.File.Exists(file))
                    {
                        returnValue.Error = "One or more files could not be read, and were skipped";
                        continue; // KBR just skip the file
                    }

                    // KBR TODO wasn't this check already made? [not from the Q&D multi-file loader...]
				    if (!Utils.ValidateImageFileExtension(file))
                        continue; // KBR not a supported image extension, skip it

                    using (var fs = System.IO.File.OpenRead(file))
                    {
                        try
                        {
                            var b = new byte[fs.Length];
                            fs.Read(b, 0, b.Length);
                            comicFile.Add(b);
                        }
                        catch (Exception)
                        {
                            // couldn't read the file, just skip it
                            returnValue.Error = "One or more files could not be read, and were skipped";
                        }
                    }
				}

				//return the ComicBook on success
			    comicFile.Location = ""; // KBR TODO comicFile is now a collection of images, but each image needs to have its own location
                returnValue.ComicBook.Add(comicFile);
                return returnValue;
			}
			catch (Exception e)
			{
				//show error and return nothing
				returnValue.Error = e.Message;
				return returnValue;
			}
		}

        // A quick-and-dirty "load all image files in a folder"
	    public LoadedFilesData LoadComicBook(string path)
	    {
	        string [] allFiles = Directory.GetFiles(path);
	        return LoadComicBook(allFiles);
	    }

	}
}

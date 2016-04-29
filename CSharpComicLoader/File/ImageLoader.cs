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
		/// Loads the comic book from a set of separate image files.
		/// </summary>
		/// <param name="files">The files.</param>
		/// <returns>A ComicBook where each file is a separate ComicFile.</returns>
		public LoadedFilesData LoadComicBook(string[] files)
		{
			var returnValue = new LoadedFilesData();
			returnValue.ComicBook = new ComicBook();

			try
			{
				foreach (string file in files)
				{
                    if (!System.IO.File.Exists(file))
                    {
                        returnValue.Error = "One or more files could not be read, and were skipped";
                        continue; // KBR just skip the file
                    }

                    // KBR Might appear duplicated check, but wasn't performed from the Q&D multi-file loader...
				    if (!Utils.ValidateImageFileExtension(file))
                        continue; // KBR not a supported image extension, skip it

                    using (var fs = System.IO.File.OpenRead(file))
                    {
                        try
                        {
                            var b = new byte[fs.Length];
                            fs.Read(b, 0, b.Length);

                            // Change to prior behavior: load each image as a separate ComicFile. This way we
                            // have a per-image location value we can display.
                            var comicFile = new ComicFile {b};
                            comicFile.Location = file;
                            returnValue.ComicBook.Add(comicFile);
                        }
                        catch (Exception)
                        {
                            // couldn't read the file, just skip it
                            returnValue.Error = "One or more files could not be read, and were skipped";
                        }
                    }
				}

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

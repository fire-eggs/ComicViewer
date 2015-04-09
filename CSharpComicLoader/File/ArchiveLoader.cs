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
using System.IO;
using SevenZip;
using CSharpComicLoader.Comic;
using CSharpComicLoader.File;

namespace CSharpComicLoader.File
{
	/// <summary>
	/// Class used to load archives.
	/// </summary>
	public class ArchiveLoader : IFileLoader
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ArchiveLoader"/> class.
		/// </summary>
		public ArchiveLoader()
		{
			//Get the location of the 7z dll (location .EXE is in)
			String executableName = System.Reflection.Assembly.GetExecutingAssembly().Location;
			FileInfo executableFileInfo = new FileInfo(executableName);

			string dllPath = executableFileInfo.DirectoryName + "//7z.dll";

			//load the 7zip dll
			try
			{
				SevenZipExtractor.SetLibraryPath(dllPath);
			}
			catch (Exception e)
			{
				throw new Exception(String.Format("Unable to load 7z.dll from: {0}", dllPath), e.InnerException);
			}

		}

		/// <summary>
		/// Gets or sets the total files (archives).
		/// </summary>
		/// <value>
		/// The total files.
		/// </value>
		public int TotalFiles { get; set; }


		/// <summary>
		/// Gets or sets the loaded files (archives).
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

			string infoTxt = "";
			MemoryStream ms = new MemoryStream();
			SevenZipExtractor extractor;
			Boolean nextFile = false;

			foreach (String file in files)
			{
				if (!System.IO.File.Exists(file))
				{
					returnValue.Error = "One or more archives where not found";
					return returnValue;
				}
			}

			try
			{
				TotalFiles = files.Length;
				foreach (string file in files)
				{
					//open archive
					extractor = new SevenZipExtractor(file);
					string[] fileNames = extractor.ArchiveFileNames.ToArray();
					Array.Sort(fileNames);

					//create ComicFiles for every single archive
					for (int i = 0; i < extractor.FilesCount; i++)
					{
						for (int x = 0; x < Enum.GetNames(typeof(SupportedImages)).Length; x++)
						{
							//if it is an image add it to array list
							if (Utils.ValidateImageFileExtension(fileNames[i]))
							{
								ms = new MemoryStream();
								extractor.ExtractFile(fileNames[i], ms);
								ms.Position = 0;
								try
								{
									comicFile.Add(ms.ToArray());
								}
								catch (Exception)
								{
									ms.Close();
									returnValue.Error = "One or more files are corrupted, and where skipped";
									return returnValue;
								}

								ms.Close();
								nextFile = true;
							}

							//if it is a txt file set it as InfoTxt
							else if (Utils.ValidateTextFileExtension(fileNames[i]))
							{
								ms = new MemoryStream();
								extractor.ExtractFile(fileNames[i], ms);
								ms.Position = 0;
								try
								{
									StreamReader sr = new StreamReader(ms);
									infoTxt = sr.ReadToEnd();
								}
								catch (Exception)
								{
									ms.Close();
									returnValue.Error = "One or more files are corrupted, and where skipped";
									return returnValue;
								}

								ms.Close();
								nextFile = true;
							}


							if (nextFile)
							{
								nextFile = false;
								x = Enum.GetNames(typeof(SupportedImages)).Length;
							}
						}
					}

					//unlock files again
					extractor.Dispose();

					//Add a ComicFile
					if (comicFile.Count > 0)
					{
						comicFile.Location = file;
						comicFile.InfoText = infoTxt;
						returnValue.ComicBook.Add(comicFile);
						infoTxt = "";
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

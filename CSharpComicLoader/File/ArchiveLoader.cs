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
using System.Linq;
using System.Text.RegularExpressions;
using CSharpComicLoader.Comic;
using SevenZip;

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
                SevenZipBase.SetLibraryPath(dllPath);
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

        public static void NumericalSort(string[] ar)
        {
            Regex rgx = new Regex("([^0-9]*)([0-9]+)");
            Array.Sort(ar, (a, b) =>
            {
                var ma = rgx.Matches(a);
                var mb = rgx.Matches(b);

                // KBR 20150209 'CH' and 'Ch'
                // KBR 20140907 might not be numeric!
                if (ma.Count != mb.Count)
                    return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);

                for (int i = 0; i < ma.Count; ++i)
                {
                    int ret = String.Compare(ma[i].Groups[1].Value, mb[i].Groups[1].Value, StringComparison.OrdinalIgnoreCase);
                    if (ret != 0)
                        return ret;

                    try
                    {
                        // KBR 20141222 integer overflow
                        ret = (int)(long.Parse(ma[i].Groups[2].Value) - long.Parse(mb[i].Groups[2].Value));
                        if (ret != 0)
                            return ret;
                    }
                    catch (Exception) // 20160405 more overflow
                    {
                        return 0;
                    }
                }

                return 0;
            });
        }

        /// <summary>
        /// Loads the comic book.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        public LoadedFilesData LoadComicBook(string[] files)
        {
            LoadedFiles = 0;
            LoadedFilesData returnValue = new LoadedFilesData {ComicBook = new ComicBook()};
            var comicFile = new ComicFile();

            Array.Sort(files);

            string infoTxt = "";
            SevenZipExtractor extractor = null;

            if (files.Any(file => !System.IO.File.Exists(file)))
            {
                returnValue.Error = "One or more archives were not found";
                return returnValue;
            }

            try
            {
                foreach (string file in files)
                {
                    //open archive
                    extractor = new SevenZipExtractor(file);
                    string[] fileNames = extractor.ArchiveFileNames.ToArray();

                    // 20140901 Sort using numeric rules
                    NumericalSort(fileNames);

                    //create ComicFiles for every single archive
                    for (int i = 0; i < extractor.FilesCount; i++)
                    {
                        //if it is an image add it to array list
                        if (Utils.ValidateImageFileExtension(fileNames[i]))
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                extractor.ExtractFile(fileNames[i], ms);
                                ms.Position = 0;
                                try
                                {
                                    comicFile.Add(ms.ToArray());
                                }
                                catch (Exception)
                                {
                                    returnValue.Error = "One or more files are corrupted, and were skipped";
                                    return returnValue;
                                }
                            }
                        }

                        //if it is a txt file set it as InfoTxt
                        if (Utils.ValidateTextFileExtension(fileNames[i]))
                        {
                            var ms = new MemoryStream();
                            extractor.ExtractFile(fileNames[i], ms);
                            ms.Position = 0;
                            StreamReader sr = null;
                            try
                            {
                                sr = new StreamReader(ms);
                                infoTxt = sr.ReadToEnd();
                            }
                            catch (Exception)
                            {
                                returnValue.Error = "One or more files are corrupted, and were skipped";
                                return returnValue;
                            }
                            finally
                            {
                                if (sr != null)
                                    sr.Dispose();
                                ms.Dispose();
                            }
                        }
                    }

                    //unlock files again
                    extractor.Dispose();
                    extractor = null;

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
            finally
            {
                if (extractor != null) extractor.Dispose();
            }
        }
    }
}

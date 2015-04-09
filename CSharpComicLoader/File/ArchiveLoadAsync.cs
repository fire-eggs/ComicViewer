using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CSharpComicLoader.Comic;
using SevenZip;

/*
Asynchronous archive loader.

NOTE: the use of a second extractor is due to 7z limitations. These notes from http://www.codeproject.com/Articles/27148/C-NET-Interface-for-Zip-Archive-DLLs :

The second issue is much smaller one. It is related to multi-threading. If you plan to use 7-Zip interfaces only in one stream you have no problem. 
The problem comes when you try to use one interface in several threads. In this case all threads except the main one (threads where interfaces are 
created) throw exceptions on any interface method calls. This is because of RCW behavior. RCW is an object that wraps COM-interface in .NET. When 
you try to use interface in different thread RCW tries to marshal interface and fails (because this implementation does not support ITypeInfo).
*/

namespace CSharpComicLoader.File
{
    public class ArchiveLoadAsync : IFileLoader
    {
        public ArchiveLoadAsync()
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

        private Thread _t1;
        private string[] _fileNames;

        public LoadedFilesData LoadComicBook(string[] files)
        {
            LoadedFilesData returnValue = new LoadedFilesData { ComicBook = new ComicBook() };
            Array.Sort(files);
            if (files.Any(file => !System.IO.File.Exists(file)))
            {
                returnValue.Error = "One or more archives were not found";
                return returnValue;
            }

            var comicFile = new ComicFile {Location = files[0]};
            returnValue.ComicBook.Add(comicFile);

            int initialFilesToRead;
            using (SevenZipExtractor extractor = new SevenZipExtractor(files[0]))
            {
                // "bye bye love letter" comic has a folder whose name ends in .PNG, and the extractor thinks it is an image
                List<string> tempFileNames = new List<string>();
                foreach (var archiveFileInfo in extractor.ArchiveFileData)
                {
                    if (!archiveFileInfo.IsDirectory)
                        tempFileNames.Add(archiveFileInfo.FileName);
                }
                _fileNames = tempFileNames.ToArray();
                if (_fileNames.Length < 1) // Nothing to show!
                    return returnValue;

                ArchiveLoader.NumericalSort(_fileNames);

                // Load the first 5 files (if possible) before returning to GUI
                initialFilesToRead = (int)Math.Min(5, extractor.FilesCount);
                for (int j = 0; j < initialFilesToRead; j++)
                {
                    ExtractFile(extractor, j, comicFile);
                }
            }

            // Load remaining files in the background
            _t1 = new Thread(() =>
            {
                using (SevenZipExtractor extractor2 = new SevenZipExtractor(files[0])) // need 2d extractor for thread: see comment at top of file
                {
                    for (int i = initialFilesToRead; i < _fileNames.Length; i++)
                    {
                        ExtractFile(extractor2, i, comicFile);
                    }
                }
            });
            _t1.Start();

            return returnValue;
        }

        private void ExtractFile(SevenZipExtractor extractor, int i, ComicFile comicFile)
        {
            //if it is an image add it to array list
            if (Utils.ValidateImageFileExtension(_fileNames[i]))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    try
                    {
                        extractor.ExtractFile(_fileNames[i], ms);
                        ms.Position = 0;
                        comicFile.Add(ms.ToArray());
                    }
                    catch
                    {
                        // Effect of exception will be to NOT add the image to the comic
                    }
                }
            }

            //if it is a txt file set it as InfoTxt
            if (Utils.ValidateTextFileExtension(_fileNames[i]))
            {
                using (var ms = new MemoryStream())
                {
                    extractor.ExtractFile(_fileNames[i], ms);
                    ms.Position = 0;
                    using (var sr = new StreamReader(ms))
                    {
                        try
                        {
                            comicFile.InfoText = sr.ReadToEnd();
                        }
                        catch
                        {
                        }
                    }
                }
            }
            
        }
    }
}

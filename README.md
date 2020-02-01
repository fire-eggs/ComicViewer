**With a Little Help From ...**

![logo1](Files/jetbrains_sm.png) - [JetBrains](https://www.jetbrains.com/) : the Acme of .NET tool suites!

![logo2](Files/deleaker_logo.png) - [Deleaker](https://www.deleaker.com) : the _best_ tool for finding memory, GDI and other leaks!

ComicViewer
===========

A fork of Rutger Spruyt's "C# ComicViewer". See http://sourceforge.net/projects/csharpcomicview/

Now at http://riuujin.github.io/charpcomicviewer-sf

_Description_

A manga/comicbook reader in C#, .NET 4.0 and WPF.  View images on disk, or from within Zip/rar/cbr/cbz/7z files. Uses 
7-zip and SevenZipSharp.

_Features_

For basic features, please see either project at the above links.

Features Specific to this Fork (in no particular order)

- "Goto page" dialog
- Double-page view
- Keyboard shortcuts hint display
- Support GIF as loadable image
- Support NFO as viewable text
- Next / Previous page via mouse "fly-overs"
- Handle various file load errors
- Plugged memory/resource leaks
- Asynchronous archive loading (don't have to wait for the entire zip file to load)
- 01/03/2016: if you open a *single* image in a folder, the program will now open *all* images in the folder.
- 04/02/2016: attempt to handle archives-containing-archives. Not asynchronous.
- 04/30/2016: most-recently-used file list under File menu
- 01/26/2018: image files would stop loading on exception (e.g. permissions, read error)

_Install_

1. Copy the four files from the Executable folder to a directory on your system. 
2. Run csharp-comicviewer.exe. 

Should work on Windows 7 or later without any additional steps.

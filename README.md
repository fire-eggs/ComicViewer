ComicViewer
===========

A fork of Rutger Spruyt's "C# ComicViewer". See http://sourceforge.net/projects/csharpcomicview/

Now at http://riuujin.github.io/charpcomicviewer-sf

_Description_

A manga/comicbook reader in C#, .NET 4.0 and WPF.  View images on disk, or from within Zip/rar/cbr/cbz/7z files. Uses 
7-zip and SevenZipSharp.

_Features_

For basic features, please see the SourceForge project at the above link.

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

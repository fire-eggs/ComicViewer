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

using System.Collections.Generic;

namespace CSharpComicLoader.Comic
{
    /// <summary>
    /// A comic file object. This corresponds to an actual file, either an archive or an image.
    /// </summary>
    public class ComicFile : List<byte[]>
    {
        private int _currentIndex;

        /// <summary>
        /// Gets the current page.
        /// </summary>
        public byte[] CurrentPage { get { return this[_currentIndex]; } }

        /// <summary>
        /// Gets or sets the info text.
        /// </summary>
        /// <value>
        /// The info text.
        /// </value>
        public string InfoText { get; set; }

        /// <summary>
        /// Gets or sets the file location.
        /// </summary>
        /// <value>
        /// The file location.
        /// </value>
        public string Location { get; set; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName
        {
            get
            {
                string filePath = Location;
                string[] filePathSplit = filePath.Split('\\');
                string fileNameWithExtension = filePathSplit[filePathSplit.Length - 1];
                return fileNameWithExtension;
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the current page number.
        /// </summary>
        public int CurrentPageNumber { get { return _currentIndex + 1; } }

        /// <summary>
        /// Gets the total pages.
        /// </summary>
        public int TotalPages { get { return Count; } }

        /// <summary>
        /// Gets the next page.
        /// </summary>
        /// <returns>The next page as byte[]</returns>
        public byte[] NextPage()
        {
            return SetIndex(_currentIndex + 1, 1);
        }

        /// <summary>
        /// Get the previous page.
        /// </summary>
        /// <param name="doublePage"> </param>
        /// <returns>The previous page as byte[]</returns>
        public byte[] PreviousPage(bool doublePage)
        {
            // If not in doublepage mode, go back one page. If in double-page mode, we typically go back "3" pages
            // (the "current page" is actually set to the second, or even-numbered page of the pair).
            int decr = doublePage ? 3 : 1;
            return SetIndex(_currentIndex - decr, -decr);
        }

        /// <summary>
        /// Go to a specific page.
        /// </summary>
        /// <param name="pageIndex">Desired target page.</param>
        /// <param name="direction">The direction in which we are turning pages. If we hit an invalid page, continue in this direction if possible.</param>
        /// <returns>The target page, or null if the target page is out of range, or no valid page can be reached.</returns>
        public byte[] SetIndex(int pageIndex, int direction = 1)
        {
            if (pageIndex < 0 || pageIndex >= Count)
                return null;
            int oldIndex = _currentIndex;
            _currentIndex = pageIndex;
            
            while ((direction ==  1 && _currentIndex < Count) ||
                   (direction < 0 && _currentIndex >= 0))
            {
                byte[] tempPage = this[_currentIndex];

                // Gon Vol 3 failed decompression of a page with zero bytes
                if (tempPage != null && tempPage.Length > 1 && tempPage[0] != 0 && tempPage[1] != 0)
                    return tempPage;
                _currentIndex += direction;
            }
            _currentIndex = oldIndex;
            return null;
        }
    }
}

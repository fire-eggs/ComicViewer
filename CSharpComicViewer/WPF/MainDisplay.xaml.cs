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
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Serialization;
using CSharpComicLoader;
using CSharpComicLoader.Comic;
using CSharpComicViewer.File;
using Microsoft.Win32;

namespace CSharpComicViewer.WPF
{
    /// <summary>
    /// Interaction logic for MainDisplay
    /// </summary>
    public partial class MainDisplay
    {
        #region Properties

        /// <summary>
        /// Gets or sets the opening file.
        /// </summary>
        /// <value>
        /// The opening file.
        /// </value>
        private readonly string _openingFile;

        /// <summary>
        /// Gets or sets the comic book.
        /// </summary>
        /// <value>
        /// The comic book.
        /// </value>
        private ComicBook _comicBook;

        /// <summary>
        /// Gets or sets the next page count.
        /// </summary>
        /// <value>
        /// The next page count.
        /// </value>
        private int _nextPageCount = 2;

        /// <summary>
        /// Gets or sets the previous page count.
        /// </summary>
        /// <value>
        /// The previous page count.
        /// </value>
        private int _previousPageCount = 2;

        /// <summary>
        /// The comic page (ea the displayed image)
        /// </summary>
        private ImageUtils _imageUtils;

        /// <summary>
        /// The file loader.
        /// </summary>
        private FileLoader _fileLoader;

        /// <summary>
        /// The show message timer.
        /// </summary>
        private DispatcherTimer _showMessageTimer;

        /// <summary>
        /// The page information timer.
        /// </summary>
        private DispatcherTimer _pageInformationTimer;

        /// <summary>
        /// The last mouse move.
        /// </summary>
        private DateTime _lastMouseMove;

        /// <summary>
        /// Gets or sets a value indicating whether mouse is hidden.
        /// </summary>
        /// <value>
        ///   <c>true</c> if mouse is hidden; otherwise, <c>false</c>.
        /// </value>
        private bool _mouseIsHidden;

        /// <summary>
        /// Gets or sets the scroll value horizontal.
        /// </summary>
        /// <value>
        /// The scroll value horizontal.
        /// </value>
        private int _scrollValueHorizontal;

        /// <summary>
        /// Gets or sets the current mouse position.
        /// </summary>
        /// <value>
        /// The current mouse position.
        /// </value>
        private Point _currentMousePosition;

        /// <summary>
        /// Gets or sets the mouse X.
        /// </summary>
        /// <value>
        /// The mouse X.
        /// </value>
        private double _mouseX;

        /// <summary>
        /// Gets or sets the mouse Y.
        /// </summary>
        /// <value>
        /// The mouse Y.
        /// </value>
        private double _mouseY;

        /// <summary>
        /// Gets or sets a value indicating whether mouse drag.
        /// </summary>
        /// <value>
        ///   <c>true</c> if mouse drag; otherwise, <c>false</c>.
        /// </value>
        private bool _mouseDrag;

        /// <summary>
        /// Gets or sets a value indicating whether going to next page is allowed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if going to next page is allowed; otherwise, <c>false</c>.
        /// </value>
        private bool _nextPageBoolean;

        /// <summary>
        /// Gets or sets a value indicating whether going to previous page is allowed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if going to previous page is allowed; otherwise, <c>false</c>.
        /// </value>
        private bool _previousPageBoolean;

        /// <summary>
        /// Gets or sets the timeout to hide.
        /// </summary>
        /// <value>
        /// The timeout to hide.
        /// </value>
        private TimeSpan _timeoutToHide;

        /// <summary>
        /// Gets or sets the mouse idle.
        /// </summary>
        /// <value>
        /// The mouse idle.
        /// </value>
        public DispatcherTimer MouseIdle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public Configuration.Configuration Configuration
        {
            get;
            private set;
        }

        /// <summary>
        /// Start position on the image
        /// </summary>
        public enum ImageStartPosition
        {
            /// <summary>
            /// Start at the start of the page.
            /// </summary>
            Top,

            /// <summary>
            /// Start at the bottom of the page.
            /// </summary>
            Bottom
        }

        /// <summary>
        /// The window mode.
        /// </summary>
        public enum WindowMode
        {
            /// <summary>
            /// Display in a fullscreen.
            /// </summary>
            Fullscreen,

            /// <summary>
            /// Display in a window.
            /// </summary>
            Windowed
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainDisplay"/> class.
        /// </summary>
        /// <param name="openingFile">The opening file.</param>
        public MainDisplay(string openingFile)
        {
            InitializeComponent();
            _openingFile = openingFile;
        }

        /// <summary>
        /// Handles the Loaded event of the MainDisplay control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MainDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            //Ensure that the window is active on start
            Activate();

            _imageUtils = new ImageUtils();
            _fileLoader = new FileLoader();

            //set mouse idle timer
            _timeoutToHide = TimeSpan.FromSeconds(2);
            MouseIdle = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            MouseIdle.Tick += MouseIdleChecker;
            MouseIdle.Start();

            //Load config
            LoadConfiguration();
            SetBookmarkMenus();

            if (Configuration.Windowed)
            {
                SetWindowMode(WindowMode.Windowed);
            }

            //gray out resume last file if the files dont't exist
            if (Configuration.Resume != null)
            {
                foreach (string file in Configuration.Resume.Files)
                {
                    if (!System.IO.File.Exists(file))
                    {
                        ResumeFile_MenuBar.IsEnabled = false;
                        ResumeFile_RightClick.IsEnabled = false;
                    }
                }
            }
            else
            {
                ResumeFile_MenuBar.IsEnabled = false;
                ResumeFile_RightClick.IsEnabled = false;
            }

            _scrollValueHorizontal = (int)(ScrollField.ViewportHeight * 0.05);
            _imageUtils.ScreenHeight = (int)ScrollField.ViewportHeight;
            _imageUtils.ScreenWidth = (int)ScrollField.ViewportWidth;

            //open file (when opening associated by double click)
            if (_openingFile != null)
            {
                LoadAndDisplayComic(false);
            }
            else
            {
                _comicBook = new ComicBook(); // KBR use an empty placeholder book
            }

            KeyGrid.ItemsSource = KeyHints; // TODO consider using an InformationWindow and show on the '?' key
        }

        /// <summary>
        /// Exit the applications.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs"></param>
        public void ApplicationExit(object sender, CancelEventArgs eventArgs)
        {
            SaveResumeToConfiguration();
            SaveConfiguration();
        }

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        private void LoadConfiguration()
        {
            //xml config load
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string userFilePath = Path.Combine(localAppData, "C# Comicviewer");

                XmlSerializer mySerializer = new XmlSerializer(typeof(Configuration.Configuration));
                if (System.IO.File.Exists(userFilePath + "\\Configuration.xml"))
                {
                    FileStream myFileStream = new FileStream(userFilePath + "\\Configuration.xml", FileMode.Open);
                    Configuration = (Configuration.Configuration)mySerializer.Deserialize(myFileStream);
                    myFileStream.Close();
                }

                if (Configuration == null)
                {
                    Configuration = new Configuration.Configuration();
                }
            }
            catch (Exception)
            {
                Configuration = new Configuration.Configuration();
            }
        }

        /// <summary>
        /// Saves the configuration.
        /// </summary>
        /// <returns><c>True</c> if success, otherwise returns <c>false</c>.</returns>
        private void SaveConfiguration()
        {
            //xml config save
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string userFilePath = Path.Combine(localAppData, "C# Comicviewer");

                if (!Directory.Exists(userFilePath))
                {
                    Directory.CreateDirectory(userFilePath);
                }

                XmlSerializer mySerializer = new XmlSerializer(typeof(Configuration.Configuration));
                StreamWriter myWriter = new StreamWriter(userFilePath + "\\Configuration.xml");
                mySerializer.Serialize(myWriter, Configuration);
                myWriter.Close();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Saves the resume to configuration.
        /// </summary>
        private void SaveResumeToConfiguration()
        {
            if (_comicBook != null && _comicBook.TotalFiles != 0)
            {
                Bookmark data = _comicBook.GetBookmark();
                Configuration.Resume = data;
            }
        }

        #region Bookmarks
        /// <summary>
        /// Sets the bookmark menus.
        /// </summary>
        private void SetBookmarkMenus()
        {
            Bookmarks_MenuRightClick.Items.Clear();
            Bookmarks_MenuRightClick.Items.Add(AddBookmark_MenuRightClick);
            Bookmarks_MenuRightClick.Items.Add(ManageBookmarks_MenuRightClick);
            Bookmarks_MenuRightClick.Items.Add(new Separator());

            Bookmarks_MenuBar.Items.Clear();
            Bookmarks_MenuBar.Items.Add(AddBookmark_MenuBar);
            Bookmarks_MenuBar.Items.Add(ManageBookmarks_MenuBar);
            Bookmarks_MenuBar.Items.Add(new Separator());

            if (Configuration == null || Configuration.Bookmarks == null || Configuration.Bookmarks.Count <= 0)
            {
                return;
            }
            foreach (Bookmark currentBookmark in Configuration.Bookmarks)
            {
                string[] files = currentBookmark.Files;
                MenuItem bookmark = new MenuItem();
                bookmark.Header = currentBookmark.CurrentFileName;
                bookmark.ToolTip = files[currentBookmark.FileNumber];
                bookmark.Click += LoadBookmark_Click;
                Bookmarks_MenuRightClick.Items.Add(bookmark);

                MenuItem bookmark_bar = new MenuItem();
                bookmark_bar.Header = currentBookmark.CurrentFileName;
                bookmark_bar.ToolTip = files[currentBookmark.FileNumber];
                bookmark_bar.Click += LoadBookmark_Click;
                Bookmarks_MenuBar.Items.Add(bookmark_bar);
            }
        }

        /// <summary>
        /// Handles the Click event of the LoadBookmark control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void LoadBookmark_Click(object sender, EventArgs e)
        {
            //right click menu
            for (int i = 0; i < Bookmarks_MenuRightClick.Items.Count; i++)
            {
                if (sender == Bookmarks_MenuRightClick.Items[i])
                {
                    Bookmark bookmark = Configuration.Bookmarks[i - 3];

                    LoadAndDisplayComic(bookmark.Files, bookmark.FileNumber, bookmark.PageNumber);
                }
            }

            //the bar
            for (int i = 0; i < Bookmarks_MenuBar.Items.Count; i++)
            {
                if (sender == Bookmarks_MenuBar.Items[i])
                {
                    Bookmark bookmark = Configuration.Bookmarks[i - 3];

                    LoadAndDisplayComic(bookmark.Files, bookmark.FileNumber, bookmark.PageNumber);
                }
            }
        }
        #endregion

        #region Keyboard & Mouse
        /// <summary>
        /// Called when [key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.X:
                    Application.Current.Shutdown();
//                    ApplicationExit(null, null);
                    break;
                case Key.R:
                    if (ResumeFile_RightClick.IsEnabled)
                    {
                        Resume_Click(sender, e);
                    }
                    else
                    {
                        ShowMessage("No archive to resume");
                    }
                    break;
                case Key.I:
                    ShowPageInformation();
                    break;
                case Key.L:
                    _lastMouseMove = DateTime.Now;

                    if (_mouseIsHidden)
                    {
                        Mouse.OverrideCursor = Cursors.Arrow;
                        _mouseIsHidden = false;
                    }

                    LoadAndDisplayComic(true);
                    break;
                case Key.M:
                    WindowState = WindowState.Minimized;
                    break;
                case Key.T:
                    ToggleImageOptions();
                    break;
                case Key.N:
                    ShowText();
                    break;
                case Key.W:
                    ToggleWindowMode();
                    break;
                case Key.D:
                    Configuration.DoublePage = !Configuration.DoublePage;
                    ReloadImage();
                    break;
                case Key.PageDown:
                case Key.PageUp:
                    //prevent default action from occurring.
                    e.Handled = true;
                    break;
                case Key.G:
                    GotoPage();
                    break;
            }
        }

        private void ToggleWindowMode()
        {
            if (Configuration.Windowed)
            {
                //go full screen if windowed
                Configuration.Windowed = false;

                SetWindowMode(WindowMode.Fullscreen);
            }
            else
            {
                //go windowed if fullscreen
                Configuration.Windowed = true;
                SetWindowMode(WindowMode.Windowed);
            }

            _scrollValueHorizontal = (int) (ScrollField.ViewportHeight*0.05);
// KBR 20141230 need to eliminate these properties. The "screen H/W" values will change as the user resizes the window. The
// invocation to resize the image can just pass in the current destination dimensions and it works well.            
            _imageUtils.ScreenHeight = (int) ScrollField.ViewportHeight;
            _imageUtils.ScreenWidth = (int) ScrollField.ViewportWidth;

            ReloadImage();
        }

        private void ShowText()
        {
            if (_comicBook.TotalFiles == 0)
            {
                ShowMessage("No archive loaded");
                return;
            }

            if (string.IsNullOrEmpty(_comicBook.CurrentFile.InfoText))
            {
                ShowMessage("No information text");
            }
            else
            {
                var infoText = new InformationText(_comicBook.CurrentFile.Location, _comicBook.CurrentFile.InfoText);
                infoText.ShowDialog();
            }
        }

        private void ReloadImage()
        {
            if (DisplayedImage.Source != null)
            {
                DisplayImage(_comicBook.CurrentFile.CurrentPage, ImageStartPosition.Top);
            }
        }

        private void GotoPage()
        {
            // Allow the user to enter a page # and view that page.
            int maxpage = _comicBook.TotalPages;
            var pageSel = new GotoPageDlg(maxpage);
            pageSel.Page = _comicBook.CurrentPageNumber;
            pageSel.Owner = this;

            Mouse.OverrideCursor = Cursors.Arrow;
            bool? result = pageSel.ShowDialog();
            Mouse.OverrideCursor = Cursors.None;
            if (result != true)
                return; // cancel: no page change

            int pageChoice = pageSel.Page - 1;
            if (pageChoice > -1)
            {
                byte[] image = _comicBook.GetPage(pageChoice); // TODO return only a valid page
                if (image != null && image.Length > 1 && image[0] != 0)   // TODO duplicated code - see Next and Previous Page
                {
                    DisplayImage(image, ImageStartPosition.Top);
                }
            }
        }

        /// <summary>
        /// Called when [preview key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Home && !Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                // first page of all
                if (_comicBook.TotalFiles != 0)
                {
                    byte[] image = _comicBook.GetPage(0, 0);
                    if (image != null)
                    {
                        DisplayImage(image, ImageStartPosition.Top);
                    }
                }
            }

            if (e.SystemKey == Key.Home && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                // first page of current archive
                if (_comicBook.TotalFiles != 0)
                {
                    byte[] image = _comicBook.GetPage(0);
                    if (image != null)
                    {
                        DisplayImage(image, ImageStartPosition.Top);
                    }
                }
            }

            if (e.Key == Key.End && !Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                // last page of all
                if (_comicBook.TotalFiles != 0)
                {
                    byte[] image = _comicBook.GetPage(_comicBook.TotalFiles - 1, _comicBook[_comicBook.TotalFiles - 1].TotalPages - 1);
                    if (image != null)
                    {
                        DisplayImage(image, ImageStartPosition.Top);
                    }
                }
            }

            if (e.SystemKey == Key.End && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                // last page of current archive
                if (_comicBook.TotalFiles != 0)
                {
                    byte[] image = _comicBook.GetPage(_comicBook.CurrentFile.TotalPages - 1);
                    if (image != null)
                    {
                        DisplayImage(image, ImageStartPosition.Top);
                    }
                }
            }

            if (e.Key == Key.PageDown)
            {
                NextPage();

                //prevent default action from occurring.
                e.Handled = true;
            }

            if (e.Key == Key.PageUp)
            {
                PreviousPage(Configuration.DoublePage);

                //prevent default action from occurring.
                e.Handled = true;
            }

            if (e.SystemKey == Key.PageDown && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                NextFile();
            }

            if (e.SystemKey == Key.PageUp && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                PreviousFile();
            }

            if (e.Key == Key.Down)
            {
                OnArrowKey(Key.Down);
            }

            if (e.Key == Key.Up)
            {
                OnArrowKey(Key.Up);
            }

            if (e.Key == Key.Right)
            {
                OnArrowKey(Key.Right);
            }

            if (e.Key == Key.Left)
            {
                OnArrowKey(Key.Left);
            }
        }

        /// <summary>
        /// Called when [arrow key].
        /// </summary>
        /// <param name="e">The e.</param>
        private void OnArrowKey(Key e)
        {
            const int scrollAmount = 50;

            //scroll down
            if (e == Key.Down && DisplayedImage.Source != null)
            {
                ScrollField.ScrollToVerticalOffset(ScrollField.VerticalOffset + scrollAmount);
            }

            //scroll up
            if (e == Key.Up && DisplayedImage.Source != null)
            {
                ScrollField.ScrollToVerticalOffset(ScrollField.VerticalOffset - scrollAmount);
            }

            //scroll right
            if (e == Key.Right && DisplayedImage.Source != null)
            {
                ScrollField.ScrollToHorizontalOffset(ScrollField.HorizontalOffset + scrollAmount);
            }

            //scroll left
            if (e == Key.Left && DisplayedImage.Source != null)
            {
                ScrollField.ScrollToHorizontalOffset(ScrollField.HorizontalOffset - scrollAmount);
            }

            if (ScrollField.VerticalOffset > ScrollField.ScrollableHeight || ScrollField.VerticalOffset < 0)
            {
                ScrollField.ScrollToVerticalOffset(ScrollField.ScrollableHeight);
            }

            if (ScrollField.HorizontalOffset > ScrollField.ScrollableWidth || ScrollField.HorizontalOffset < 0)
            {
                ScrollField.ScrollToHorizontalOffset(ScrollField.ScrollableWidth);
            }
        }

        /// <summary>
        /// Called when mouse wheel scrolls.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseWheelEventArgs"/> instance containing the event data.</param>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //scroll down
            if (e.Delta < 0 && DisplayedImage.Source != null)
            {
                _previousPageBoolean = false;
                _previousPageCount = 2;
                if (DisplayedImage.Width > ScrollField.ViewportWidth)
                {
                    //image widther then screen
                    if (ScrollField.HorizontalOffset == ScrollField.ScrollableWidth && ScrollField.VerticalOffset == ScrollField.ScrollableHeight)
                    {
                        //Can count down for next page
                        _nextPageBoolean = true;
                        _nextPageCount--;
                    }
                    else if (ScrollField.VerticalOffset == ScrollField.ScrollableHeight)
                    {
                        //scroll horizontal
                        ScrollField.ScrollToHorizontalOffset(ScrollField.HorizontalOffset + _scrollValueHorizontal);
                    }
                }
                else if (ScrollField.VerticalOffset == ScrollField.ScrollableHeight)
                {
                    //Can count down for next page
                    _nextPageBoolean = true;
                    _nextPageCount--;
                }
            }
            else if (e.Delta > 0 && DisplayedImage.Source != null)
            {
                //scroll up
                _nextPageBoolean = false;
                _nextPageCount = 2;
                if (DisplayedImage.Width > ScrollField.ViewportWidth)
                {
                    //image widther then screen
                    if (ScrollField.HorizontalOffset == 0)
                    {
                        //Can count down for previous page
                        _previousPageBoolean = true;
                        _previousPageCount--;
                    }
                    else if (ScrollField.VerticalOffset == 0)
                    {
                        //scroll horizontal
                        ScrollField.ScrollToHorizontalOffset(ScrollField.HorizontalOffset - _scrollValueHorizontal);
                    }
                }
                else if (ScrollField.VerticalOffset == 0)
                {
                    //Can count down for previous page
                    _previousPageBoolean = true;
                    _previousPageCount--;
                }
            }

            if (_nextPageBoolean && _nextPageCount <= 0)
            {
                NextPage();
                _nextPageBoolean = false;
                _nextPageCount = 2;
            }
            else if (_previousPageBoolean && _previousPageCount <= 0)
            {
                PreviousPage(Configuration.DoublePage);
                _previousPageBoolean = false;
                _previousPageCount = 2;
            }
        }

        /// <summary>
        /// Called when [preview mouse wheel].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseWheelEventArgs"/> instance containing the event data.</param>
        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            OnMouseWheel(sender, e);
        }

        /// <summary>
        /// Called when [mouse move].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            _lastMouseMove = DateTime.Now;

            if (_mouseIsHidden && (Mouse.GetPosition(this) != _currentMousePosition))
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                _mouseIsHidden = false;
            }

            _currentMousePosition = Mouse.GetPosition(this);

            const int speed = 2; //amount by with mouse_x/y - MousePosition.X/Y is divided, determines drag speed
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //I am mouse dragging.
                if (_mouseDrag == false)
                {
                    //If changed position
                    _mouseX = _currentMousePosition.X;
                    _mouseY = _currentMousePosition.Y;
                    _mouseDrag = true;
                }
                else
                {
                    //Did not change position
                    if (_currentMousePosition.X < _mouseX && DisplayedImage.Source != null)
                    {
                        //Drag left
                        ScrollField.ScrollToHorizontalOffset(ScrollField.HorizontalOffset + (_mouseX - _currentMousePosition.X) / speed);
                        _mouseDrag = false;
                    }
                    else if (_currentMousePosition.X > _mouseX && DisplayedImage.Source != null)
                    {
                        //Drag right
                        ScrollField.ScrollToHorizontalOffset(ScrollField.HorizontalOffset + (_mouseX - _currentMousePosition.X) / speed);
                        _mouseDrag = false;
                    }

                    if (_currentMousePosition.Y < _mouseY && DisplayedImage.Source != null)
                    {
                        //Drag up
                        ScrollField.ScrollToVerticalOffset(ScrollField.VerticalOffset + (_mouseY - _currentMousePosition.Y) / speed);
                        _mouseDrag = false;
                    }
                    else if (_currentMousePosition.Y > _mouseY && DisplayedImage.Source != null)
                    {
                        //Drag down
                        ScrollField.ScrollToVerticalOffset(ScrollField.VerticalOffset + (_mouseY - _currentMousePosition.Y) / speed);
                        _mouseDrag = false;
                    }
                }
            }
            else
            {
                //make it possible to drag on next check
                _mouseDrag = false;
            }
        }

        /// <summary>
        /// Called when [right button down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            _mouseIsHidden = false;
        }

        /// <summary>
        /// Mouses the idle checker.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void MouseIdleChecker(object sender, EventArgs e)
        {
            TimeSpan elaped = DateTime.Now - _lastMouseMove;
            if (elaped >= _timeoutToHide && !_mouseIsHidden && IsActive)
            {
                if (MenuRightClick.IsOpen)
                {
                    _lastMouseMove = DateTime.Now;
                }
                else
                {
                    Mouse.OverrideCursor = Cursors.None;
                    _mouseIsHidden = true;
                }
            }
        }

        #endregion

        #region Menus
        /// <summary>
        /// Handles the Click event of the Resume control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Resume_Click(object sender, RoutedEventArgs e)
        {
            if (Configuration.Resume != null)
            {
                LoadAndDisplayComic(Configuration.Resume.Files, Configuration.Resume.FileNumber, Configuration.Resume.PageNumber);
            }
        }

        /// <summary>
        /// Handles the Click event of the Load control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            LoadAndDisplayComic(true);
        }

        /// <summary>
        /// Handles the Click event of the NextPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            NextPage();
        }

        /// <summary>
        /// Handles the Click event of the PreviousPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            PreviousPage(Configuration.DoublePage);
        }

        /// <summary>
        /// Handles the Click event of the NextFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void NextFile_Click(object sender, RoutedEventArgs e)
        {
            NextFile();
        }

        /// <summary>
        /// Handles the Click event of the PreviousFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void PreviousFile_Click(object sender, RoutedEventArgs e)
        {
            PreviousFile();
        }

        /// <summary>
        /// Handles the Click event of the ShowPageInformation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ShowPageInformation_Click(object sender, RoutedEventArgs e)
        {
            ShowPageInformation();
        }

        /// <summary>
        /// Handles the Click event of the Exit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
//			ApplicationExit(sender, null);
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles the Click event of the AddBookmark control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void AddBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (_comicBook != null)
            {
                Configuration.Bookmarks.Add(_comicBook.GetBookmark());
                SetBookmarkMenus();
                ShowMessage("Bookmark added.");
            }
        }

        /// <summary>
        /// Handles the Click event of the ManageBookmarks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ManageBookmarks_Click(object sender, RoutedEventArgs e)
        {
            BookmarkManager bookmarkManager = new BookmarkManager(this);
            bookmarkManager.ShowDialog();
            SetBookmarkMenus();
        }

        /// <summary>
        /// Handles the Click event of the About control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            new About().ShowDialog();
        }
        #endregion

        #region Messages and Page information
        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void ShowMessage(string message)
        {
            MessageBox.Text = message;
            MessageBox.Visibility = Visibility.Visible;

            if (_showMessageTimer != null)
            {
                _showMessageTimer.Stop();
            }

            _showMessageTimer = new DispatcherTimer();
            _showMessageTimer.Tick += HideMessage;
            _showMessageTimer.Interval = new TimeSpan(0, 0, 2);
            _showMessageTimer.Start();
        }

        /// <summary>
        /// Hides the message.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HideMessage(object sender, EventArgs e)
        {
            MessageBox.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Shows the page information.
        /// </summary>
        public void ShowPageInformation()
        {
            if (_comicBook != null)
            {
                if (_fileLoader.PageType == PageType.Archive)
                {
                    PageInfoBox.Text = "Archive " + _comicBook.CurrentFileNumber + "/" + _comicBook.TotalFiles + "\r\nArchive name: " + _comicBook.CurrentFile.FileName + "\r\nPage: " + _comicBook.CurrentPageNumber + "/" + _comicBook.TotalPages;
                }
                else
                {
                    PageInfoBox.Text = "File name: " + _comicBook.CurrentFile.FileName + "\r\nPage: " + _comicBook.CurrentPageNumber + "/" + _comicBook.TotalPages;
                }

                PageInfoBox.Visibility = Visibility.Visible;

                if (_pageInformationTimer != null)
                {
                    _pageInformationTimer.Stop();
                }

                _pageInformationTimer = new DispatcherTimer();
                _pageInformationTimer.Tick += HidePageInformation;
                _pageInformationTimer.Interval = new TimeSpan(0, 0, 5);
                _pageInformationTimer.Start();
            }
        }

        /// <summary>
        /// Hides the page information.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HidePageInformation(object sender, EventArgs e)
        {
            PageInfoBox.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Updates the page information.
        /// </summary>
        private void UpdatePageInformation()
        {
            PageInfoBox.Text = "Archive" + _comicBook.CurrentFileNumber + "/" + _comicBook.TotalFiles + "\r\nPage: " + _comicBook.CurrentPageNumber + "/" + _comicBook.TotalPages;
        }
        #endregion

        #region Load and Display

        /// <summary>
        /// Displays the image.
        /// </summary>
        /// <param name="imageAsBytes">The image as bytes.</param>
        /// <param name="scrollTo">The scroll to.</param>
        public void DisplayImage(byte[] imageAsBytes, ImageStartPosition scrollTo)
        {
            if (Configuration.DoublePage)
            {
                var image1 = _comicBook.CurrentFile.CurrentPage;
                var image2 = _comicBook.CurrentFile.NextPage();

                _imageUtils.DrawDouble(image1, image2,
                                       new System.Drawing.Size((int)ScrollField.ViewportWidth, (int)ScrollField.ViewportHeight),
                                       Configuration.OverrideHeight, Configuration.OverrideWidth);
                imageAsBytes = _imageUtils.ObjectValueAsBytes;
            }

            // If page information is displayed update it with new information
            if (PageInfoBox.Visibility == Visibility.Visible)
            {
                UpdatePageInformation();
            }

            switch (scrollTo.ToString())
            {
                case "Top":
                    {
                        ScrollField.ScrollToTop();
                        ScrollField.ScrollToLeftEnd();
                        break;
                    }

                case "Bottom":
                    {
                        ScrollField.ScrollToBottom();
                        ScrollField.ScrollToRightEnd();
                        break;
                    }

                default:
                    {
                        break;
                    }
            }

            BitmapImage bitmapimage = GetImage(imageAsBytes);

            _imageUtils.ObjectValue = imageAsBytes;

            // KBR TODO lets try skipping this altogether - maybe reduce memory footprint
            //try
            //{
            //    // This can fail when viewing an archive, and a large image.
            //    Background = _imageUtils.BackgroundColor;
            //}
            //catch
            //{
            //}

            try
            {
                // This can fail when viewing an archive, and a large image.
                if (Configuration.OverrideHeight || Configuration.OverrideWidth)
                {
                    // KBR 20141230 passing in the bitmap width for the destination size is wrong and relies on "screenwidth" which isn't maintained properly. See comment in ToggleWindowMode()
                    //_imageUtils.ResizeImage(new System.Drawing.Size(bitmapimage.PixelWidth, (int)ScrollField.ViewportHeight), Configuration.OverrideHeight, Configuration.OverrideWidth);
                    _imageUtils.ResizeImage(new System.Drawing.Size((int)ScrollField.ViewportWidth, (int)ScrollField.ViewportHeight), Configuration.OverrideHeight, Configuration.OverrideWidth);
                    bitmapimage = GetImage(_imageUtils.ObjectValueAsBytes);
                }
            }
            catch
            {
            }

            DisplayedImage.Source = bitmapimage;
            DisplayedImage.Width = bitmapimage.PixelWidth;
            DisplayedImage.Height = bitmapimage.PixelHeight;

            if (DisplayedImage.Width < ScrollField.ViewportWidth)
            {
                DisplayedImage.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else if (DisplayedImage.Width >= ScrollField.ViewportWidth)
            {
                DisplayedImage.HorizontalAlignment = HorizontalAlignment.Left;
            }

            if (DisplayedImage.Height < ScrollField.ViewportHeight)
            {
                DisplayedImage.VerticalAlignment = VerticalAlignment.Center;
            }
            else if (DisplayedImage.Height >= ScrollField.ViewportHeight)
            {
                DisplayedImage.VerticalAlignment = VerticalAlignment.Top;
            }

            ShowPageInformation();

            GC.Collect();
        }

        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <param name="imageAsByteArray">The image as byte array.</param>
        /// <returns>
        /// Returns an image.
        /// </returns>
        private BitmapImage GetImage(byte[] imageAsByteArray)
        {
            try
            {
                using (var ms = new MemoryStream(imageAsByteArray))
                {
                    ms.Position = 0;
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = ms;
                    bi.EndInit();
                    bi.Freeze();
                    return bi;
                }
            }
            catch
            {
                try
                {
                    //If it fails the normal way try it again with a convert, possible quality loss.
                    System.Drawing.ImageConverter ic = new System.Drawing.ImageConverter();
                    System.Drawing.Image img = (System.Drawing.Image)ic.ConvertFrom(imageAsByteArray);
                    if (img != null)
                    {
                        System.Drawing.Bitmap bitmap1 = new System.Drawing.Bitmap(img);
                        MemoryStream ms = new MemoryStream();
                        bitmap1.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        ms.Position = 0;
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.StreamSource = ms;
                        bi.EndInit();
                        return bi;
                    }
                }
                catch
                {
                    ShowMessage("Could not load image.");
                }
            }
            return null;
        }

        // If the target folder doesn't exist, go up to the parent repeatedly until a folder doesn't exist
        private string FirstValidFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                var parent = Directory.GetParent(folderPath);
                if (parent == null) // reached the root
                    return folderPath;
                return FirstValidFolder(parent.FullName);
            }
            return folderPath;
        }

        /// <summary>
        /// Load archive(s) and display first page
        /// </summary>
        /// <param name="askOpenFileDialog">Should file dialog be used?</param>
        public void LoadAndDisplayComic(bool askOpenFileDialog)
        {
            string[] files = { _openingFile };

            if (askOpenFileDialog)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (_comicBook != null)
                {
                    Bookmark bookmark = _comicBook.GetBookmark();
                    if (bookmark != null && !string.IsNullOrWhiteSpace(bookmark.GetCurrentFileDirectoryLocation())) // TODO fails when opening image files (not archives)
                        openFileDialog.InitialDirectory = FirstValidFolder(bookmark.GetCurrentFileDirectoryLocation());
                }

                openFileDialog.Filter = Utils.FileLoaderFilter;
                openFileDialog.Multiselect = true;
                openFileDialog.ShowDialog();

                if (openFileDialog.FileNames.Length <= 0)
                {
                    return;
                }

                files = openFileDialog.FileNames;
            }

            LoadAndDisplayComic(files);
        }

        /// <summary>
        /// Load archive(s) and display a page of choice
        /// </summary>
        /// <param name="files">Array with archive locations</param>
        /// <param name="fileIndex">File in array to start at</param>
        /// <param name="pageIndex">Page on which to start from selected file</param>
        public void LoadAndDisplayComic(string[] files, int fileIndex=0, int pageIndex=0)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                foreach (string file in files)
                {
                    if (!System.IO.File.Exists(file))
                    {
                        ShowMessage("One or more archives not found");
                        return;
                    }
                }

                LoadFile(files, fileIndex, pageIndex);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Load the archives
        /// </summary>
        /// <param name="files">Archive location</param>
        /// <param name="fileNumber">File in array to start at</param>
        /// <param name="pageNumber">Page on which to start from selected file</param>
        public void LoadFile(string[] files, int fileNumber, int pageNumber)
        {
            KeyGrid.Visibility = Visibility.Collapsed;

            _fileLoader.Load(files);

            if (_fileLoader.LoadedFileData.HasFile)
            {
                _comicBook = _fileLoader.LoadedFileData.ComicBook;
                if (_comicBook.TotalPages < 1) // Found a ZIP containing ZIPs
                {
                    ShowMessage("Unable to load any images");
                    return;
                }

                DisplayImage(_comicBook.GetPage(fileNumber, pageNumber), ImageStartPosition.Top);

                foreach (ComicFile comicFile in _comicBook)
                {
                    if (!string.IsNullOrEmpty(comicFile.InfoText))
                    {
                        Mouse.OverrideCursor = Cursors.Arrow;
                        var infoText = new InformationText(comicFile.Location, comicFile.InfoText);
                        infoText.ShowDialog();
                        Mouse.OverrideCursor = Cursors.Wait;
                    }
                }

                if (!string.IsNullOrEmpty(_fileLoader.Error))
                {
                    ShowMessage(_fileLoader.Error);
                }
            }
            else if (!string.IsNullOrEmpty(_fileLoader.Error))
            {
                ShowMessage(_fileLoader.Error);
            }
            else
            {
                ShowMessage("No supported files found.");
            }
        }

        /// <summary>
        /// Toggle the images options (fit to screen etc.)
        /// </summary>
        public void ToggleImageOptions()
        {
            if (!Configuration.OverrideHeight && !Configuration.OverrideWidth)
            {
                //normal to height
                Configuration.OverrideHeight = true;

                DisplayedImage.Height = 40;
                ShowMessage("Fit to height.");
            }
            else if (Configuration.OverrideHeight && !Configuration.OverrideWidth)
            {
                //height to width
                Configuration.OverrideHeight = false;
                Configuration.OverrideWidth = true;
                ShowMessage("Fit to width.");
            }
            else if (!Configuration.OverrideHeight && Configuration.OverrideWidth)
            {
                //width to screen
                Configuration.OverrideHeight = true;
                //Configuration.OverrideWidth = true;
                ShowMessage("Fit to screen.");
            }
            else if (Configuration.OverrideHeight && Configuration.OverrideWidth)
            {
                //screen to normal
                Configuration.OverrideHeight = false;
                Configuration.OverrideWidth = false;
                ShowMessage("Normal mode.");
            }

            if (DisplayedImage.Source != null)
            {
                DisplayImage(_comicBook.CurrentFile.CurrentPage, ImageStartPosition.Top);
            }
        }

        /// <summary>
        /// Loads the first file found after the current one and displays the first image if possible.
        /// </summary>
        private void NextFile()
        {
            if (_comicBook == null) return;
            if (_comicBook.NextFile() == null)
            {
                var fileNextPrevious = new FileNextPrevious();
                string file = fileNextPrevious.GetNextFileInDirectory(_comicBook.CurrentFile.Location);
                if (!string.IsNullOrEmpty(file))
                {
                    LoadAndDisplayComic(new[] { file });
                }
            }
            else
            {
                byte[] image = _comicBook.CurrentFile.CurrentPage;
                if (image != null)
                {
                    DisplayImage(image, ImageStartPosition.Top);
                }
            }
        }

        /// <summary>
        /// Show the previous file.
        /// </summary>
        private void PreviousFile()
        {
            if (_comicBook == null) { return; }
            if (_comicBook.PreviousFile() == null)
            {
                var fileNextPrevious = new FileNextPrevious();
                string file = fileNextPrevious.GetPreviousFileInDirectory(_comicBook.CurrentFile.Location);
                if (!string.IsNullOrEmpty(file))
                {
                    LoadAndDisplayComic(new[] { file });
                }
            }
            else
            {
                byte[] image = _comicBook.CurrentFile.CurrentPage;
                if (image != null)
                {
                    DisplayImage(image, ImageStartPosition.Bottom);
                }
            }
        }

        /// <summary>
        /// Go to next page
        /// </summary>
        public void NextPage()
        {
            if (_comicBook == null || _comicBook.TotalFiles == 0)
                return;

            byte[] image = _comicBook.NextPage();
            if (image != null)
            {
                DisplayImage(image, ImageStartPosition.Top);
            }
        }

        /// <summary>
        /// Go to previous page
        /// </summary>
        /// <param name="doublePage"> </param>
        public void PreviousPage(bool doublePage)
        {
            if (_comicBook == null || _comicBook.TotalFiles == 0) 
                return;

            byte[] image = _comicBook.PreviousPage(doublePage);
            if (image != null)
            {
                DisplayImage(image, ImageStartPosition.Bottom);
            }
        }

        /// <summary>
        /// Sets the window mode.
        /// </summary>
        /// <param name="windowMode">The window mode.</param>
        public void SetWindowMode(WindowMode windowMode)
        {
            //set window mode
            if (windowMode == WindowMode.Windowed)
            {
                //go hidden first to fix size bug
                MenuBar.Visibility = Visibility.Visible;
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;

                //Add small delay to prevent bug where window size isn't set correctly.
                System.Threading.Thread.Sleep(100);

                WindowState = WindowState.Maximized;
            }
            else
            {
                MenuBar.Visibility = Visibility.Collapsed;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;

                //go minimized first to hide taskbar
                WindowState = WindowState.Minimized;

                //Add small delay to prevent bug where window size isn't set correctly.
                System.Threading.Thread.Sleep(100);

                WindowState = WindowState.Maximized;
                Focus();
            }
        }
        #endregion

        #region Key Functions display support
        /// <summary>
        /// A holder class for a single key-function pair
        /// </summary>
        public class KeyHint
        {
            /// <summary>
            /// The string to display as the 'key'
            /// </summary>
            public string Key { get; set; }
            /// <summary>
            /// The string to display as the description of the 'key'
            /// </summary>
            public string Function { get; set; }
        }

        private List<KeyHint> _keyHints;
        /// <summary>
        /// A list of key-description pairs for display in a datagrid as initial "help"
        /// </summary>
        public List<KeyHint> KeyHints
        {
            get
            {
                if (_keyHints == null)
                    GenerateKeyHints();
                return _keyHints;
            }
        }

        private void GenerateKeyHints()
        {
            _keyHints = new List<KeyHint>();
            var keys = new[] { "R", "L", "T", "D", "I", "N", "W", "M", "X", "G", " ",
                               "Arrow keys", "Page Down", "Page Up", "Alt + Page Down", "Alt + Page Up",
                               "Alt + Home", "Home", "Alt + End", "End"
                             };
            var funcs = new[] {
                                "Resume last file",
                                "Load file(s)",
                                "Toggle image display mode (fit vs stretch)",
                                "Show double pages (ignores display mode)",
                                "Show page count",
                                "Show .txt in current file if available",
                                "Toggle between windowed and fullscreen mode",
                                "Minimize",
                                "Exit",
                                "Goto Page",
                                " ",
                                "Move over page",
                                "Next page",
                                "Previous page",
                                "Next file",
                                "Previous file",
                                "First page of current file",
                                "First page of all files",
                                "Last page of current file",
                                "Last page of all files"
                                 };
            for (int i = 0; i < keys.Length; i++)
                _keyHints.Add(new KeyHint { Key = keys[i], Function = funcs[i]});
        }
        #endregion

        #region Nav Regions
        private void LeftNav_MouseEnter(object sender, MouseEventArgs e)
        {
            leftNav.Opacity = 0.75;
        }

        private void LeftNav_MouseLeave(object sender, MouseEventArgs e)
        {
            leftNav.Opacity = 0.1;
        }

        private void LeftNav_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PreviousPage(Configuration.DoublePage);
        }

        private void RightNav_MouseEnter(object sender, MouseEventArgs e)
        {
            rightNav.Opacity = 0.75;
        }

        private void RightNav_MouseLeave(object sender, MouseEventArgs e)
        {
            rightNav.Opacity = 0.1;
        }

        private void RightNav_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NextPage();
        }
        #endregion

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
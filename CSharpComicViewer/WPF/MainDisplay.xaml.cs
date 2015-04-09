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
using Microsoft.Win32;

namespace CSharpComicViewer.WPF
{
	/// <summary>
	/// Interaction logic for Window2.xaml
	/// </summary>
	public partial class MainDisplay : Window
	{
		#region Properties

		/// <summary>
		/// Gets or sets the opening file.
		/// </summary>
		/// <value>
		/// The opening file.
		/// </value>
		private string _openingFile;

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
		private int _previousPageCount;

		/// <summary>
		/// The comic page (ea the displayed image)
		/// </summary>
		private ImageUtils _imageUtils;

		/// <summary>
		/// The information Text
		/// </summary>
		private InformationText _informationText;

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
		/// Gets or sets the scroll value vertical.
		/// </summary>
		/// <value>
		/// The scroll value vertical.
		/// </value>
		private int _scrollValueVertical;

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
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="MainDisplay"/> class.
		/// </summary>
		public MainDisplay()
		{
			_nextPageCount = 2;
			_previousPageCount = 2;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MainDisplay"/> class.
		/// </summary>
		/// <param name="openingFile">The opening file.</param>
		public MainDisplay(string openingFile)
			: this()
		{
			InitializeComponent();
		    DataContext = this;
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
			this.Activate();

			_imageUtils = new ImageUtils();
			_fileLoader = new FileLoader();

			//set mouse idle timer
			_timeoutToHide = TimeSpan.FromSeconds(2);
			MouseIdle = new DispatcherTimer();
			MouseIdle.Interval = TimeSpan.FromSeconds(1);
			MouseIdle.Tick += new EventHandler(MouseIdleChecker);
			MouseIdle.Start();

			//Load config
			LoadConfiguration();
			SetBookmarkMenus();

			//set window mode
			if (Configuration.Windowed)
			{
				//go hidden first to fix size bug
				MenuBar.Visibility = Visibility.Hidden;
				this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
				this.WindowState = System.Windows.WindowState.Maximized;
				this.ResizeMode = System.Windows.ResizeMode.CanResize;
				MenuBar.Visibility = Visibility.Visible;
			}
			else
			{
				//if fullscreen
				this.WindowStyle = System.Windows.WindowStyle.None;
				this.WindowState = System.Windows.WindowState.Maximized;
				this.ResizeMode = System.Windows.ResizeMode.NoResize;
			}

			//gray out resume last file if the files dont't exist
			if (Configuration.Resume != null)
			{
				foreach (string file in Configuration.Resume.Files)
				{
					if (!File.Exists(file))
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
			_scrollValueVertical = (int)(ScrollField.ViewportWidth * 0.05);
			_imageUtils.ScreenHeight = (int)ScrollField.ViewportHeight;
			_imageUtils.ScreenWidth = (int)ScrollField.ViewportWidth;

			//open file (when opening associated by double click)
			if (_openingFile != null)
			{
				LoadAndDisplayComic(false);
			}

		    KeyGrid.ItemsSource = KeyHints;
		}

	    /// <summary>
	    /// Exit the applications.
	    /// </summary>
	    /// <param name="sender">The sender.</param>
	    /// <param name="cancelEventArgs"> </param>
	    private void ApplicationExit(object sender, CancelEventArgs cancelEventArgs)
		{
			SaveResumeToConfiguration();
			SaveConfiguration();
			Application.Current.Shutdown();
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
				if (File.Exists(userFilePath + "\\Configuration.xml"))
				{
					System.IO.FileStream myFileStream = new System.IO.FileStream(userFilePath + "\\Configuration.xml", System.IO.FileMode.Open);
					Configuration = (Configuration.Configuration)mySerializer.Deserialize(myFileStream);
					myFileStream.Close();
				}

				if (Configuration == null)
				{
					Configuration = new Configuration.Configuration();
				}
			}
			catch (Exception ex)
			{
				Configuration = new Configuration.Configuration();
			}
		}

		/// <summary>
		/// Saves the configuration.
		/// </summary>
		/// <returns><c>True</c> if succes, otherwise returns <c>false</c>.</returns>
		private bool SaveConfiguration()
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
				System.IO.StreamWriter myWriter = new System.IO.StreamWriter(userFilePath + "\\Configuration.xml");
				mySerializer.Serialize(myWriter, Configuration);
				myWriter.Close();
				return true;
			}
			catch (Exception ex)
			{
				return false;
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

			if (Configuration != null)
			{
				if (Configuration.Bookmarks != null)
				{
					if (Configuration.Bookmarks.Count > 0)
					{
						foreach (Bookmark currentBookmark in Configuration.Bookmarks)
						{
							string[] files = currentBookmark.Files;
							MenuItem bookmark = new MenuItem();
							bookmark.Header = currentBookmark.CurrentFileName;
							bookmark.ToolTip = files[currentBookmark.FileNumber];
							bookmark.Click += new RoutedEventHandler(LoadBookmark_Click);
							Bookmarks_MenuRightClick.Items.Add(bookmark);

							MenuItem bookmark_bar = new MenuItem();
							bookmark_bar.Header = currentBookmark.CurrentFileName;
							bookmark_bar.ToolTip = files[currentBookmark.FileNumber];
							bookmark_bar.Click += new RoutedEventHandler(LoadBookmark_Click);
							Bookmarks_MenuBar.Items.Add(bookmark_bar);
						}
					}
				}
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
			ArrayList data = new ArrayList();
			for (int i = 0; i < Bookmarks_MenuRightClick.Items.Count; i++)
			{
				if ((MenuItem)sender == Bookmarks_MenuRightClick.Items[i])
				{
					Bookmark bookmark = Configuration.Bookmarks[i - 3];

					LoadAndDisplayComic(bookmark.Files, bookmark.FileNumber, bookmark.PageNumber);
				}
			}

			//the bar
			for (int i = 0; i < Bookmarks_MenuBar.Items.Count; i++)
			{
				if ((MenuItem)sender == Bookmarks_MenuBar.Items[i])
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
                    ApplicationExit(null, null);
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
                    if (_comicBook != null && _comicBook.TotalFiles != 0)
                    {
                        if (string.IsNullOrEmpty(_comicBook.CurrentFile.InfoText))
                        {
                            ShowMessage("No information text");
                        }
                        else
                        {
                            _informationText = new InformationText(_comicBook.CurrentFile.Location, _comicBook.CurrentFile.InfoText);
                            _informationText.ShowDialog();
                        }
                    }
                    else
                    {
                        ShowMessage("No archive loaded");
                    }
		            break;
                case Key.W:
                    if (Configuration.Windowed)
                    {
                        //go full screen if windowed
                        Configuration.Windowed = false;

                        WindowStyle = WindowStyle.None;

                        //go minimized first to hide taskbar
                        WindowState = WindowState.Minimized;
                        WindowState = WindowState.Maximized;
                        ResizeMode = ResizeMode.NoResize;
                        MenuBar.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        //go windowed if fullscreen
                        //go hidden first to fix size bug
                        MenuBar.Visibility = Visibility.Hidden;
                        WindowStyle = WindowStyle.SingleBorderWindow;
                        WindowState = WindowState.Maximized;
                        ResizeMode = ResizeMode.CanResize;
                        MenuBar.Visibility = Visibility.Visible;
                        Configuration.Windowed = true;
                    }
				    _scrollValueHorizontal = (int)(ScrollField.ViewportHeight * 0.05);
				    _scrollValueVertical = (int)(ScrollField.ViewportWidth * 0.05);
				    _imageUtils.ScreenHeight = (int)ScrollField.ViewportHeight;
				    _imageUtils.ScreenWidth = (int)ScrollField.ViewportWidth;

				    if (DisplayedImage.Source != null)
				    {
					    DisplayImage(_comicBook.CurrentFile.CurrentPage, ImageStartPosition.Top);
				    }
		            break;
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

			if (e.Key == Key.Home && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
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

			if (e.Key == Key.End && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
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
			}

			if (e.Key == Key.PageUp)
			{
				PreviousPage();
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
			int scrollAmmount = 50;

			//scroll down
			if (e == Key.Down && DisplayedImage.Source != null)
			{
				ScrollField.ScrollToVerticalOffset(ScrollField.VerticalOffset + scrollAmmount);
			}

			//scroll up
			if (e == Key.Up && DisplayedImage.Source != null)
			{
				ScrollField.ScrollToVerticalOffset(ScrollField.VerticalOffset - scrollAmmount);
			}

			//scroll right
			if (e == Key.Right && DisplayedImage.Source != null)
			{
				ScrollField.ScrollToHorizontalOffset(ScrollField.HorizontalOffset + scrollAmmount);
			}

			//scroll left
			if (e == Key.Left && DisplayedImage.Source != null)
			{
				ScrollField.ScrollToHorizontalOffset(ScrollField.HorizontalOffset - scrollAmmount);
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
				PreviousPage();
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

			int speed = 2; //amount by with mouse_x/y - MousePosition.X/Y is divided, determines drag speed
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
			if (elaped >= _timeoutToHide && !_mouseIsHidden)
			{
				if (this.IsActive && !MenuRightClick.IsOpen)
				{
					Mouse.OverrideCursor = Cursors.None;
					_mouseIsHidden = true;
				}
				else if (this.IsActive && MenuRightClick.IsOpen)
				{
					_lastMouseMove = DateTime.Now;
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
			PreviousPage();
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
			ApplicationExit(sender, null);
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
			About about = new About();
			about.ShowDialog();
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
			MessageBox.Visibility = System.Windows.Visibility.Visible;

			if (_showMessageTimer != null)
			{
				_showMessageTimer.Stop();
			}

			_showMessageTimer = new DispatcherTimer();
			_showMessageTimer.Tick += new EventHandler(HideMessage);
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
			MessageBox.Visibility = System.Windows.Visibility.Hidden;
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

				PageInfoBox.Visibility = System.Windows.Visibility.Visible;

				if (_pageInformationTimer != null)
				{
					_pageInformationTimer.Stop();
				}

				_pageInformationTimer = new DispatcherTimer();
				_pageInformationTimer.Tick += new EventHandler(HidePageInformation);
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
			PageInfoBox.Visibility = System.Windows.Visibility.Hidden;
		}

		/// <summary>
		/// Updates the page information.
		/// </summary>
		private void UpdatePageInformation()
		{
			PageInfoBox.Text = "Archive" + _comicBook.CurrentFileNumber + "/" + _comicBook.TotalFiles + "\r\nPage: " + _comicBook.CurrentPageNumber + "/" + _comicBook.TotalPages;
		}
		#endregion

		#region Load an Display
		/// <summary>
		/// Displays the image.
		/// </summary>
		/// <param name="imageAsBytes">The image as bytes.</param>
		/// <param name="scrollTo">The scroll to.</param>
		public void DisplayImage(byte[] imageAsBytes, ImageStartPosition scrollTo)
		{
			// If page information is displayed update it with new information
			if (PageInfoBox.Visibility == System.Windows.Visibility.Visible)
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

			if (Configuration.OverideHeight || Configuration.OverideWidth)
			{
				_imageUtils.ResizeImage(new System.Drawing.Size(bitmapimage.PixelWidth, (int)ScrollField.ViewportHeight), Configuration.OverideHeight, Configuration.OverideWidth);
				bitmapimage = GetImage(_imageUtils.ObjectValueAsBytes);
			}

			DisplayedImage.Source = bitmapimage;

			DisplayedImage.Width = bitmapimage.PixelWidth;
			DisplayedImage.Height = bitmapimage.PixelHeight;
			this.Background = _imageUtils.BackgroundColor;

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
		}

		/// <summary>
		/// Gets the image.
		/// </summary>
		/// <param name="imageAsByteArray">The image as byte array.</param>
		/// <returns>Returns an image.</returns>
		private BitmapImage GetImage(byte[] imageAsByteArray)
		{
			BitmapImage bi = new BitmapImage();

			try
			{
				bi.CacheOption = BitmapCacheOption.OnLoad;
				MemoryStream ms = new MemoryStream(imageAsByteArray);
				ms.Position = 0;
				bi.BeginInit();
				bi.StreamSource = ms;
				bi.EndInit();
			}
			catch
			{
				try
				{
					//If it fails the normal way try it again with a convert, possible quality loss.
					System.Drawing.ImageConverter ic = new System.Drawing.ImageConverter();
					System.Drawing.Image img = (System.Drawing.Image)ic.ConvertFrom(imageAsByteArray);
					System.Drawing.Bitmap bitmap1 = new System.Drawing.Bitmap(img);
					MemoryStream ms = new MemoryStream();
					bitmap1.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
					ms.Position = 0;
					bi = new BitmapImage();
					bi.CacheOption = BitmapCacheOption.OnLoad;
					bi.BeginInit();
					bi.StreamSource = ms;
					bi.EndInit();
				}
				catch
				{
					ShowMessage("Could not load image.");
				}
			}

			return bi;
		}

		/// <summary>
		/// Load archive(s) and display first page
		/// </summary>
		/// <param name="askOpenFileDialog">Should file dialog be used?</param>
		public void LoadAndDisplayComic(bool askOpenFileDialog)
		{
			try
			{
				string[] files;

				if (askOpenFileDialog)
				{
					OpenFileDialog openFileDialog = new OpenFileDialog();
					if (_comicBook != null)
					{
						Bookmark bookmark = _comicBook.GetBookmark();
						openFileDialog.InitialDirectory = bookmark.GetCurrentFileDirectoryLocation();
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
				else
				{
					files = new string[] { _openingFile };
				}

                Mouse.OverrideCursor = Cursors.Wait;
                foreach (string file in files)
				{
					if (!File.Exists(file))
					{
						ShowMessage("One or more archives not found");
					    return;
					}
				}

				LoadFile(files, 0, 0);
			}
			finally
			{
                Mouse.OverrideCursor = Cursors.Arrow;
            }

		}

		/// <summary>
		/// Load archive(s) and display first page
		/// </summary>
		/// <param name="files">Array with archive locations</param>
		public void LoadAndDisplayComic(string[] files)
		{
			try
			{
                Mouse.OverrideCursor = Cursors.Wait;
                foreach (string file in files)
				{
					if (!File.Exists(file))
					{
						ShowMessage("One or more archives not found");
					    return;
					}
				}

				LoadFile(files, 0, 0);
			}
			finally
			{
                Mouse.OverrideCursor = Cursors.Arrow;
            }
		}

		/// <summary>
		/// Load archive(s) and display a page of choice
		/// </summary>
		/// <param name="files">Array with archive locations</param>
		/// <param name="fileNumber">File in array to start at</param>
		/// <param name="pageNumber">Page on which to start from selected file</param>
		public void LoadAndDisplayComic(string[] files, int fileNumber, int pageNumber)
		{
			try
			{
                Mouse.OverrideCursor = Cursors.Wait;
                foreach (string file in files)
				{
					if (!File.Exists(file))
					{
						ShowMessage("One or more archives not found");
					    return;
					}
				}

				LoadFile(files, fileNumber, pageNumber);
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

				foreach (ComicFile comicFile in _comicBook)
				{
					if (!string.IsNullOrEmpty(comicFile.InfoText))
					{
						Mouse.OverrideCursor = Cursors.Arrow;
						_informationText = new InformationText(comicFile.Location, comicFile.InfoText);
						_informationText.ShowDialog();
						Mouse.OverrideCursor = Cursors.Wait;
					}
				}

				DisplayImage(_comicBook.GetPage(fileNumber, pageNumber), ImageStartPosition.Top);
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
			if (!Configuration.OverideHeight && !Configuration.OverideWidth)
			{
				//normal to hight
				Configuration.OverideHeight = true;

				DisplayedImage.Height = 40;
				ShowMessage("Fit to height.");
			}
			else if (Configuration.OverideHeight && !Configuration.OverideWidth)
			{
				//hight to width
				Configuration.OverideHeight = false;
				Configuration.OverideWidth = true;
				ShowMessage("Fit to width.");
			}
			else if (!Configuration.OverideHeight && Configuration.OverideWidth)
			{
				//width to screen
				Configuration.OverideHeight = true;
				Configuration.OverideWidth = true;
				ShowMessage("Fit to screen.");
			}
			else if (Configuration.OverideHeight && Configuration.OverideWidth)
			{
				//screen to normal
				Configuration.OverideHeight = false;
				Configuration.OverideWidth = false;
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
			if (_comicBook != null)
			{
				if (_comicBook.NextFile() == null)
				{
					FileNextPrevious fileNextPrevious = new FileNextPrevious();
					string file = fileNextPrevious.GetNextFileInDirectory(_comicBook.CurrentFile.Location);
					if (!string.IsNullOrEmpty(file))
					{
						LoadAndDisplayComic(new string[] { file });
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
		}

		/// <summary>
		/// Previouses the file.
		/// </summary>
		private void PreviousFile()
		{
			if (_comicBook != null)
			{
				if (_comicBook.PreviousFile() == null)
				{
					FileNextPrevious fileNextPrevious = new FileNextPrevious();
					string file = fileNextPrevious.GetPreviousFileInDirectory(_comicBook.CurrentFile.Location);
					if (!string.IsNullOrEmpty(file))
					{
						LoadAndDisplayComic(new string[] { file });
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
		}

		/// <summary>
		/// Go to next page
		/// </summary>
		public void NextPage()
		{
			if (_comicBook != null)
			{
				if (_comicBook.TotalFiles != 0)
				{
					byte[] image = _comicBook.NextPage();
					if (image != null)
					{
						DisplayImage(image, ImageStartPosition.Top);
					}
				}
			}
		}

		/// <summary>
		/// Go to previous page
		/// </summary>
		public void PreviousPage()
		{
			if (_comicBook != null)
			{
				if (_comicBook.TotalFiles != 0)
				{
					byte[] image = _comicBook.PreviousPage();
					if (image != null)
					{
						DisplayImage(image, ImageStartPosition.Bottom);
					}
				}
			}
		}

		#endregion

        #region Key Functions display support
        public class KeyHint
        {
            public string Key { get; set; }
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
                    generateKeyHints();
	            return _keyHints;
	        }
	    }

        private void generateKeyHints()
        {
            _keyHints = new List<KeyHint>();
            string[] keys = new string[]
                                {
                                    "R", "L", "T", "I", "N", "W", "M", "X", " ",
                                    "Arrow keys", "Page Down", "Page Up", "Alt + Page Down", "Alt + Page Up",
                                    "Home", "Alt + Home", "End", "Alt + End"
                                };
            string[] funcs = new string[]
                                 {
                                     "Resume last file",
                                     "Load file(s)",
                                     "Toggle image display mode (fit vs stretch)",
                                     "Show page count",
                                     "Show .txt in current file if available",
                                     "Toggle between windowed and fullscreen mode",
                                     "Minimize",
                                     "Exit",
                                     " ",
                                     "Move over page",
                                     "Next page",
                                     "Previous page",
                                     "Next file",
                                     "Previous file",
                                     "First page of current file",
                                     "First page of all files",
                                     "Last page of current file",
                                     "Last page of all files",
                                 };
            for (int i = 0; i < keys.Length; i++)
                _keyHints.Add(new KeyHint() { Key = keys[i], Function = funcs[i]});
        }
        #endregion
    }
}
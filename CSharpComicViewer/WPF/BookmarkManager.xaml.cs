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
using System.Windows;
using System.Windows.Controls;
using CSharpComicLoader;

namespace CSharpComicViewer.WPF
{
	/// <summary>
	/// Interaction logic for BookmarkManager.xaml
	/// </summary>
	public partial class BookmarkManager : Window
	{
		/// <summary>
		/// The bookmarks currently stored
		/// </summary>
		private List<Bookmark> bookmarks;

		/// <summary>
		///  Initializes a new instance of the BookmarkManager class.
		/// </summary>
		/// <param name="parent">The parent of this window.</param>
		public BookmarkManager(MainDisplay parent)
		{
			InitializeComponent();
			this.DataGridBookmarks.DataContext = parent.Configuration.Bookmarks;

			this.bookmarks = parent.Configuration.Bookmarks;
		}

		/// <summary>
		/// Find visual parent.
		/// </summary>
		/// <typeparam name="T">Object that is returned.</typeparam>
		/// <param name="element">Element of wich the visual parent is requested.</param>
		/// <returns>Object wich is the visual parent.</returns>
		/// <remarks>Needed for one click editing.</remarks>
		private static T FindVisualParent<T>(UIElement element) where T : UIElement
		{
			UIElement parent = element;
			while (parent != null)
			{
				T correctlyTyped = parent as T;
				if (correctlyTyped != null)
				{
					return correctlyTyped;
				}

				parent = System.Windows.Media.VisualTreeHelper.GetParent(parent) as UIElement;
			}

			return null;
		}

		/// <summary>
		/// Save and delete bookmarks that are selected for deletion.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void Ok_btn_Click(object sender, RoutedEventArgs e)
		{
			for (int i = 0; i < this.bookmarks.Count; i++)
			{
				if (this.bookmarks[i].Delete)
				{
					this.bookmarks.RemoveAt(i);
					i--;
				}
			}

			this.Close();
		}

		/// <summary>
		/// Cancel, reset and close.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		/// <remarks>Delete boolean for bookmarks that are selected for deletion.</remarks>
		private void Cancel_btn_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// Allows one click editing.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
		private void DataGridCell_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			DataGridCell cell = sender as DataGridCell;
			if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
			{
				if (!cell.IsFocused)
				{
					cell.Focus();
				}

				DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
				if (dataGrid != null)
				{
					if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
					{
						if (!cell.IsSelected)
						{
							cell.IsSelected = true;
						}
					}
					else
					{
						DataGridRow row = FindVisualParent<DataGridRow>(cell);
						if (row != null && !row.IsSelected)
						{
							row.IsSelected = true;
						}
					}
				}
			}
		}

		/// <summary>
		/// Handles the Closing event of the ManageBookmarksWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void ManageBookmarksWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			for (int i = 0; i < this.bookmarks.Count; i++)
			{
				this.bookmarks[i].Delete = false;
			}
		}
	}
}

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
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;

namespace CSharpComicViewer.WPF
{
	/// <summary>
	/// Interaction logic for About.xaml
	/// </summary>
	public partial class About
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="About"/> class.
		/// </summary>
		public About()
		{
			InitializeComponent();
		    DataContext = this;
			SetDescription();
		}

		/// <summary>
		/// Gets the name of the program.
		/// </summary>
		/// <value>
		/// The name of the program.
		/// </value>
		public string ProgramName
		{
			get
			{
				return ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute))).Title;
			}
		}

		/// <summary>
		/// Gets the version.
		/// </summary>
		public string Version
		{
			get
			{
				return string.Format("Version: {0}", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);
			}
		}

		/// <summary>
		/// Handles the Click event of the Hyperlink control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			// open URL
			Hyperlink source = sender as Hyperlink;

			if (source != null)
			{
				Process.Start(source.NavigateUri.ToString());
			}
		}

		/// <summary>
		/// Gets the copyright.
		/// </summary>
		public string Copyright
		{
			get
			{
				return ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute))).Copyright;
			}
		}

		/// <summary>
		/// Handles the Click event of the Close control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void Close_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// Sets the description.
		/// </summary>
		private void SetDescription()
		{
			Description_TextBox.Text = ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyDescriptionAttribute))).Description;
		}
	}
}

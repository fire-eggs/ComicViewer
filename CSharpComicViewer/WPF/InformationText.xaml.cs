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
using System.Windows.Markup;

namespace CSharpComicViewer.WPF
{
	/// <summary>
	/// Interaction logic for About.xaml
	/// </summary>
	public partial class InformationText : Window
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InformationText"/> class.
		/// </summary>
		public InformationText()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InformationText"/> class.
		/// </summary>
		/// <param name="fileLocation">The file location.</param>
		/// <param name="infoText">The info text.</param>
		public InformationText(string fileLocation, string infoText)
		{
			InitializeComponent();
			Information_TextBox.Text = infoText;
			this.Title = "Info text from: \"" + fileLocation + "\"";
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
	}
}

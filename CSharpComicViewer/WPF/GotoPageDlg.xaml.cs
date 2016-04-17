//-------------------------------------------------------------------------------------
//  Copyright (c) 2013-2016 by Kevin Routley
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
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace CSharpComicViewer.WPF
{
    /// <summary>
    /// Interaction logic for GotoPageDlg.xaml
    /// </summary>
    public partial class GotoPageDlg
    {
        private readonly int _maxPage;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxPage"></param>
        public GotoPageDlg(int maxPage)
        {
            _maxPage = maxPage;
            InitializeComponent();
            Page = 0;
            DataContext = this;

            lblMaxPage.Content = " of " + _maxPage;
        }

        /// <summary>
        /// Requested page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Binding for the textbox in xaml. Enforces range.
        /// </summary>
        public string PageText
        {
            get
            {
                return Page.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                int newpage;
                if (!int.TryParse(value, out newpage))
                    return;
                Page = Math.Min(_maxPage, newpage); // disallow > max
                Page = Math.Max(1, Page);        // disallow < 1
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; 
            Close();
        }

        // Prevent space, non-numeric keys. Allow right/left arrow, tab, return, backspace.
        private void TxtPage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Back:
                case Key.Right:
                case Key.Left:
                case Key.Tab:
                case Key.Return:
                case Key.Delete:
                case Key.Escape: // allow to close dialog
                    break;
                default:
                    int val = (int)e.Key;
                    e.Handled = val > 43 || val < 34;  // 43 == '9', 34 == '0'
                    break;
            }
        }
    }
}

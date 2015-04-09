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

using System.Windows;
using System.Windows.Threading;
using CSharpComicViewer.WPF;

// ReSharper disable CSharpWarnings::CS1591
namespace CSharpComicViewer
{
    internal partial class App
	{
	    public App()
	    {
	        Dispatcher.UnhandledException += OnUnhandledException;
	    }

	    private MainDisplay _mainWindow;

	    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	    {
	        if (_mainWindow != null)
	        {
	            _mainWindow.ApplicationExit(null, null); // KBR TODO hack: force configuration save on exception
	        }
	    }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
	        var initial = e.Args.Length > 0 ? e.Args[0] : null;
            _mainWindow = new MainDisplay(initial);
            _mainWindow.Show();
		}
	}
}
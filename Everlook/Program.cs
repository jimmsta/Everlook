﻿//
//  Program.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2016 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Globalization;
using System.IO;
using Everlook.UI;
using Everlook.Utility;
using Gtk;
using log4net;
using OpenTK;

namespace Everlook
{
	internal static class Program
	{
		/// <summary>
		/// Logger instance for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

		public static void Main()
		{
			// Bind any unhandled exceptions in the main thread so that they are logged.
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			// Set correct working directory for compatibility with double-clicking
			Directory.SetCurrentDirectory(DirectoryHelpers.GetLocalDir());

			log4net.Config.XmlConfigurator.Configure();

			Log.Info("----------------");
			Log.Info("Initializing Everlook...");

			Log.Info("Initializing OpenGL...");

			// OpenGL
			Toolkit.Init(new ToolkitOptions
			{
				Backend = PlatformBackend.PreferNative,
				EnableHighResolution = true
			});

			Log.Info($"OpenGL initialized using the {(OpenTK.Configuration.RunningOnSdl2 ? "SDL2" : "native")} backend.");


			Log.Info("Initializing GTK...");

			// Bind any unhandled exceptions in the GTK UI so that they are logged.
			GLib.ExceptionManager.UnhandledException += OnGLibUnhandledException;

			// GTK
			IconManager.LoadEmbeddedIcons();
			Application.Init();
			MainWindow win = MainWindow.Create();
			win.Show();
			Application.Run();
		}

		/// <summary>
		/// Passes any unhandled exceptions from the GTK UI to the generic handler.
		/// </summary>
		/// <param name="args">The event object containing the information about the exception.</param>
		private static void OnGLibUnhandledException(GLib.UnhandledExceptionArgs args)
		{
			OnUnhandledException(null, args);
		}

		/// <summary>
		///	Event handler for all unhandled exceptions that may be encountered during runtime. While there should never
		/// be any unhandled exceptions in an ideal program, unexpected issues can and will arise. This handler logs
		/// the exception and all relevant information to a logfile and prints it to the console for debugging purposes.
		/// </summary>
		/// <param name="sender">The sending object.</param>
		/// <param name="unhandledExceptionEventArgs">The event object containing the information about the exception.</param>
		private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
		{
			// Force english exception output
			System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

			Log.Fatal("----------------");
			Log.Fatal("FATAL UNHANDLED EXCEPTION!");
			Log.Fatal("Something has gone terribly, terribly wrong during runtime.");
			Log.Fatal("The following is what information could be gathered by the program before crashing.");
			Log.Fatal("Please report this to <jarl.gullberg@gmail.com> or via GitHub. Include the full log and a " +
			          "description of what you were doing when it happened.");

			Exception unhandledException = unhandledExceptionEventArgs.ExceptionObject as Exception;
			if (unhandledException != null)
			{
				if (unhandledException.GetType() == typeof(DllNotFoundException))
				{
					Log.Fatal("This exception is typical of instances where the GTK# runtime has not been installed.\n" +
					          "If you haven't installed it, download it at \'https://download.gnome.org/binaries/win32/gtk-sharp/2.99/gtk-sharp-2.99.3.msi\'.\n" +
					          "If you have installed it, reboot your computer and try again.");
				}

				Log.Fatal("Exception type: " + unhandledException.GetType().FullName);
				Log.Fatal("Exception Message: " + unhandledException.Message);
				Log.Fatal("Exception Stacktrace: " + unhandledException.StackTrace);
			}
		}
	}
}

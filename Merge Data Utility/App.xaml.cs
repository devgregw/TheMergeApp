#region LICENSE

// Project Merge Data Utility:  App.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 06/23/2017 at 10:43 AM.
// 
// The MIT License (MIT)
// 
// Copyright (c) 2017 Greg Whatley
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

#region USINGS

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using Merge_Data_Utility.Tools;

#endregion

namespace Merge_Data_Utility {
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public App() {
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                var ex = (Exception)e.ExceptionObject;
                var name = $"crash-{DateTime.Now.ToString("MM-dd-yyyy-hh-mm-tt", CultureInfo.CurrentUICulture)}.txt";
                File.WriteAllLines(name,
                    new[] {
                        $"-- Merge Data Utility (version {VersionInfo.VersionString} update {VersionInfo.Update}) CRASH REPORT --",
                        "Please do not modify this file!",
                        "",
                        $"Time of crash: {DateTime.Now.ToShortDateString()} at {DateTime.Now.ToLongTimeString()}",
                        $"Windows version: {Environment.OSVersion.VersionString}"
                    });
                File.AppendAllLines(name,
                                new[] {
                        "",
                        "-- Exception Details --",
                        ex.ToString()
                                });
                File.AppendAllLines(name, new[] {

                        "",
                        "-- Log --"
                });
                File.AppendAllLines(name, Extensions.LogText);

                //File.WriteAllText(name, $"-- MERGE DATA UTILITY (version {VersionInfo.VersionString} update {VersionInfo.Update}) CRASH REPORT--\nDO NOT MODIFY THIS FILE\n\nTime of crash: {DateTime.Now.ToShortDateString()} at {DateTime.Now.ToLongTimeString()}\nWindows: {Environment.OSVersion.VersionString}\n\n--LOG--\n\n{Extensions.LogText}\n--EXCEPTION STACK TRACE--\n\n{ex.ToString()}");
                MessageBox.Show($"Unfortunately, a fatal error has occurred and the Merge Data Utility must exit.  A crash report was saved here: {new FileInfo(name).FullName}.  Please attach this file to an email and sent it to devgregw@outlook.com.\nClick OK to exit.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                Current.Shutdown();
            };
#endif
        }
    }
}
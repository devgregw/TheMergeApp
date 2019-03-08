#region LICENSE

// Project mdu-updater:  MainWindow.xaml.cs (in Solution mdu-updater)
// Created by Greg Whatley on 06/24/2017 at 10:57 AM.
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
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;
using System.Windows;

#endregion

namespace mdu_updater {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            Loaded += (s, e) => {
                using (var client = new WebClient()) {
                    client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                    client.DownloadFileCompleted += (ss, ee) => {
                        status.Text = "Applying update...";
                        progress.IsIndeterminate = true;
                        using (var archive = ZipFile.OpenRead("update.zip")) {
                            foreach (var file in archive.Entries)
                                if (File.Exists(file.FullName))
                                    File.Delete(file.FullName);
                        }
                        ZipFile.ExtractToDirectory("update.zip", Environment.CurrentDirectory);
                        status.Text = "Complete.";
                        progress.IsIndeterminate = false;
                        progress.Value = 0;
                        if (MessageBox.Show(this,
                                "The update has completed.  Do you want to launch Merge Data Utility now?",
                                "Update Complete", MessageBoxButton.YesNo, MessageBoxImage.Information,
                                MessageBoxResult.Yes) == MessageBoxResult.Yes)
                            Process.Start("Merge Data Utility.exe");
                        Close();
                    };
                    client.DownloadProgressChanged += (ss, ee) => {
                        status.Text =
                            $"Downloading update...\n{ee.BytesReceived} bytes received.\n{ee.TotalBytesToReceive - ee.BytesReceived} bytes remaining.";
                        progress.IsIndeterminate = false;
                        progress.Value = ee.ProgressPercentage;
                    };
                    client.DownloadFileAsync(new Uri("https://merge.gregwhatley.dev/utility/latest.zip"), "update.zip");
                }
            };
        }
    }
}
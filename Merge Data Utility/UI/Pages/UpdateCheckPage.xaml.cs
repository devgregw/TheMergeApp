#region LICENSE

// Project Merge Data Utility:  UpdateCheckPage.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 06/23/2017 at 10:45 AM.
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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

#endregion

namespace Merge_Data_Utility.UI.Pages {
    /// <summary>
    ///     Interaction logic for UpdateCheckPage.xaml
    /// </summary>
    public partial class UpdateCheckPage : Page {
        public UpdateCheckPage(int tab = 0, bool skip = false) {
            InitializeComponent();
            Loaded += async (s, e) => {
                ConsoleVersion info = null;
                try {
                    info = await CheckForUpdate();
                } catch (Exception ex) {
                    MessageBox.Show(
                        $"An error occurred while checking for updates.  You may continue to use the Merge Data Utility.\n{ex.Message} ({ex.GetType().FullName})",
                        "Check for Updates", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    info = null;
                } finally {
                    if (info != null && info.IsAvailable)
                        NavigationService.Navigate(new UpdatePromptPage(info, tab, skip));
                    else
                        NavigationService.Navigate(skip ? (Page) new MainPage(tab) : new AuthenticationPage());
                }
            };
        }

        private async Task<ConsoleVersion[]> GetVersions() {
            using (var client = new WebClient()) {
                client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                var xml =
                    XDocument.Parse(
                        await client.DownloadStringTaskAsync("https://merge.devgregw.com/utility/versions.xml"));
                var versions = new List<ConsoleVersion>();
                foreach (var e in xml.Root.Elements())
                    versions.Add(new ConsoleVersion(Version.Parse(e.Attribute("code").Value),
                        bool.Parse(e.Attribute("require").Value), e.Attribute("file").Value));
                return versions.ToArray();
            }
        }

        private async Task<ConsoleVersion> CheckForUpdate() {
            var versions = await GetVersions();
            var latest = versions.Where(v => v.Code > VersionInfo.Version).ToArray();
            if (latest.Length == 0)
                return new ConsoleVersion();
            return latest.Last();
        }

        public class ConsoleVersion {
            public ConsoleVersion() {
                IsAvailable = false;
            }

            public ConsoleVersion(Version code, bool require, string file) {
                Code = code;
                Require = require;
                File = file;
                IsAvailable = true;
            }

            public Version Code { get; }

            public bool Require { get; }

            public string File { get; }

            public bool IsAvailable { get; }
        }
    }
}
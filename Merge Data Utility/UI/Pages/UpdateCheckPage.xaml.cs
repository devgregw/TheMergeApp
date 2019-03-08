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
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

#endregion

namespace Merge_Data_Utility.UI.Pages {
    /// <summary>
    ///     Interaction logic for UpdateCheckPage.xaml
    /// </summary>
    public partial class UpdateCheckPage : Page {
        public UpdateCheckPage(int tab = 0, bool skip = false) {
            InitializeComponent();
            Loaded += async (s, e) => {
                UtilityVersion info = null;
                try {
                    info = (await GetVersionsAsync()).LastOrDefault();
                } catch (Exception ex) {
                    MessageBox.Show(
                        $"An error occurred while checking for updates.  You may continue to use the Merge Data Utility.\n{ex.Message} ({ex.GetType().FullName})",
                        "Check for Updates", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    info = null;
                } finally {
                    if (info != null && info.Version > VersionInfo.Version)
                        NavigationService.Navigate(new UpdatePromptPage(info, tab, skip));
                    else
                        NavigationService.Navigate(skip ? (Page) new MainPage(tab) : new AuthenticationPage());
                }
            };
        }

        private async Task<UtilityVersion[]> GetVersionsAsync() {
            using (var client = new WebClient()) {
                client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                var json = JObject.Parse(
                    await client.DownloadStringTaskAsync("https://merge.gregwhatley.dev/utility/versions.json"));
                var array = json.Value<JArray>("versions");
                return array.Select(token => token.ToObject<UtilityVersion>()).OrderBy(v => v.Version).ToArray();
            }
        }

        public sealed class UtilityVersion {
            [JsonProperty("version")]
            [JsonConverter(typeof(VersionConverter))]
            public Version Version { get; set; }

            [JsonProperty("required")]
            public bool IsUpdateRequired { get; set; }

            [JsonProperty("note")]
            public string Note { get; set; }
        }
    }
}
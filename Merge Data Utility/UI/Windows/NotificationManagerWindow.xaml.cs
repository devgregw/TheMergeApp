#region LICENSE

// Project Merge Data Utility:  NotificationManagerWindow.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 05/13/2017 at 9:11 AM.
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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Merge_Data_Utility.UI.Windows {
    /// <summary>
    ///     Interaction logic for NotificationManagerWindow.xaml
    /// </summary>
    public partial class NotificationManagerWindow : Window {
        public NotificationManagerWindow() {
            InitializeComponent();
            Loaded += Reload;
        }

        public sealed class NotificationInfo {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("successful")]
            public int Successful { get; set; }

            [JsonProperty("failed")]
            public int Failed { get; set; }

            [JsonProperty("remaining")]
            public int Remaining { get; set; }

            [JsonProperty("converted")]
            public int Converted { get; set; }

            [JsonProperty("send_after")]
            public int SendAfter { get; set; }

            [JsonProperty("headings")]
            public IDictionary<string, string> Headings { get; set; }

            [JsonProperty("contents")]
            public IDictionary<string, string> Contents { get; set; }

            [JsonProperty("canceled")]
            public bool Canceled { get; set; }

            public NotificationInfo Fix() {
                //Headings["en"] = Encoding.Unicode.GetString(Encoding.Convert(Encoding.UTF8, Encoding.Unicode, Encoding.UTF8.GetBytes(Headings["en"])));
                //Contents["en"] = Encoding.Unicode.GetString(Encoding.Convert(Encoding.UTF8, Encoding.Unicode, Encoding.UTF8.GetBytes(Contents["en"])));
                return this;
            }
        }

        private async void Reload(object sender, RoutedEventArgs e) {
            var reference = new LoaderReference(content);
            reference.StartLoading("Crunching the latest data...");
            inProgressList.Children.Clear();
            scheduledList.Children.Clear();
            historyList.Children.Clear();
            using (var web = new WebClient()) {
                web.Headers.Add(HttpRequestHeader.Authorization,
                    "Basic MGNlYmMzYjAtZmMxMS00MjQxLTgxOGUtOGI4ZWU0YTQ0YzUz");
                //var client = new OneSignalClient("MGNlYmMzYjAtZmMxMS00MjQxLTgxOGUtOGI4ZWU0YTQ0YzUz");
                var inProgress = new List<NotificationInfo>();
                var scheduled = new List<NotificationInfo>();
                var all = new List<NotificationInfo>();
                var json = JObject.Parse(
                    await web.DownloadStringTaskAsync(
                        "https://onesignal.com/api/v1/notifications?app_id=b52deecc-3f20-4904-a3f0-fd8e9aabb2b3"));
                all.AddRange(json.Value<JArray>("notifications").Select(obj => obj.ToObject<NotificationInfo>().Fix()));
                foreach (var n in all) {
                    if (n.SendAfter.ToDateTime() > DateTime.Now)
                        scheduled.Add(n);
                    else if (n.Remaining > 0)
                        inProgress.Add(n);
                }
                json = JObject.Parse(
                    await web.DownloadStringTaskAsync(
                        $"https://onesignal.com/api/v1/notifications?app_id=b52deecc-3f20-4904-a3f0-fd8e9aabb2b3&offset={inProgress.Count + scheduled.Count}"));
                var history = json.Value<JArray>("notifications").Select(obj => obj.ToObject<NotificationInfo>().Fix()).ToList();
                inProgress.ForEach(n => inProgressList.Children.Add(new NotificationControl(n, true)));
                scheduled.ForEach(n => scheduledList.Children.Add(new NotificationControl(n, true)));
                history.ForEach(n => historyList.Children.Add(new NotificationControl(n, false)));
                if (inProgressList.Children.Count == 0)
                    inProgressList.Children.Add(new TextBlock {
                        Text = "No data found.",
                        Margin = new Thickness(5),
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    });
                if (scheduledList.Children.Count == 0)
                    scheduledList.Children.Add(new TextBlock {
                        Text = "No data found.",
                        Margin = new Thickness(5),
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    });
                if (historyList.Children.Count == 0)
                    historyList.Children.Add(new TextBlock {
                        Text = "No data found.",
                        Margin = new Thickness(5),
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    });
            }
            reference.StopLoading();
        }

        private void SendNotification(object sender, RoutedEventArgs e) {
            new NotificationSenderWindow().ShowDialog();
            Reload(null, null);
        }

        private void OpenDashboard(object sender, RoutedEventArgs e) {
            Process.Start("https://www.onesignal.com");
        }
    }
}
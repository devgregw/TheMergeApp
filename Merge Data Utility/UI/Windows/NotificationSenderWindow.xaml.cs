#region LICENSE

// Project Merge Data Utility:  NotificationSenderWindow.xaml.cs (in Solution Merge Data Utility)
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Client;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Merge_Data_Utility.UI.Windows {
    /// <summary>
    ///     Interaction logic for NotificationSenderWindow.xaml
    /// </summary>
    public partial class NotificationSenderWindow : Window {
        public NotificationSenderWindow() {
            InitializeComponent();
        }

        private bool _usePlayerId => player.Visibility == Visibility.Visible;

        private async void Send(object sender, RoutedEventArgs e) {
            var errors = new List<string>();
            errors.Add(string.IsNullOrWhiteSpace(titleBox.Text) ? "No title specified." : "");
            errors.Add(string.IsNullOrWhiteSpace(messageBox.Text) ? "No message specified." : "");
            if (!_usePlayerId) {
                errors.Add(gradesField.Value.Count == 0 ? "At least one grade level must be selected." : "");
                errors.Add(gendersField.Value.Count == 0 ? "At least one gender must be selected." : "");
                errors.Add(platformsBox.SelectedItems.Count == 0 ? "At least one platform must be selected." : "");
            } else {
                errors.Add(string.IsNullOrWhiteSpace(player.Text) ? "No player ID specified." : "");
            }
            if (scheduling.IsChecked.GetValueOrDefault(false))
                errors.Add(schedulingDateTime.Value.GetValueOrDefault(DateTime.Now) <= DateTime.Now
                    ? "The specified date and time is invalid."
                    : "");
            errors.RemoveAll(string.IsNullOrWhiteSpace);
            if (errors.Any()) {
                new EditorPage.InputValidationResult(errors).Display(null);
            } else {
                var reference = new LoaderReference(scroller);
                reference.StartLoading("Processing...");
                /*var options = new NotificationCreateOptions {
                    AppId = Guid.Parse("b52deecc-3f20-4904-a3f0-fd8e9aabb2b3"),
                    Headings = {
                        { "en", titleBox.Text }
                    },
                    Contents = {
                        { "en", messageBox.Text }
                    },
                    IosBadgeType = IosBadgeTypeEnum.None,
                    SmallAndroidIcon = "ic_notification",
                    AndroidLedColor = "#FFFFFF00"
                };
                options.Data = new Dictionary<string, string>();
                options.IosAttachments = new Dictionary<string, string>();
                options.IncludePlayerIds = new List<string>();
                options.IncludedSegments = new List<string>();
                if (badge.IsChecked.GetValueOrDefault(false)) {
                    options.IosBadgeType = IosBadgeTypeEnum.Increase;
                    options.IosBadgeCount = 1;
                }
                if (scheduling.IsChecked.GetValueOrDefault(false)) {
                    // ReSharper disable once PossibleInvalidOperationException
                    options.SendAfter = schedulingDateTime.Value.Value;
                }
                if (action.SelectedAction != null)
                    options.Data.Add("action", JsonConvert.SerializeObject(action.SelectedAction));
                if (!string.IsNullOrWhiteSpace(cover.Value)) {
                    var url = await cover.PerformChangesAsync(
                        $"OneSignal-Notification-{DateTime.Now.ToString("Mdyyyy\"-\"hmmtt", CultureInfo.CurrentUICulture)}", null);
                    options.IosAttachments.Add("pic", url);
                    options.BigPictureForAndroid = url;
                }
                if (_usePlayerId)
                    options.IncludePlayerIds.Add(player.Text);
                else {
                    foreach (var grade in EnumConsts.AllGradeLevels)
                        if (gradesField.Value.Contains(grade))
                            options.IncludedSegments.Add($"{grade} Graders");
                    foreach (var gender in EnumConsts.AllGenders)
                        if (gendersField.Value.Contains(gender))
                            options.IncludedSegments.Add(gender == Gender.Male ? "Guys" : "Girls");
                    options.DeliverToAndroid = platformsBox.SelectedItems.Contains(platformsBox.Items[0]);
                    options.DeliverToIos = platformsBox.SelectedItems.Contains(platformsBox.Items[1]);
                }
                await Task.Run(() => new OneSignalClient("MGNlYmMzYjAtZmMxMS00MjQxLTgxOGUtOGI4ZWU0YTQ0YzUz").Notifications.Create(options));*/
                var options = new JObject(
                    new JProperty("app_id", "b52deecc-3f20-4904-a3f0-fd8e9aabb2b3"),
                    new JProperty("headings",
                        new JObject(new JProperty("en", titleBox.Text))),
                    new JProperty("contents",
                        new JObject(new JProperty("en", messageBox.Text))),
                    new JProperty("small_icon", "ic_notification"),
                    new JProperty("android_led_color", "FFFFFF00"));
                if (badge.IsChecked.GetValueOrDefault(false)) {
                    options.Add(new JProperty("ios_badgeType", "Increase"));
                    options.Add(new JProperty("ios_badgeCount", 1));
                }
                if (scheduling.IsChecked.GetValueOrDefault(false)) {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                    // ReSharper disable once PossibleInvalidOperationException
                    var dateTime = schedulingDateTime.Value.Value;
                    // ReSharper disable once PossibleInvalidOperationException
                    var offset = timeZone.IsDaylightSavingTime(dateTime) ? 5 : 6;
                    options.Add(new JProperty("send_after",
                        schedulingDateTime.Value.Value.ToString($"yyyy-MM-dd HH:mm:ss \"GMT-0{offset}00\"")));
                }
                if (action.SelectedAction != null)
                    options.Add(new JProperty("action", JsonConvert.SerializeObject(action.SelectedAction)));
                if (!string.IsNullOrWhiteSpace(cover.Value)) {
                    var url = /*await cover.PerformChangesAsync(
                        $"OneSignal-Notification-{DateTime.Now.ToString("Mdyyyy\"-\"hmmtt", CultureInfo.CurrentUICulture)}"
                            .ToLower())*/"";
                    options.Add(new JProperty("ios_attachments", new JObject(new JProperty("pic", url))));
                    options.Add(new JProperty("big_picture", url));
                }
                if (_usePlayerId) {
                    options.Add(new JProperty("include_player_ids", new JArray(player.Text)));
                } else {
                    var segments = new JArray();
                    foreach (var grade in EnumConsts.AllGradeLevels)
                        if (gradesField.Value.Contains(grade))
                            segments.Add($"{grade} Graders");
                    foreach (var gender in EnumConsts.AllGenders)
                        if (gendersField.Value.Contains(gender))
                            segments.Add(gender == Gender.Male ? "Guys" : "Girls");
                    options.Add(new JProperty("included_segments", segments));
                    options.Add(new JProperty("isAndroid", platformsBox.SelectedItems.Contains(platformsBox.Items[0])));
                    options.Add(new JProperty("isIos", platformsBox.SelectedItems.Contains(platformsBox.Items[1])));
                }
                var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;
                request.KeepAlive = true;
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.Headers.Add("Authorization", $"Basic MGNlYmMzYjAtZmMxMS00MjQxLTgxOGUtOGI4ZWU0YTQ0YzUz");
                var bytes = Encoding.UTF8.GetBytes(options.ToString());
                var responseContent = "<nothing>";
                try {
                    using (var writer = await request.GetRequestStreamAsync()) {
                        writer.Write(bytes, 0, bytes.Length);
                    }
                    using (var response = (HttpWebResponse) await request.GetResponseAsync())
                    using (var reader = new StreamReader(response.GetResponseStream())) {
                        responseContent = reader.ReadToEnd();
                    }
                } catch (Exception ex) {
                    Debug.WriteLine($"ONESIGNAL EXCEPTION: {ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}");
                }
                if (responseContent != "<nothing>")
                    Debug.WriteLine(responseContent);
                Close();
            }
        }

        private void Import(object sender, RoutedEventArgs e) {
            var window = new ObjectChooserWindow(async () =>
                (await MergeDatabase.ListAsync<MergeEvent>()).Cast<ModelBase>()
                .Concat(await MergeDatabase.ListAsync<MergePage>()).Select(m => new ListViewItem {
                    Content = $"{(m is MergeEvent ? "Event" : "Page")}: {m.Title} ({m.Id})",
                    Tag = m
                }));
            window.ShowDialog();
            if (!window.ObjectSelected) return;
            var obj = window.GetSelectedObject<ModelBase>();
            titleBox.Text = obj.Title;
            messageBox.Text = obj.ShortDescription;
            action.DefaultAction = obj is MergeEvent
                ? (ActionBase) OpenEventDetailsAction.FromEventId(obj.Id)
                : OpenPageAction.FromPageId(obj.Id);
            action.Reset();
            cover.SetOriginalValue(obj.CoverImage);
            gradesField.Value = obj.GradeLevels;
            gendersField.Value = obj.Genders;
        }

        private void PlatformsSelectAll_Click(object sender, RoutedEventArgs e) {
            platformsBox.SelectedItems.Clear();
            platformsBox.SelectedItems.Add(platformsBox.Items[0]);
            platformsBox.SelectedItems.Add(platformsBox.Items[1]);
        }

        private void PlatformsSelectNone_Click(object sender, RoutedEventArgs e) {
            platformsBox.SelectedItems.Clear();
        }

        private void SchedulingCheck(object sender, RoutedEventArgs e) {
            schedulingDateTime.IsEnabled = scheduling.IsChecked.GetValueOrDefault(false);
        }

        private void PlayerId(object sender, RoutedEventArgs e) {
            player.Visibility = player.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
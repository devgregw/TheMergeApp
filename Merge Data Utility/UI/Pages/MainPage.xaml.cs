#region LICENSE

// Project Merge Data Utility:  MainPage.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 03/20/2017 at 6:42 PM.
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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MergeApi.Client;
using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Interfaces;
using MergeApi.Models.Core;
using MergeApi.Models.Core.Attendance;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Controls;
using Merge_Data_Utility.UI.Pages.Base;
using Merge_Data_Utility.UI.Pages.Editors;
using Merge_Data_Utility.UI.Windows;
using Merge_Data_Utility.UI.Windows.Choosers;
using Newtonsoft.Json.Linq;
using ValidationResult = MergeApi.Tools.ValidationResult;

#endregion

namespace Merge_Data_Utility.UI.Pages {
    /// <summary>
    ///     Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page {
        public MainPage(int tab) {
            InitializeComponent();
            reload.Click += (s, e) => ReloadEverything();
            Loaded += (s, args) => InitializeEverything(tab);
        }

        public void ReloadEverything() {
            NavigationService.Navigate(new MainPage(tabs.SelectedIndex));
        }

        public void InitializeEverything(int tab) {
            InitializeCore(tab);
            InitializeDrafts();

            InitializeAttendance();
            InitializeNotifications();
            InitializeAboutAdmin();
        }

        public void ClearLists() {
            eventsList.Children.Clear();
            groupsList.Children.Clear();
            pagesList.Children.Clear();
            leadersList.Children.Clear();
        }

        private void PopulateList<T>(StackPanel list, IEnumerable<T> c) where T : IIdentifiable {
            var content = c.ToList();
            content.ForEach(
                i =>
                    list.Children.Add(ModelControl.Create(i, false, o => HandleEdit(o, false),
                        o => HandleDelete((T) o, false))));
            if (list.Children.Count == 0)
                list.Children.Add(new TextBlock {
                    Text = $"No {typeof(T).Name.ToLower().Replace("merge", "")}s found.",
                    Margin = new Thickness(5),
                    FontStyle = FontStyles.Italic,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
        }

        public async void InitializeCore(int tab = -1) {
            var coreRef = new LoaderReference(coreBox);
            var attnRef = new LoaderReference(attentionBox);
            coreRef.StartLoading("Loading data...");
            attnRef.StartLoading("Validating data...");
            tabs.SelectedIndex = tab == -1 ? tabs.SelectedIndex : tab;
            ClearLists();
            var events = await MergeDatabase.ListAsync<MergeEvent>();
            var groups = await MergeDatabase.ListAsync<MergeGroup>();
            var pages = await MergeDatabase.ListAsync<MergePage>();
            //var leaders = await MergeDatabase.ListLeadersAsync();
            PopulateList(eventsList, events);
            PopulateList(groupsList, groups);
            PopulateList(pagesList, pages);
            //PopulateList(leadersList, leaders);
            InitializeTabManagers();
            coreRef.StopLoading();
            attentionList.Children.Clear();
            var validatables = events.Concat(pages.Cast<IValidatable>());
            var results = new List<ValidationResult>();
            foreach (var v in validatables)
                results.Add(await v.ValidateAsync());
            var failures = results.Where(r => r.ResultType != ValidationResultType.Success).ToList();
            if (!failures.Any()) {
                attentionList.Children.Add(new TextBlock {
                    Text = "Nothing needs your attention.",
                    Margin = new Thickness(5),
                    FontStyle = FontStyles.Italic,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                });
                attnHeader.FontSize = 12;
                attnHeader.FontWeight = FontWeights.Normal;
                attnHeader.Foreground = new SolidColorBrush(Colors.Black);
            } else {
                failures.ForEach(r => attentionList.Children.Add(new AttentionControl(r, () => InitializeCore())));
                attnHeader.FontSize = 16;
                attnHeader.FontWeight = FontWeights.Bold;
                attnHeader.Foreground = new SolidColorBrush(Colors.Red);
            }
            attnRef.StopLoading();
        }

        public void InitializeTabManagers() {
            eTab.Init(Tab.Events, InitializeDrafts);
            gTab.Init(Tab.Groups, InitializeDrafts);
            pTab.Init(Tab.Home, InitializeDrafts);
            //lTab.Init(Tab.Leaders, "Leaders");
        }

        public async void InitializeAttendance() {
            var reference = new LoaderReference(attendanceBox);
            reference.StartLoading("Loading attendance...");
            var records = (await MergeDatabase.ListAsync<AttendanceRecord>()).ToList();
            var groups = (await MergeDatabase.ListAsync<AttendanceGroup>()).ToList();
            var avg = AttendanceTools.GetAverageAttendance(records, groups);
            attendanceAvg.Text = avg == null ? "No data" : $"{avg.Item1} students/week ({avg.Item2}%)";
            var low = AttendanceTools.GetLowestRecordedAttendance(records);
            var high = AttendanceTools.GetHighestRecordedAttendance(records);
            var recent = AttendanceTools.GetMostRecentAttendance(records);
            attendanceRecordLow.Text = low == null
                ? "No data"
                : $"{low.Item1} students ({low.Item2.ToShortDateString()})";
            attendanceRecordHigh.Text = high == null
                ? "No data"
                : $"{high.Item1} students ({high.Item2.ToShortDateString()})";
            attendanceRecent.Text = recent == null
                ? "No data"
                : $"{recent.Item1} students ({recent.Item2.ToShortDateString()})";
            var totals = AttendanceTools.GetTotals(groups);
            attendanceTotalGroups.Text = totals?.Item1.ToString() ?? "0";
            attendanceTotalStudents.Text = totals?.Item2.ToString() ?? "0";
            reference.StopLoading();
        }

        public void InitializeDrafts() {
            draftsList.Children.Clear();
            foreach (var draft in DraftManager.GetAllDrafts()) {
                var c = ModelControl.Create(draft, true, o => { HandleEdit(o, true); },
                    o => { HandleDelete((IIdentifiable) o, true); });
                c.Minify();
                draftsList.Children.Add(c);
            }
            if (draftsList.Children.Count == 0)
                draftsList.Children.Add(new TextBlock {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    TextAlignment = TextAlignment.Center,
                    Text = "No drafts found.",
                    Margin = new Thickness(5),
                    FontStyle = FontStyles.Italic
                });
        }

        public async void InitializeNotifications() {
            /*var reference = new LoaderReference(notificationsBox);
            reference.StartLoading("Loading notification data...");
            using (var client = new WebClient()) {
                client.Headers.Add(HttpRequestHeader.Authorization,
                    "Basic ZmEwYWRhMDUtNmUxZC00M2QyLWFkMDEtNTAwOGEwZjk1OTUx");
                var response =
                    JObject.Parse(
                        await client.DownloadStringTaskAsync(
                            "https://onesignal.com/api/v1/apps/b52deecc-3f20-4904-a3f0-fd8e9aabb2b3"));
                messageableUsers.Text = response.Value<int>("messageable_players").ToString();
            }
            reference.StopLoading();*/
        }

        public void InitializeAboutAdmin() {
            aboutMduVersion.Text = $"{VersionInfo.Version} (update {VersionInfo.Update})";
            aboutLibVersion.Text = $"{MergeApi.Tools.VersionInfo.Version} (update {MergeApi.Tools.VersionInfo.Update})";
        }

        private void NewEvent_Click(object sender, RoutedEventArgs e) {
            HandleNew(new EventEditorPage(null, false));
        }

        private void NewMergeGroup_Click(object sender, RoutedEventArgs e) {
            HandleNew(new GroupEditorPage(null, false));
        }

        private void NewPage_Click(object sender, RoutedEventArgs e) {
            HandleNew(new PageEditorPage(null, false));
        }

        private void attendanceManager_Click(object sender, RoutedEventArgs e) {
            new AttendanceManagerWindow().ShowDialog();
            InitializeAttendance();
        }

        private void notificationManager_Click(object sender, RoutedEventArgs e) {
            new NotificationManagerWindow().Show();
        }

        private void notificationDashboard_Click(object sender, RoutedEventArgs e) {
            Process.Start("https://www.onesignal.com/");
        }

        #region Handlers

        private void HandleNew(EditorPage p) {
            new EditorWindow(p, EditorResult).Show();
        }

        private void EditorResult(EditorWindow.ResultType result) {
            if (result == EditorWindow.ResultType.Published)
                InitializeCore();
            InitializeDrafts();
        }

        private void HandleEdit(object source, bool draft) {
            new EditorWindow(source, draft, EditorResult).Show();
        }

        private async void HandleDelete<T>(T item, bool draft) where T : IIdentifiable {
            if (MessageBox.Show(this.GetWindow(),
                    $"Are you sure you want to delete this {(draft ? "draft" : "object")}?  This action cannot be undone.",
                    $"Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation,
                    MessageBoxResult.No) != MessageBoxResult.Yes) return;
            if (draft) {
                DraftManager.AutoDelete(item);
                InitializeDrafts();
            } else {
                await MergeDatabase.DeleteAsync(item);
                InitializeCore();
            }
        }

        #endregion

        private void CheckForUpdates(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new UpdateCheckPage(tabs.SelectedIndex, true));
        }

        private void ActionCodeViewer_Click(object sender, RoutedEventArgs e) {
            new ActionCodeViewerWindow().ShowDialog();
        }

        private void NotificationComposer_Click(object sender, RoutedEventArgs e) {
            Process.Start("https://console.firebase.google.com/project/the-merge-app/notification/compose");
        }

        private void FileBrowser_Click(object sender, RoutedEventArgs e) {
            new FileBrowserWindow().ShowDialog();
        }
    }
}
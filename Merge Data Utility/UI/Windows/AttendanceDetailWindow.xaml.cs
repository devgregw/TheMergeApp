#region LICENSE

// Project Merge Data Utility:  AttendanceDetailWindow.xaml.cs (in Solution Merge Data Utility)
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

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Framework.Enumerations.Converters;
using MergeApi.Models.Core.Attendance;
using Merge_Data_Utility.Tools;

#endregion

namespace Merge_Data_Utility.UI.Windows {
    /// <summary>
    ///     Interaction logic for AttendanceDetailWindow.xaml
    /// </summary>
    public partial class AttendanceDetailWindow : Window {
        public AttendanceDetailWindow() {
            InitializeComponent();
            /*treeView.SelectedItemChanged += (s, e) => {
                var state = ((TreeViewItem) treeView.SelectedItem)?.Tag != null;
                delButton.IsEnabled = state;
                editButton.IsEnabled = state;
            };*/
        }

        public AttendanceDetailWindow(IList<AttendanceRecord> records) : this() {
            InitWithRecords(records);
        }

        public AttendanceDetailWindow(AttendanceGroup group) : this() {
            InitWithGroup(group);
        }

        private AttendanceRecord GetSelectedRecord() {
            return (AttendanceRecord) ((TreeViewItem) treeView.SelectedItem)?.Tag;
        }

        private TreeViewItem MakeGroupItem(AttendanceRecord record, AttendanceGroup group) {
            var root = new TreeViewItem {
                Header =
                    $"{group.Summary} ({record.Students.Count} students, leader(s) present: {record.LeadersPresent.YesOrNo()})",
                Tag = record
            };
            foreach (var s in record.Students)
                root.Items.Add(new TreeViewItem {
                    Header = s,
                    Tag = record
                });
            return root;
        }

        private async void InitWithRecords(IList<AttendanceRecord> records) {
            var @ref = new LoaderReference(content);
            @ref.StartLoading(
                $"Loading attendance records for {records[0].Date.WithoutTime().ToLongDateString()} (attendance/records/date/{records[0].Date.ToString("MMddyyyy", CultureInfo.CurrentUICulture)})...");
            date.Text = records[0].Date.WithoutTime().ToLongDateString();
            var split = new List<List<AttendanceRecord>>();
            var groups = new List<AttendanceGroup>();
            foreach (var r in records) {
                if (groups.All(g => g.Id != r.GroupId))
                    groups.Add(await r.GetGroupAsync());
                var assoc = groups.First(g => g.Id == r.GroupId);
                var item = MakeGroupItem(r, assoc);
                if (GradeLevelConverter.ToInt32(assoc.GradeLevel) <= 8)
                    jhItem.Items.Add(item);
                else
                    hsItem.Items.Add(item);
            }
            if (jhItem.Items.Count == 0)
                jhItem.Items.Add(new TreeViewItem {
                    Header = "No data"
                });
            if (hsItem.Items.Count == 0)
                hsItem.Items.Add(new TreeViewItem {
                    Header = "No data"
                });
            jhItem.Header =
                $"Junior High ({records.Where(r => GradeLevelConverter.ToInt32(groups.First(g => g.Id == r.GroupId).GradeLevel) <= 8).Sum(r => r.Students.Count)} students, {groups.Count(g => GradeLevelConverter.ToInt32(g.GradeLevel) <= 8)} groups)";
            hsItem.Header =
                $"High School ({records.Where(r => GradeLevelConverter.ToInt32(groups.First(g => g.Id == r.GroupId).GradeLevel) >= 9).Sum(r => r.Students.Count)} students, {groups.Count(g => GradeLevelConverter.ToInt32(g.GradeLevel) >= 9)} groups)";
            @ref.StopLoading();
        }

        private async void InitWithGroup(AttendanceGroup group) {
            var @ref = new LoaderReference(content);
            @ref.StartLoading(
                $"Loading attendance records for {group.Summary} (attendance/records/group/{group.Id})...");
            date.Text = group.Summary;
            var records = await group.GetRecordsAsync();
            if (records == null || !records.Any()) {
                MessageBox.Show(this, "No attendance data found.");
                Close();
                return;
            }
            treeView.Items.Clear();
            var average = AttendanceTools.GetMetrics(records, new List<AttendanceGroup> {
                group
            }, null, null);
            //var average = AttendanceTools.GetAverageAttendance(records, new[] {group}.ToList());
            var groupItem = new TreeViewItem {
                Header =
                    $"{group.Summary} ({records.Count} records, {average.AverageStudentCount} students ({average.AverageAttendancePercentage}%) on average)"
            };
            records.Sort((x, y) => y.Date.WithoutTime().CompareTo(x.Date.WithoutTime()));
            foreach (var r in records) {
                var dateItem = new TreeViewItem {
                    Header =
                        $"{r.Date.WithoutTime().ToLongDateString()} ({r.Students.Count} students, leader(s) present: {r.LeadersPresent.YesOrNo()})",
                    Tag = r
                };
                foreach (var s in r.Students)
                    dateItem.Items.Add(new TreeViewItem {
                        Header = s,
                        Tag = r
                    });
                groupItem.Items.Add(dateItem);
            }
            treeView.Items.Add(groupItem);
            @ref.StopLoading();
        }

        private void DeleteRecord(object sender, RoutedEventArgs e) {
            var r = GetSelectedRecord();
            MessageBox.Show(this, $"DELETE {r.Date.WithoutTime().ToLongDateString()}, {r.Students.Count}, {r.GroupId}");
        }

        private void EditRecord(object sender, RoutedEventArgs e) {
            var r = GetSelectedRecord();
            MessageBox.Show(this, $"EDIT {r.Date.WithoutTime().ToLongDateString()}, {r.Students.Count}, {r.GroupId}");
        }
    }
}
#region LICENSE

// Project Merge Data Utility:  AttendanceManagerWindow.xaml.cs (in Solution Merge Data Utility)
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MergeApi;
using MergeApi.Models.Core.Attendance;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Controls;

#endregion

namespace Merge_Data_Utility.UI.Windows {
    /// <summary>
    ///     Interaction logic for AttendanceManagerWindow.xaml
    /// </summary>
    public partial class AttendanceManagerWindow : Window {
        private List<AttendanceGroup> _groups;

        private List<AttendanceRecord> _records;

        public AttendanceManagerWindow() {
            InitializeComponent();
            Loaded += async (s, e) => await Refresh();
        }

        private async Task Refresh(Func<Task> preop = null) {
            var reference = new LoaderReference(content);
            reference.StartLoading();
            if (preop != null)
                await preop();
            _records = (await MergeDatabase.ListAsync<AttendanceRecord>()).OrderByDescending(r => r.Date).ToList();
            _groups = (await MergeDatabase.ListAsync<AttendanceGroup>()).ToList();
            Initialize();
            reference.StopLoading();
        }

        private void Initialize() {
            recordsList.Children.Clear();
            var rem = new List<object>();
            foreach (var r in _records) {
                if (rem.Contains(r.Date.WithoutTime()))
                    continue;
                rem.Add(r.Date.WithoutTime());
                recordsList.Children.Add(new AttendanceControl(_records, _groups, r.Date.WithoutTime()));
            }
            if (recordsList.Children.Count == 0)
                recordsList.Children.Add(new TextBlock {
                    Text = "No records found.",
                    Margin = new Thickness(5),
                    FontStyle = FontStyles.Italic,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            groupsList.Children.Clear();
            foreach (var g in _groups)
                groupsList.Children.Add(new AttendanceControl(_records, g,
                    _g => { EditorWindow.Create(g, false, async r => await Refresh()).ShowDialog(); }, async _g => {
                        if (
                            MessageBox.Show(this, "Are you sure you want to delete this group?", "Confirm",
                                MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) ==
                            MessageBoxResult.Yes)
                            await Refresh(async () => await MergeDatabase.DeleteAsync(g));
                    }));
            if (groupsList.Children.Count == 0)
                groupsList.Children.Add(new TextBlock {
                    Text = "No groups found.",
                    Margin = new Thickness(5),
                    FontStyle = FontStyles.Italic,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
        }

        private void NewRecord_Click(object sender, RoutedEventArgs e) {
            EditorWindow.Create<AttendanceRecord>(null, false, async r => await Refresh())
                .ShowDialog();
        }

        private void NewGroup_Click(object sender, RoutedEventArgs e) {
            EditorWindow.Create<AttendanceGroup>(null, false, async r => await Refresh())
                .ShowDialog();
        }
    }
}
#region LICENSE

// Project Merge Data Utility:  AttendanceRecordEditorPage.xaml.cs (in Solution Merge Data Utility)
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
using System.Windows.Input;
using MergeApi.Client;
using MergeApi.Models.Core.Attendance;
using Merge_Data_Utility.UI.Pages.Base;
using Merge_Data_Utility.UI.Windows;
using Xceed.Wpf.Toolkit.Primitives;

#endregion

namespace Merge_Data_Utility.UI.Pages.Editors {
    /// <summary>
    ///     Interaction logic for AttendanceRecordEditorPage.xaml
    /// </summary>
    public partial class AttendanceRecordEditorPage : EditorPage {
        private AttendanceGroup _group;

        public AttendanceRecordEditorPage() {
            InitializeComponent();
        }

        public AttendanceRecordEditorPage(AttendanceRecord src, bool draft) : this() {
            SetSource(src, false);
            DisableDrafting();
        }

        private void SelectGroup(AttendanceGroup g) {
            content.IsEnabled = true;
            _group = g;
            students.Items.Clear();
            foreach (var s in _group.StudentNames)
                students.Items.Add(new TextBlock {
                    TextWrapping = TextWrapping.Wrap,
                    Text = s
                });
            UpdateTitle();
        }

        private void Browse(object sender, RoutedEventArgs e) {
            var dialog = new ObjectChooserWindow(async () => (await MergeDatabase.ListAsync<AttendanceGroup>()).Select(
                g => new ListViewItem {
                    Content = $"{g.Summary} (attendance/groups/{g.Id})",
                    Tag = g
                }));
            dialog.ShowDialog();
            if (dialog.ObjectSelected)
                SelectGroup(dialog.GetSelectedObject<AttendanceGroup>());
        }

        private void StudentSelected(object sender, ItemSelectionChangedEventArgs e) {
            remove.IsEnabled = students.SelectedItem == null ||
                               _group.StudentNames.Contains(((TextBlock) students.SelectedItem).Text);
        }

        private void AddStudent(object sender, RoutedEventArgs e) {
            var dialog = new TextInputWindow("Add Student", "Enter student's name:",
                scope: InputScopeNameValue.PersonalFullName);
            dialog.ShowDialog();
            if (!string.IsNullOrWhiteSpace(dialog.Input))
                students.Items.Add(new TextBlock {
                    TextWrapping = TextWrapping.Wrap,
                    Text = dialog.Input
                });
        }

        private void RemoveStudent(object sender, RoutedEventArgs e) {
            students.Items.Remove(students.SelectedItem);
        }

        protected override async void Update() {
            if (!HasSource)
                return;
            var reference = GetLoaderReference();
            reference.StartLoading("Preparing...");
            var src = GetSource<AttendanceRecord>();
            date.SelectedDate = src.Date;
            SelectGroup(await src.GetGroupAsync());
            leaders.IsChecked = src.LeadersPresent;
            date.IsEnabled = false;
            browse.IsEnabled = false;
            reference.StopLoading();
        }

        protected override InputValidationResult ValidateInput() {
            if (_group == null)
                return new InputValidationResult(new List<string> {
                    "No attendance group selected."
                });
            var errors = new List<string>();
            errors.Add(date.SelectedDate.HasValue ? "" : "No data specified.");
            errors.RemoveAll(string.IsNullOrWhiteSpace);
            return new InputValidationResult(errors);
        }

        protected override Task<object> MakeObject() {
            return Task.FromResult((object) new AttendanceRecord {
                // ReSharper disable once PossibleInvalidOperationException
                Date = date.SelectedDate.Value,
                GroupId = _group.Id,
                Students = students.SelectedItems.Cast<TextBlock>().Select(t => t.Text).ToList(),
                LeadersPresent = leaders.IsChecked.GetValueOrDefault(true)
            });
        }

        public override string GetIdentifier() {
            return
                $"attendance/records/{date.SelectedDate?.ToString("MMddyyy") ?? "<no date>"}/{(_group == null ? "<no group>" : _group.Id)}";
        }

        public override async Task<bool> Publish() {
            var res = ValidateInput();
            if (!res.IsInputValid) {
                res.Display(Window);
            } else {
                var reference = GetLoaderReference();
                reference.StartLoading("Processing...");
                var o = (AttendanceRecord) await MakeObject();
                try {
                    if (students.Items.Count > _group.StudentNames.Count) {
                        MessageBox.Show(Window,
                            "The selected attendance group will be updated to include the additional students provided.",
                            "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                        _group.StudentNames = students.Items.Cast<TextBlock>().Select(t => t.Text).ToList();
                        await MergeDatabase.UpdateAsync(_group);
                    }
                    await MergeDatabase.UpdateAsync(o);
                    return true;
                } catch (Exception ex) {
                    MessageBox.Show(Window,
                        $"Could not update attendance/records/{o.DateString}/{o.GroupId} ({o.GetType().FullName}):\n{ex.Message}\n{ex.GetType().FullName}");
                }
            }
            return false;
        }

        public override Task SaveAsDraft() {
            throw new NotImplementedException();
        }
    }
}
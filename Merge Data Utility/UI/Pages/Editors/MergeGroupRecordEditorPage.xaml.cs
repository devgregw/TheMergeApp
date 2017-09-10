#region LICENSE

// Project Merge Data Utility:  MergeGroupRecordEditorPage.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 08/24/2017 at 5:14 PM.
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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Client;
using MergeApi.Models.Core;
using MergeApi.Models.Core.Attendance;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages.Base;
using Merge_Data_Utility.UI.Windows;

#endregion

namespace Merge_Data_Utility.UI.Pages.Editors {
    /// <summary>
    ///     Interaction logic for MergeGroupRecordEditorPage.xaml
    /// </summary>
    public partial class MergeGroupRecordEditorPage : EditorPage {
        private MergeGroup _group;

        public MergeGroupRecordEditorPage() {
            InitializeComponent();
        }

        public MergeGroupRecordEditorPage(MergeGroupAttendanceRecord src, bool draft) : this() {
            SetSource(src, false);
            DisableDrafting();
        }

        private void Browse(object sender, RoutedEventArgs e) {
            var dialog = new ObjectChooserWindow(async () => (await MergeDatabase.ListAsync<MergeGroup>()).Select(
                g => new ListViewItem {
                    Content = $"{g.Name} (groups/{g.Id})",
                    Tag = g
                }));
            dialog.ShowDialog();
            if (dialog.ObjectSelected)
                id.Text = (_group = dialog.GetSelectedObject<MergeGroup>()).Id;
        }

        protected override async void Update() {
            browse.IsEnabled = !HasSource;
            date.IsEnabled = !HasSource;
            if (!HasSource)
                return;
            var reference = new LoaderReference((ScrollViewer) Content);
            reference.StartLoading("Preparing...");
            var src = GetSource<MergeGroupAttendanceRecord>();
            date.SelectedDate = src.Date;
            id.Text = (_group = await src.GetMergeGroupAsync()).Id;
            studentCount.Value = src.StudentCount;
            if (!string.IsNullOrWhiteSpace(src.Image))
                imageField.SetOriginalValue(src.Image);
            reference.StopLoading();
        }

        protected override InputValidationResult ValidateInput() {
            var errors = new List<string> {
                date.SelectedDate.HasValue ? "" : "No date specified.",
                _group == null ? "No Merge group specified." : "",
                studentCount.Value.HasValue ? "" : "The specified student count is invalid."
            };
            errors.RemoveAll(string.IsNullOrWhiteSpace);
            return new InputValidationResult(errors);
        }

        protected override async Task<object> MakeObject() {
            return new MergeGroupAttendanceRecord {
                Date = date.SelectedDate.Value,
                Image = string.IsNullOrWhiteSpace(imageField.Value)
                    ? ""
                    : await imageField.PerformChangesAsync(
                        $"{_group.Id}-{date.SelectedDate.Value.ToString("MM-dd-yyyy", CultureInfo.CurrentUICulture)}",
                        ""),
                StudentCount = studentCount.Value.Value,
                MergeGroupId = id.Text
            };
        }

        public override string GetIdentifier() {
            return
                $"attendance/mgrecords/{date.SelectedDate?.ToString("MMddyyyy") ?? "<no date>"}/{(_group == null ? "<no group>" : _group.Id)}";
        }

        public override async Task<bool> Publish() {
            var res = ValidateInput();
            if (!res.IsInputValid) {
                res.Display(Window);
            } else {
                var reference = GetLoaderReference();
                reference.StartLoading("Processing...");
                var o = (MergeGroupAttendanceRecord) await MakeObject();
                try {
                    await MergeDatabase.UpdateAsync(o);
                    return true;
                } catch (Exception ex) {
                    MessageBox.Show(Window,
                        $"Could not update attendance/mgrecords/{o.Date.ToString("MMddyyyy", CultureInfo.CurrentUICulture)}/{o.MergeGroupId} ({o.GetType().FullName}):\n{ex.Message}\n{ex.GetType().FullName}");
                }
            }
            return false;
        }

        public override Task SaveAsDraft() {
            throw new NotImplementedException();
        }
    }
}
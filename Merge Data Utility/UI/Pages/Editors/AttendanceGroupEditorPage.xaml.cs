#region LICENSE

// Project Merge Data Utility:  AttendanceGroupEditorPage.xaml.cs (in Solution Merge Data Utility)
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
using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Enumerations.Converters;
using MergeApi.Models.Core.Attendance;
using Merge_Data_Utility.UI.Pages.Base;
using Merge_Data_Utility.UI.Windows;

#endregion

namespace Merge_Data_Utility.UI.Pages.Editors {
    /// <summary>
    ///     Interaction logic for AttendanceGroupEditorPage.xaml
    /// </summary>
    public partial class AttendanceGroupEditorPage : EditorPage {
        private List<(string Old, string New)> _renames;

        public AttendanceGroupEditorPage() {
            InitializeComponent();
        }

        public AttendanceGroupEditorPage(AttendanceGroup src, bool draft) : this() {
            SetSource(src, false);
            _renames = new List<(string Old, string New)>();
            DisableDrafting();
        }

        protected override void Update() {
            idField.Regenerated += (s, e) => { UpdateTitle(); };
            leadersList.Prepare(() => {
                var input = TextInputWindow.GetInput("Add Leader", "Enter leader's name:");
                return input == null
                    ? null
                    : new ListViewItem {
                        Content = input
                    };
            }, i => true, i => {
                var input = TextInputWindow.GetInput("Edit Leader", "Enter leader's name:", i.Content.ToString());
                return input == null
                    ? i
                    : new ListViewItem {
                        Content = input
                    };
            });
            studentsList.Prepare(() => {
                var input = TextInputWindow.GetInput("Add Student", "Enter student's name:");
                return input == null
                    ? null
                    : new ListViewItem {
                        Content = input
                    };
            }, i => true, i => {
                var input = TextInputWindow.GetInput("Edit Student", "Enter student's name:", i.Content.ToString());
                if (input != null)
                    _renames.Add((i.Content.ToString(), input));
                return input == null
                    ? i
                    : new ListViewItem {
                        Content = input
                    };
            });
            if (HasSource) {
                var gr = GetSource<AttendanceGroup>();
                idField.SetId(gr.Id, false);
                leadersList.SetItems(gr.LeaderNames.Select(n => new ListViewItem {
                    Content = n
                }));
                studentsList.SetItems(gr.StudentNames.Select(n => new ListViewItem {
                    Content = n
                }));
                gradeBox.SelectedIndex = GradeLevelConverter.ToInt32(gr.GradeLevel) - 7;
                genderBox.SelectedIndex = gr.Gender == Gender.Male ? 0 : 1;
            } else {
                idField.SetId("", true);
            }
        }

        protected override InputValidationResult ValidateInput() {
            var errors = new List<string> {
                leadersList.Count == 0 ? "At least one leader must be specified." : "",
                studentsList.Count == 0 ? "At least one student must be specified." : "",
                gradeBox.SelectedIndex == -1 ? "No grade level specified." : "",
                genderBox.SelectedIndex == -1 ? "No gender specified." : ""
            };
            errors.RemoveAll(string.IsNullOrWhiteSpace);
            return new InputValidationResult(errors);
        }

#pragma warning disable 1998
        protected override async Task<object> MakeObject() {
            return new AttendanceGroup {
                Id = idField.Id,
                LeaderNames = leadersList.GetItems(i => i.Content.ToString()).ToList(),
                StudentNames = studentsList.GetItems(i => i.Content.ToString())
                    .OrderBy(n => n.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries).Last()).ToList(),
                GradeLevel = GradeLevelConverter.FromString(((ComboBoxItem) gradeBox.SelectedItem).Content.ToString()),
                Gender = genderBox.SelectedIndex == 0 ? Gender.Male : Gender.Female
            };
        }
#pragma warning restore 1998

        public override string GetIdentifier() {
            return $"attendance/groups/{idField.Id}";
        }

        public override async Task<bool> Publish() {
            var res = ValidateInput();
            if (!res.IsInputValid) {
                res.Display(Window);
            } else {
                var reference = GetLoaderReference();
                reference.StartLoading("Processing...");
                var o = (AttendanceGroup) await MakeObject();
                try {
                    if (_renames.Any()) {
                        reference.SetMessage("Renaming students...");
                        var records =
                            (await MergeDatabase.ListAsync<AttendanceRecord>()).Where(r => r.GroupId == o.Id).ToList();
                        foreach (var t in _renames)
                        foreach (var r in records) {
                            if (!r.Students.Contains(t.Old)) continue;
                            r.Students[r.Students.IndexOf(t.Old)] = t.New;
                            await MergeDatabase.UpdateAsync(r);
                        }
                    }
                    reference.SetMessage("Processing...");
                    await MergeDatabase.UpdateAsync(o);
                    return true;
                } catch (Exception ex) {
                    MessageBox.Show(Window,
                        $"Could not update attendance/groups/{o.Id} ({o.GetType().FullName}):\n{ex.Message}\n{ex.GetType().FullName}");
                }
            }
            return false;
        }

        public override Task SaveAsDraft() {
            throw new NotImplementedException();
        }
    }
}
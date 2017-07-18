#region LICENSE

// Project Merge.Android:  AttendanceRecordEditorActivity.cs (in Solution Merge.Android)
// Created by Greg Whatley on 06/30/2017 at 9:20 AM.
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
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Merge.Android.Helpers;
using MergeApi.Client;
using MergeApi.Models.Core.Attendance;
using Newtonsoft.Json;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Toolbar = Android.Support.V7.Widget.Toolbar;

#endregion

namespace Merge.Android.UI.Activities.LeadersOnly {
    [Activity(Label = "Record Editor")]
    public class AttendanceRecordEditorActivity : AppCompatActivity, View.IOnClickListener {
        private Button _addStudentButton;
        private TextView _date, _groupId;
        private AttendanceGroup _group;
        private CheckBox _leadersPresent;
        private IMenu _menu;
        private AttendanceRecord _record;
        private LinearLayout _studentsList;

        public void OnClick(View v) {
            if (v.Id == _addStudentButton.Id) {
                var view = new EditText(this);
                var dialog = new AlertDialog.Builder(this).SetTitle("Add Student").SetCancelable(false)
                    .SetMessage("To add a new student, type in their name then tap Add.").SetPositiveButton("Add",
                        (s, e) => {
                            _studentsList.AddView(new CheckBox(this) {
                                Text = view.Text,
                                Checked = true
                            });
                        }).SetNegativeButton("Cancel", (s, e) => { }).SetView(view).Create();
                dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
                dialog.Show();
            }
        }

        public override bool OnPrepareOptionsMenu(IMenu menu) {
            menu.Add(0, 1, 0, "Save Changes").SetEnabled(!Intent.HasExtra("recordJson"))
                .SetShowAsAction(ShowAsAction.Always);
            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            switch (item.ItemId) {
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                case 1:
                    var dialog = new ProgressDialog(this) {
                        Indeterminate = true
                    };
                    dialog.SetTitle("Record Editor");
                    dialog.SetMessage("Saving changes...");
                    dialog.SetCancelable(false);
                    dialog.Show();
                    _record = new AttendanceRecord {
                        GroupId = _groupId.Text.Substring(0, 8),
                        Date = DateTime.Parse(_date.Text),
                        LeadersPresent = _leadersPresent.Checked,
                        Students = GetStudents().Where(IsStudentChecked).ToList()
                    };
                    foreach (var name in GetStudents().Where(n => !_group.StudentNames.Contains(n)))
                        if (IsStudentChecked(name))
                            _group.StudentNames.Add(name);
                    Task.Run(async () => {
                        await MergeDatabase.UpdateAsync(_group);
                        await MergeDatabase.UpdateAsync(_record);
                        dialog.Dismiss();
                        Finish();
                    });
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private List<string> GetStudents() => (from checkBox in _studentsList.GetChildren().OfType<CheckBox>()
            select checkBox.Text).ToList();

        private bool IsStudentChecked(string name) {
            return (from checkBox in _studentsList.GetChildren().OfType<CheckBox>()
                where checkBox.Text == name
                select checkBox.Checked).FirstOrDefault();
        }

        private void AddStudent(string name, bool check, bool enable) {
            _studentsList.AddView(new CheckBox(this) {
                Text = name,
                Checked = check,
                Enabled = enable
            });
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AttendanceRecordEditorActivity);
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            _addStudentButton = FindViewById<Button>(Resource.Id.addStudent);
            _addStudentButton.SetOnClickListener(this);
            _studentsList = FindViewById<LinearLayout>(Resource.Id.studentsList);
            _date = FindViewById<TextView>(Resource.Id.recordDate);
            _groupId = FindViewById<TextView>(Resource.Id.groupIdView);
            _leadersPresent = FindViewById<CheckBox>(Resource.Id.recordLeadersPresent);
            string recordJson;
            _group = JsonConvert.DeserializeObject<AttendanceGroup>(Intent.GetStringExtra("groupJson"));
            var checkedNames = new List<string>();
            var enable = true;
            if (!string.IsNullOrWhiteSpace(recordJson = Intent.GetStringExtra("recordJson"))) {
                enable = false;
                _addStudentButton.Enabled = false;
                _leadersPresent.Enabled = false;
                Title = "Edit Record";
                _record = JsonConvert.DeserializeObject<AttendanceRecord>(recordJson);
                _leadersPresent.Checked = _record.LeadersPresent;
                checkedNames = _record.Students;
                _groupId.Text = $"{_record.GroupId} ({_group.Summary})";
                _date.Text = _record.Date.ToLongDateString();
            } else {
                Title = "Add Record";
                _groupId.Text = $"{_group.Id} ({_group.Summary})";
                _date.Text = DateTime.Now.ToLongDateString();
            }
            foreach (var name in _group.StudentNames)
                AddStudent(name, checkedNames.Contains(name), enable);
            foreach (var name in checkedNames.Where(n => !_group.StudentNames.Contains(n)))
                AddStudent(name, true, enable);
        }
    }
}
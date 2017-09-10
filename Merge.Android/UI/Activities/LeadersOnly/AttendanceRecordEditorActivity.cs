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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Merge.Android.Helpers;
using MergeApi.Client;
using MergeApi.Models.Core.Attendance;
using Newtonsoft.Json;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Toolbar = Android.Support.V7.Widget.Toolbar;

#endregion

namespace Merge.Android.UI.Activities.LeadersOnly {
    [Activity(Label = "Record Editor", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public class AttendanceRecordEditorActivity : AppCompatActivity {
        [BindView(Resource.Id.addStudent)]
        private Button _addStudentButton;
        [BindView(Resource.Id.recordDate)]
        private TextView _date;
        [BindView(Resource.Id.groupIdView)]
        private TextView _groupId;
        [BindView(Resource.Id.recordLeadersPresent)]
        private CheckBox _leadersPresent;
        [BindView(Resource.Id.studentsList)]
        private LinearLayout _studentsList;

        private AttendanceGroup _group;
        private bool _enable = true;
        private AttendanceRecord _record;

        [OnClick(Resource.Id.addStudent)]
        public void AddStudent_OnClick(object sender, EventArgs args) {
            var view = new EditText(this);
            var dialog = new AlertDialog.Builder(this).SetTitle("Add Student").SetCancelable(false)
                .SetMessage("To add a new student, type in their name then tap Add.").SetPositiveButton("Add",
                    (s, e) => {
                        if (string.IsNullOrWhiteSpace(view.Text)) {
                            Toast.MakeText(this, "Could not add student: No name specified.", ToastLength.Long)
                                .Show();
                            return;
                        }
                        _studentsList.AddView(new CheckBox(this) {
                            Text = view.Text,
                            Checked = true
                        });
                    }).SetNegativeButton("Cancel", (s, e) => { }).SetView(view).Create();
            dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
            dialog.Show();
        }

        public override bool OnPrepareOptionsMenu(IMenu menu) {
            menu.Add(0, 1, 0, "Save Changes").SetEnabled(_enable)
                .SetShowAsAction(ShowAsAction.Always);
            return base.OnPrepareOptionsMenu(menu);
        }

        private void SaveAndExit() {
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
        }

        public override void OnBackPressed() {
            if (!_enable) {
                Finish();
                return;
            }
            var dialog = new AlertDialog.Builder(this).SetTitle("Save Changes").SetMessage("Do you want to save or discard your changes?").SetPositiveButton("Save",
                (s, e) => SaveAndExit()).SetNegativeButton("Discard", (s, e) => Finish()).SetNeutralButton("Cancel", (s, e) => { }).Create();
            dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
            dialog.Show();
        }
        
        public override bool OnOptionsItemSelected(IMenuItem item) {
            switch (item.ItemId) {
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                case 1:
                    SaveAndExit();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private List<string> GetStudents() => (from checkBox in _studentsList.GetChildren().OfType<CheckBox>()
                                               select checkBox.Text).ToList();

        private bool IsStudentChecked(string name) => (from checkBox in _studentsList.GetChildren().OfType<CheckBox>()
                                                       where checkBox.Text == name
                                                       select checkBox.Checked).FirstOrDefault();

        private void AddStudent(string name, bool check, bool enable) => _studentsList.AddView(new CheckBox(this) {
            Text = name,
            Checked = check,
            Enabled = enable
        });

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AttendanceRecordEditorActivity);
            Cheeseknife.Bind(this);
            if (SdkChecker.KitKat)
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            string recordJson;
            _group = JsonConvert.DeserializeObject<AttendanceGroup>(Intent.GetStringExtra("groupJson"));
            var checkedNames = new List<string>();
            if (!string.IsNullOrWhiteSpace(recordJson = Intent.GetStringExtra("recordJson"))) {
                _record = JsonConvert.DeserializeObject<AttendanceRecord>(recordJson);
                _enable = DateTime.Now.DayOfYear == _record.Date.DayOfYear;
                _addStudentButton.Enabled = _enable;
                _leadersPresent.Enabled = _enable;
                Title = $"{(_enable ? "Edit" : "View")} Record";
                _leadersPresent.Checked = _record.LeadersPresent;
                checkedNames = _record.Students;
                _groupId.Text = $"{_record.GroupId} ({_group.Summary})";
                _date.Text = _record.Date.ToLongDateString();
                LogHelper.FirebaseLog(this, "manageAttendanceRecord", new Dictionary<string, string> {
                        {"groupId", _group.Id},
                    {"recordDate", _record.DateString},
                    { "editable", _enable.ToString()}
                });
            } else {
                _leadersPresent.Checked = true;
                Title = "Add Record";
                _groupId.Text = $"{_group.Id} ({_group.Summary})";
                _date.Text = DateTime.Now.ToLongDateString();
                LogHelper.FirebaseLog(this, "manageAttendanceRecord", new Dictionary<string, string> {
                    {"groupId", _group.Id},
                    {"recordDate", "null"},
                    { "editable", _enable.ToString()}
                });
            }
            foreach (var name in _group.StudentNames)
                AddStudent(name, checkedNames.Contains(name), _enable);
            foreach (var name in checkedNames.Where(n => !_group.StudentNames.Contains(n)))
                AddStudent(name, true, _enable);
        }
    }
}
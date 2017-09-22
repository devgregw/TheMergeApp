#region LICENSE

// Project Merge.Android:  AttendanceGroupEditorActivity.cs (in Solution Merge.Android)
// Created by Greg Whatley on 09/01/2017 at 8:35 AM.
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
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
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
    [Activity(Label = "Edit Students", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public class AttendanceGroupEditorActivity : AppCompatActivity, View.IOnClickListener {
        private AttendanceGroup _group;
        private List<(string Old, string New)> _renames;
        private List<string> _students;

        [BindView(Resource.Id.studentsList)] private LinearLayout _studentsList;

        public void OnClick(View v) {
            switch (v.Id) {
                case 555:
                    if (_students.Count - 1 <= 0) {
                        var dialog = new AlertDialog.Builder(this).SetTitle("Error")
                            .SetMessage("This group must contain at least one student.").SetPositiveButton("OK",
                                (s, e) => { }).Create();
                        dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
                        dialog.Show();
                        return;
                    }
                    var name = ((ObjectWrapper<string>) v.Tag).Value;
                    _studentsList.RemoveViewAt(_students.IndexOf(name));
                    _students.Remove(name);
                    break;
                case 556:
                    var name2 = ((ObjectWrapper<string>) v.Tag).Value;
                    GetName(name2, n => {
                        if (_renames.Select(t => t.New).Contains(name2)) {
                            var index1 = _renames.Select(t => t.New).IndexOf(name2);
                            var previous = _renames[index1];
                            _renames[index1] = (previous.Old, n);
                        } else {
                            _renames.Add((name2, n));
                        }
                        var index2 = _students.IndexOf(name2);
                        _students[index2] = n;
                        ((TextView) ((ViewGroup) _studentsList.GetChildAt(index2)).GetChildAt(0)).Text = n;
                    });
                    break;
            }
        }

        private void GetName(string current, Action<string> callback) {
            var view = new EditText(this);
            var hasCurrentValue = !string.IsNullOrWhiteSpace(current);
            if (hasCurrentValue)
                view.Text = current;
            var nameDialog = new AlertDialog.Builder(this).SetTitle($"{(hasCurrentValue ? "Edit" : "Add")} Student")
                .SetCancelable(false)
                .SetMessage(hasCurrentValue
                    ? "Edit the student's name then tap Done."
                    : "To add a new student, type in their name then tap Done.").SetPositiveButton("Done",
                    (s, e) => { callback(view.Text); }).SetNegativeButton("Cancel", (s, e) => { })
                .SetView(view).Create();
            nameDialog.SetOnShowListener(AlertDialogColorOverride.Instance);
            nameDialog.Show();
        }

        [OnClick(Resource.Id.addStudent)]
        private void AddStudent_OnClick(object sender, EventArgs e) => GetName("", AddStudent);

        private void AddStudent(string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                Toast.MakeText(this, "Could not add student: No name specified.", ToastLength.Long).Show();
                return;
            }
            _students.Add(name);
            var layout = new LinearLayout(this) {
                Orientation = Orientation.Vertical
            };
            var buttons = new LinearLayout(this) {
                Orientation = Orientation.Horizontal
            };
            var textView = new TextView(this) {
                Text = name
            };
            textView.SetTextColor(Color.Black);
            textView.SetTextSize(ComplexUnitType.Sp, 18);
            var remove = new Button(this) {
                Id = 555,
                Tag = new ObjectWrapper<string>(name),
                Text = "Remove",
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                    ViewGroup.LayoutParams.WrapContent)
            };
            var rename = new Button(this) {
                Id = 556,
                Tag = new ObjectWrapper<string>(name),
                Text = "Edit",
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                    ViewGroup.LayoutParams.WrapContent)
            };
            remove.SetOnClickListener(this);
            rename.SetOnClickListener(this);
            layout.AddView(textView);
            buttons.AddView(remove);
            buttons.AddView(rename);
            layout.AddView(buttons);
            _studentsList.AddView(layout);
        }

        public override void OnBackPressed() {
            var dialog = new AlertDialog.Builder(this).SetTitle("Save Changes")
                .SetMessage("Do you want to save or discard your changes?").SetPositiveButton("Save",
                    (s, e) => SaveAndExit()).SetNegativeButton("Discard", (s, e) => Finish())
                .SetNeutralButton("Cancel", (s, e) => { }).Create();
            dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
            dialog.Show();
        }

        private void SaveAndExit() {
            if (_students.Count == 0) {
                var errDialog = new AlertDialog.Builder(this).SetTitle("No Students")
                    .SetMessage("This group must contain at least one student.").SetPositiveButton("Ok",
                        (s, e) => { }).Create();
                errDialog.SetOnShowListener(AlertDialogColorOverride.Instance);
                errDialog.Show();
                return;
            }
            var dialog = new ProgressDialog(this) {
                Indeterminate = true
            };
            dialog.SetTitle("Group Editor");
            dialog.SetMessage("Saving changes...");
            dialog.SetCancelable(false);
            dialog.Show();
            _group.StudentNames = _students;
            Task.Run(async () => {
                if (_renames.Any()) {
                    var records =
                        (await MergeDatabase.ListAsync<AttendanceRecord>()).Where(r => r.GroupId == _group.Id).ToList();
                    foreach (var t in _renames)
                    foreach (var r in records) {
                        if (!r.Students.Contains(t.Old)) continue;
                        r.Students[r.Students.IndexOf(t.Old)] = t.New;
                        await MergeDatabase.UpdateAsync(r);
                    }
                }
                await MergeDatabase.UpdateAsync(_group);
                dialog.Dismiss();
                Finish();
            });
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            switch (item.ItemId) {
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                case 12345:
                    SaveAndExit();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu) {
            menu.Add(0, 12345, 1, "Save Changes").SetShowAsAction(ShowAsAction.Always);
            return base.OnPrepareOptionsMenu(menu);
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AttendanceGroupEditorActivity);
            Cheeseknife.Bind(this);
            if (SdkChecker.KitKat)
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            _group = JsonConvert.DeserializeObject<AttendanceGroup>(Intent.GetStringExtra("groupJson"));
            LogHelper.FirebaseLog(this, "manageAttendanceGroup", new Dictionary<string, string> {
                {"groupId", _group.Id}
            });
            FindViewById<TextView>(Resource.Id.groupIdView).Text = $"{_group.Id} ({_group.Summary})";
            _students = new List<string>();
            _renames = new List<(string Old, string New)>();
            _group.StudentNames.ForEach(AddStudent);
        }
    }
}
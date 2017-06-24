using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Merge.Android.Classes.Helpers;
using MergeApi.Client;
using MergeApi.Models.Core.Attendance;
using Newtonsoft.Json;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using System.Threading.Tasks;

namespace Merge.Android {
    [Activity(Label = "Record Editor")]
    public class RecordEditorActivity : AppCompatActivity, View.IOnClickListener {
        private Button _addStudentButton;
        private LinearLayout _studentsList;
        private TextView _date, _groupId;
        private AttendanceGroup _group;
        private AttendanceRecord _record;
        private CheckBox _leadersPresent;
        
        public override bool OnPrepareOptionsMenu(IMenu menu) {
            menu.Add(0, 1, 0, "Save Changes").SetShowAsAction(ShowAsAction.Always);
            return true;
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
            return (from checkBox in _studentsList.GetChildren().OfType<CheckBox>() where checkBox.Text == name select checkBox.Checked).FirstOrDefault();
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.RecordEditorActivity);
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            _addStudentButton = FindViewById<Button>(Resource.Id.recordAddStudent);
            _addStudentButton.SetOnClickListener(this);
            _studentsList = FindViewById<LinearLayout>(Resource.Id.recordStudents);
            _date = FindViewById<TextView>(Resource.Id.recordDate);
            _groupId = FindViewById<TextView>(Resource.Id.recordGroupId);
            _leadersPresent = FindViewById<CheckBox>(Resource.Id.recordLeadersPresent);
            string recordJson;
            _group = JsonConvert.DeserializeObject<AttendanceGroup>(Intent.GetStringExtra("groupJson"));
            var checkedNames = new List<string>();
            if (!string.IsNullOrWhiteSpace(recordJson = Intent.GetStringExtra("recordJson"))) {
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
                _studentsList.AddView(new CheckBox(this) {
                    Text = name,
                    Checked = checkedNames.Contains(name)
                });
        }

        public void OnClick(View v) {
            if (v.Id == _addStudentButton.Id) {
                var view = new EditText(this);
                var dialog = new global::Android.Support.V7.App.AlertDialog.Builder(this).SetTitle("Add Student").SetCancelable(false).SetMessage("To add a new student, type in their name then tap Add.").SetPositiveButton("Add",
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
    }
}
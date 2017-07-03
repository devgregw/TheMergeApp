
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Merge.Android.Helpers;
using MergeApi.Client;
using MergeApi.Models.Core.Attendance;
using Newtonsoft.Json;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Orientation = Android.Widget.Orientation;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Merge.Android.UI.Activities.LeadersOnly {
    [Activity(Label = "Edit Students")]
    public class AttendanceGroupEditorActivity : AppCompatActivity, View.IOnClickListener {
        private LinearLayout _studentsList;
        private AttendanceGroup _group;

        private List<string> _students;

        private void AddStudent(string name) {
            _students.Add(name);
            var layout = new LinearLayout(this) {
                Orientation = Orientation.Vertical,
                Tag = new ObjectWrapper<string>(name)
            };
            var textView = new TextView(this) {
                Text = name
            };
            textView.SetTextColor(Color.Black);
            textView.SetTextSize(ComplexUnitType.Sp, 18);
            var button = new Button(this) {
                Id = 555,
                Tag = new ObjectWrapper<string>($"button:{name}"),
                Text = "Remove",
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            };
            button.SetOnClickListener(this);
            layout.AddView(textView);
            layout.AddView(button);
            _studentsList.AddView(layout);
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            switch (item.ItemId) {
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                case 12345:
                    var dialog = new ProgressDialog(this) {
                        Indeterminate = true
                    };
                    dialog.SetTitle("Group Editor");
                    dialog.SetMessage("Saving changes...");
                    dialog.SetCancelable(false);
                    dialog.Show();
                    _group.StudentNames = _students;
                    Task.Run(async () => {
                        await MergeDatabase.UpdateAsync(_group);
                        dialog.Dismiss();
                        Finish();
                    });
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
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            _studentsList = FindViewById<LinearLayout>(Resource.Id.studentsList);
            FindViewById<Button>(Resource.Id.addStudent).SetOnClickListener(this);
            _group = JsonConvert.DeserializeObject<AttendanceGroup>(Intent.GetStringExtra("groupJson"));
            FindViewById<TextView>(Resource.Id.groupIdView).Text = $"{_group.Id} ({_group.Summary})";
            _students = new List<string>();
            _group.StudentNames.ForEach(AddStudent);
        }

        public void OnClick(View v) {
            switch (v.Id) {
                case Resource.Id.addStudent:
                    var view = new EditText(this);
                    var nameDialog = new AlertDialog.Builder(this).SetTitle("Add Student").SetCancelable(false)
                        .SetMessage("To add a new student, type in their name then tap Add.").SetPositiveButton("Add",
                            (s, e) => {
                                AddStudent(view.Text);
                            }).SetNegativeButton("Cancel", (s, e) => { }).SetView(view).Create();
                    nameDialog.SetOnShowListener(AlertDialogColorOverride.Instance);
                    nameDialog.Show();
                    break;
                case 555:
                    var name = ((ObjectWrapper<string>)v.Tag).Value.Replace("button:", "");
                    _studentsList.RemoveViewAt(_students.IndexOf(name));
                    _students.Remove(name);
                    break;
            }
        }
    }
}
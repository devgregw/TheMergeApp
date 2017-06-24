using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Maps;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Merge.Android.Classes.Controls;
using Merge.Android.Classes.Helpers;
using MergeApi.Client;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Core;
using MergeApi.Models.Core.Attendance;
using Newtonsoft.Json;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Fragment = Android.Support.V4.App.Fragment;
using ListFragment = Android.Support.V4.App.ListFragment;
using Object = Java.Lang.Object;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Merge.Android {
    [Activity(Label = "Attendance Manager", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class AttendanceManagerActivity : AppCompatActivity {
        /*public class AttendanceListFragment : ListFragment {
            private Func<Task<Dictionary<string, string>>> _itemSource;
            private Action<string, Dictionary<string, object>> _onClick;
            private Context _context;
            private Dictionary<string, string> _items;
            
            public Dictionary<string, object> Extras { get; set; }

            public AttendanceListFragment(Context c, Func<Task<Dictionary<string, string>>> itemSource, Action<string, Dictionary<string, object>> onClick) {
                Extras = new Dictionary<string, object>();
                _itemSource = itemSource;
                _onClick = onClick;
                _context = c;
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
                new Action(async () => {
                    (ListAdapter as ArrayAdapter<Java.Lang.String>)?.Clear();
                    try {
                        _items = await _itemSource();
                    } catch (Exception e) {
                        _items = new Dictionary<string, string>();
                        var dialog = new AlertDialog.Builder(_context).SetCancelable(false).SetTitle("Error")
                            .SetMessage($"An error occurred while loading content.\n{BasicCard.MakeExceptionString(e)}")
                            .SetPositiveButton("Close", (s, args) => ((Activity) _context).Finish()).Create();
                        dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
                        dialog.Show();
                    } finally {
                        ListAdapter = new ArrayAdapter<Java.Lang.String>(_context, global::Android.Resource.Layout.SimpleListItem1,
                            _items.Keys.Select(s => new Java.Lang.String(s)).ToList());
                    }
                }).Invoke();
                return base.OnCreateView(inflater, container, savedInstanceState);
            }

            public override void OnListItemClick(ListView l, View v, int position, long id) {
                _onClick(_items.ElementAt(position).Value, Extras);
            }
        }*/

        public sealed class AttendanceListFragment : ListFragment {
            private const int StateMain = 0, StateGroups = 1, StateRecords = 2;
            private Context _context;
            private int _state;
            private List<AttendanceGroup> _groups;
            private List<AttendanceRecord> _records;
            private Dictionary<string, string> _items;
            private Dictionary<int, object> _arguments;

            public AttendanceListFragment(Context c) {
                _context = c;
                _groups = new List<AttendanceGroup>();
                _records = new List<AttendanceRecord>();
                _items = new Dictionary<string, string>();
                _arguments = new Dictionary<int, object>();
            }

            private void SetItems(Dictionary<string, string> items) {
                (ListAdapter as ArrayAdapter<Java.Lang.String>)?.Clear();
                _items = items;
                ListAdapter = new ArrayAdapter<string>(_context, global::Android.Resource.Layout.SimpleListItem1, _items.Keys.ToList());
            }

            private void SetState(int state, bool load, object argument) {
                new Action(async () => {
                    _state = state;
                    _arguments[_state] = argument;
                    (ListAdapter as ArrayAdapter<Java.Lang.String>)?.Clear();
                    if (load) {
                        var dialog = new ProgressDialog(_context) {
                            Indeterminate = true
                        };
                        dialog.SetTitle("Attendance Manager");
                        dialog.SetMessage("Loading...");
                        dialog.SetCancelable(false);
                        dialog.Show();
                        _groups = (await MergeDatabase.ListAsync<AttendanceGroup>()).ToList();
                        _records = (await MergeDatabase.ListAsync<AttendanceRecord>()).ToList();
                        dialog.Dismiss();
                    }
                    switch (_state) {
                        case StateMain:
                            SetItems(new Dictionary<string, string> {
                                { "Junior High", "jh" },
                                { "High School", "hs" }
                            });
                            break;
                        case StateGroups:
                            SetItems(_groups.Where(g => argument.ToString() == "jh" ? (int)g.GradeLevel <= 8 : (int)g.GradeLevel >= 9).ToDictionary(g => g.Summary, JsonConvert.SerializeObject));
                            break;
                        case StateRecords:
                            var sorted = _records.Where(r => r.GroupId == argument.ToString()).ToList();
                            sorted.Sort((x, y) => DateTime.Compare(y.Date, x.Date));
                            SetItems(new Dictionary<string, string> { { "Add Record", $"add:{argument}" } }.Concat(sorted.ToDictionary(g => g.Date.ToLongDateString(), JsonConvert.SerializeObject)).ToDictionary(p => p.Key, p => p.Value));
                            break;
                    }
                }).Invoke();
            }

            public bool GoBack() {
                if (_state == StateMain) return false;
                SetState(_state - 1, false, _arguments[_state - 1]);
                return true;
            }

            public override void OnResume() {
                base.OnResume();
                SetState(_state, true, _arguments[_state]);
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
                SetState(StateMain, true, null);
                return base.OnCreateView(inflater, container, savedInstanceState);
            }

            public override void OnListItemClick(ListView l, View v, int position, long id) {
                var data = _items.ElementAt(position).Value;
                switch (_state) {
                    case StateMain:
                        SetState(StateGroups, false, data);
                        break;
                    case StateGroups:
                        var group = JsonConvert.DeserializeObject<AttendanceGroup>(data);
                        SetState(StateRecords, false, group.Id);
                        break;
                    case StateRecords:
                        var intent = new Intent(_context, typeof(RecordEditorActivity));
                        if (!data.StartsWith("add:"))
                            intent.PutExtra("recordJson", data);
                        intent.PutExtra("groupJson", JsonConvert.SerializeObject(_groups.First(g => g.Id == _arguments[StateRecords].ToString().Replace("add:", ""))));
                        _context.StartActivity(intent);
                        break;
                }
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            if (item.ItemId != global::Android.Resource.Id.Home)
                return base.OnOptionsItemSelected(item);
            OnBackPressed();
            return true;
        }

        private AttendanceListFragment _fragment;
        private bool _done;

        public override void OnBackPressed() {
            /*if (SupportFragmentManager.BackStackEntryCount == 1)
                Finish();
            else
                SupportFragmentManager.PopBackStack();*/
            if (!_fragment.GoBack())
                Finish();
        }

        /*private void CreateRecordListFragment(Func<AttendanceRecord, bool> predicate, AttendanceGroup commonGroup) {
            var fragment = new AttendanceListFragment(this,
                async () => {
                    var d = CreateProgressDialog();
                    d.Show();
                    var records = (await MergeDatabase.ListAsync<AttendanceRecord>()).Where(predicate);
                    d.Dismiss();
                    return new Dictionary<string, string> {{"Add Record", "add"}}
                        .Concat(records.ToDictionary(r => r.Date.ToLongDateString(), JsonConvert.SerializeObject))
                        .ToDictionary(p => p.Key, p => p.Value);
                },
                (data, extras) => {
                    var i = new Intent(this, typeof(RecordEditorActivity));
                    i.PutExtra("groupJson", JsonConvert.SerializeObject(extras["group"]));
                    if (data != "add")
                        i.PutExtra("recordJson", data);
                    StartActivity(i);
                });
            fragment.Extras.Add("group", commonGroup);
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.container, fragment)
                .AddToBackStack("records").Commit();
        }

        private void CreateGroupListFragment(Func<AttendanceGroup, bool> predicate) {
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.container,
                new AttendanceListFragment(this, async () => {
                    var d = CreateProgressDialog();
                    d.Show();
                    var groups = (await MergeDatabase.ListAsync<AttendanceGroup>()).Where(predicate);
                    d.Dismiss();
                    return groups.ToDictionary(g => g.Summary, JsonConvert.SerializeObject);
                },
                    (data, extras) => {
                        var g = JsonConvert.DeserializeObject<AttendanceGroup>(data);
                        CreateRecordListFragment(r => r.GroupId == g.Id, g);
                    })).AddToBackStack("groups").Commit();
        }

        private void CreateMainFragment() {
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.container, new AttendanceListFragment(this, () => Task.FromResult(new Dictionary<string, string> {
                { "Junior High", "jh" },
                { "High School", "hs" }
            }), (data, extras) => CreateGroupListFragment(g => data == "jh" ? (int)g.GradeLevel <= (int)GradeLevel.Eighth : (int)g.GradeLevel >= (int)GradeLevel.Ninth))).AddToBackStack("main").Commit();
        }*/

        private ProgressDialog CreateProgressDialog() {
            var d = new ProgressDialog(this) {
                Indeterminate = true
            };
            d.SetMessage("Loading...");
            d.SetTitle("Attendance Manager");
            d.SetCancelable(false);
            return d;
        }

        protected override void OnResumeFragments() {
            base.OnResumeFragments();
            if (_done)
                _fragment.OnResume();
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AttendanceManagerActivity);
            if (SdkChecker.KitKat)
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            _fragment = new AttendanceListFragment(this);
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.container, _fragment).Commit();
            _done = true;
        }
    }
}
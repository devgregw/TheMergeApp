#region LICENSE

// Project Merge.Android:  AttendanceListFragment.cs (in Solution Merge.Android)
// Created by Greg Whatley on 06/30/2017 at 9:21 AM.
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
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Merge.Android.UI.Activities.LeadersOnly;
using MergeApi.Client;
using MergeApi.Models.Core;
using MergeApi.Models.Core.Attendance;
using Newtonsoft.Json;
using ListFragment = Android.Support.V4.App.ListFragment;
using String = Java.Lang.String;

#endregion

namespace Merge.Android.UI.Fragments {
    public sealed class AttendanceListFragment : ListFragment {
        private const int StateMain = 0, StateGroups = 1, StateRecords = 2;
        private readonly Dictionary<int, object> _arguments;
        private readonly Context _context;
        private List<AttendanceGroup> _groups;
        private Dictionary<string, string> _items;
        private List<MergeGroupAttendanceRecord> _mergeGroupRecords;
        private List<MergeGroup> _mergeGroups;
        private List<AttendanceRecord> _records;
        private int _state;

        public AttendanceListFragment(Context c) {
            _context = c;
            _groups = new List<AttendanceGroup>();
            _records = new List<AttendanceRecord>();
            _mergeGroups = new List<MergeGroup>();
            _mergeGroupRecords = new List<MergeGroupAttendanceRecord>();
            _items = new Dictionary<string, string>();
            _arguments = new Dictionary<int, object>();
        }

        public AttendanceGroup SelectedGroup { get; private set; }
        public MergeGroup SelectedMergeGroup { get; private set; }

        private void SetItems(Dictionary<string, string> items) {
            (ListAdapter as ArrayAdapter<String>)?.Clear();
            _items = items;
            ListAdapter = new ArrayAdapter<string>(_context, global::Android.Resource.Layout.SimpleListItem1,
                _items.Keys.ToList());
        }

        private void SetState(int state, bool load, object argument) => new Action(async () => {
            _state = state;
            _arguments[_state] = argument;
            (ListAdapter as ArrayAdapter<String>)?.Clear();
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
                _mergeGroups = (await MergeDatabase.ListAsync<MergeGroup>()).ToList();
                _mergeGroupRecords = (await MergeDatabase.ListAsync<MergeGroupAttendanceRecord>()).ToList();
                dialog.Dismiss();
            }
            switch (_state) {
                case StateMain:
                    SelectedGroup = null;
                    SetItems(new Dictionary<string, string> {
                        {"Junior High", "jh"},
                        {"High School", "hs"},
                        {"Merge Groups", "mg"}
                    });
                    break;
                case StateGroups:
                    var ministry = argument.ToString();
                    if (ministry == "mg") {
                        SelectedMergeGroup = null;
                        SetItems(_mergeGroups.OrderBy(g => g.Id)
                            .ToDictionary(g => g.Name, g => $"mg:{JsonConvert.SerializeObject(g)}"));
                        return;
                    }
                    SelectedGroup = null;
                    SetItems(_groups
                        .Where(g => argument.ToString() == "jh" ? (int) g.GradeLevel <= 8 : (int) g.GradeLevel >= 9)
                        .OrderBy(g => (int) g.GradeLevel).ThenBy(g =>
                            IntFromWord(g.LeaderNames.ElementAt(0)
                                .Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries).Last()))
                        .ToDictionary(g => g.Summary, g => $"ag:{JsonConvert.SerializeObject(g)}"));
                    break;
                case StateRecords:
                    Console.WriteLine(argument.ToString());
                    var isMergeGroup = argument.ToString().StartsWith("mg:");
                    var id = argument.ToString().Length == 11 ? argument.ToString().Remove(0, 3) : argument.ToString();
                    if (isMergeGroup) {
                        SelectedMergeGroup = _mergeGroups.First(g => g.Id == id);
                        SetItems(new Dictionary<string, string> {
                                {"Add Record", $"add:{id}"}
                            }.Concat(_mergeGroupRecords.Where(r => r.MergeGroupId == id).OrderByDescending(r => r.Date)
                                .ToDictionary(r => r.Date.ToLongDateString(), JsonConvert.SerializeObject))
                            .ToDictionary(r => r.Key, r => r.Value));
                        return;
                    }
                    SelectedGroup = _groups.First(g => g.Id == id);
                    var sorted = _records.Where(r => r.GroupId == id).OrderByDescending(r => r.Date).ToList();
                    SetItems(new Dictionary<string, string> {
                            {"Edit Group", "edit"},
                            {"Add Record", $"add:{id}"}
                        }
                        .Concat(sorted.ToDictionary(g => g.Date.ToLongDateString(), JsonConvert.SerializeObject))
                        .ToDictionary(p => p.Key, p => p.Value));
                    break;
            }
        }).Invoke();

        public bool GoBack() {
            if (_state == StateMain) return false;
            SetState(_state - 1, false, _arguments[_state - 1]);
            return true;
        }

        public override void OnResume() {
            base.OnResume();
            SetState(_state, true, _arguments.TryGetValue(_state, out var o) ? o : null);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
            SetState(StateMain, true, null);
            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        private int IntFromWord(string w) {
            switch (w.ToLower()) {
                case "one":
                    return 1;
                case "two":
                    return 2;
                case "three":
                    return 3;
                case "four":
                    return 4;
                case "five":
                    return 5;
            }
            return -1;
        }

        public override void OnListItemClick(ListView l, View v, int position, long id) {
            var data = _items.ElementAt(position).Value;
            switch (_state) {
                case StateMain:
                    SetState(StateGroups, false, data);
                    break;
                case StateGroups:
                    string newData = data.Remove(0, 3),
                        gid = data.StartsWith("ag:")
                            ? $"ag:{JsonConvert.DeserializeObject<AttendanceGroup>(newData).Id}"
                            : $"mg:{JsonConvert.DeserializeObject<MergeGroup>(newData).Id}";
                    SetState(StateRecords, false, gid);
                    break;
                case StateRecords:
                    if (SelectedMergeGroup != null) {
                        var mgIntent = new Intent(_context, typeof(MergeGroupRecordEditorActivity));
                        if (!data.StartsWith("add:"))
                            mgIntent.PutExtra("recordJson", data);
                        mgIntent.PutExtra("groupJson",
                            JsonConvert.SerializeObject(
                                _mergeGroups.First(
                                    g => g.Id == _arguments[StateRecords].ToString().Replace("mg:", ""))));
                        _context.StartActivity(mgIntent);
                        return;
                    }
                    if (data == "edit") {
                        var editorIntent = new Intent(_context, typeof(AttendanceGroupEditorActivity));
                        editorIntent.PutExtra("groupJson", JsonConvert.SerializeObject(SelectedGroup));
                        _context.StartActivity(editorIntent);
                        break;
                    }
                    var intent = new Intent(_context, typeof(AttendanceRecordEditorActivity));
                    if (!data.StartsWith("add:"))
                        intent.PutExtra("recordJson", data);
                    intent.PutExtra("groupJson",
                        JsonConvert.SerializeObject(
                            _groups.First(g => g.Id == _arguments[StateRecords].ToString().Replace("ag:", ""))));
                    _context.StartActivity(intent);
                    break;
            }
        }
    }
}
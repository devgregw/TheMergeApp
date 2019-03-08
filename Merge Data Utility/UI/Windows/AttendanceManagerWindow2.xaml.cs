#region LICENSE

// Project Merge Data Utility:  AttendanceManagerWindow2.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 08/22/2017 at 3:57 PM.
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
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MergeApi;
using MergeApi.Models.Core;
using MergeApi.Models.Core.Attendance;
using MergeApi.Tools;
using Merge_Data_Utility.Tools;
using static Merge_Data_Utility.Tools.AttendanceTools;

#endregion

namespace Merge_Data_Utility.UI.Windows {
    /// <summary>
    ///     Interaction logic for AttendanceManagerWindow2.xaml
    /// </summary>
    public partial class AttendanceManagerWindow2 : Window {
        private bool _groupByDate = true;
        private List<AttendanceGroup> _groups;
        private List<MergeGroupAttendanceRecord> _mergeGroupRecords;
        private List<MergeGroup> _mergeGroups;
        private List<AttendanceRecord> _records;

        public AttendanceManagerWindow2() {
            InitializeComponent();
            Loaded += (s, e) => Load(true);
        }

        private void SetActions(Dictionary<string, Action> actions) {
            var content = new StackPanel {
                Margin = new Thickness(5)
            };
            foreach (var a in actions) {
                var button = new Button {
                    Content = a.Key,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                button.Click += (s, e) => a.Value();
                content.Children.Add(button);
            }
            __statPanel.Children.Add(new GroupBox {
                Header = "Actions",
                Content = content
            });
        }

        private void ClearMetricsAndActions() => __statPanel.Children.Clear();

        private void AddMetric(Tag t) {
            if (t == null)
                return;
            switch (t.Type) {
                case "week":
                    var week = t.GetArgument<Week>();
                    var weekMetrics = week.GetMetrics(_groups);
                    AddMetric($"{week.Date.ToLongDateString()} Statistics", new Dictionary<string, string> {
                        {
                            "Attendance",
                            $"{weekMetrics.TotalStudents} students ({weekMetrics.AverageAttendancePercentage}%)"
                        },
                        {"Leader Attendance", $"{weekMetrics.AverageLeaderAttendancePercentage}%"},
                        {"---", ""}, {
                            "Highest Attendance Group",
                            weekMetrics.HighestAttendanceGroup == null
                                ? "No data"
                                : $"{weekMetrics.HighestAttendanceGroup.Summary} ({weekMetrics.HighestAttendanceGroup.GetMetrics(_records).AverageAttendancePercentage}%)"
                        }, {
                            "Lowest Attendance Group",
                            weekMetrics.LowestAttendanceGroup == null
                                ? "No data"
                                : $"{weekMetrics.LowestAttendanceGroup.Summary} ({weekMetrics.LowestAttendanceGroup.GetMetrics(_records).AverageAttendancePercentage}%)"
                        }
                    });
                    break;
                case "group":
                    var group = t.GetArgument<AttendanceGroup>();
                    var groupMetrics = group.GetMetrics(_records);
                    Tuple<string, int> ls = groupMetrics.LowestStudentAttendancePercentage,
                        hs = groupMetrics.HighestStudentAttendancePercentage;
                    AttendanceWeekMetrics hwm = groupMetrics.HighestAttendanceRecord == null
                            ? null
                            : new AttendanceWeekMetrics(new Week(new List<AttendanceRecord> {
                                groupMetrics.HighestAttendanceRecord
                            }), new List<AttendanceGroup> {
                                group
                            }),
                        lwm = groupMetrics.LowestAttendanceRecord == null
                            ? null
                            : new AttendanceWeekMetrics(new Week(new List<AttendanceRecord> {
                                groupMetrics.LowestAttendanceRecord
                            }), new List<AttendanceGroup> {
                                group
                            });
                    AddMetric($"{group.Summary} Statistics", new Dictionary<string, string> {
                        {
                            "Attendance",
                            $"{groupMetrics.AverageStudentCount} students ({groupMetrics.AverageAttendancePercentage}%)"
                        },
                        {"Leader Attendance", $"{groupMetrics.AverageLeaderAttendancePercentage}%"},
                        {"---1", ""}, {
                            "Highest Attendance Week",
                            hwm == null
                                ? "No data"
                                : $"{hwm.TotalStudents} students ({hwm.AverageAttendancePercentage}%) on {hwm.Week.Date.ToLongDateString()}"
                        }, {
                            "Lowest Attendance Week",
                            lwm == null
                                ? "No data"
                                : $"{lwm.TotalStudents} students ({lwm.AverageAttendancePercentage}%) on {lwm.Week.Date.ToLongDateString()}"
                        },
                        {"---2", ""},
                        {"Highest Attendance Student", hs == null ? "No data" : $"{hs.Item1} ({hs.Item2}%)"},
                        {"Lowest Attendance Student", ls == null ? "No data" : $"{ls.Item1} ({ls.Item2}%)"}
                    });
                    break;
                case "ministry":
                    var arg = t.GetArgument<dynamic>();
                    var groups = _groups.Where(g => arg.Ministry == "jh"
                        ? (int) g.GradeLevel <= 8
                        : (int) g.GradeLevel >= 9).ToList();
                    var groupIds = groups.Select(g => g.Id);
                    if (!groups.Any())
                        break;
                    var m = GetMetrics(_groupByDate
                            ? _records
                                .Where(r => r.Date.ToLongDateString() ==
                                            ((Week) arg.Week).Date.ToLongDateString() &&
                                            groupIds.Contains(r.GroupId)).ToList()
                            : _records.Where(r => groupIds.Contains(r.GroupId)).ToList(), groups,
                        new List<MergeGroup>(),
                        new List<MergeGroupAttendanceRecord>());
                    AddMetric($"{(arg.Ministry == "jh" ? "Junior High" : "High School")} Statistics",
                        new Dictionary<string, string> {
                            {
                                "Attendance",
                                $"{m.AverageStudentCount} students ({m.AverageAttendancePercentage}%)"
                            },
                            {"Leader Attendance", $"{m.AverageLeaderAttendancePercentage}%"},
                            {"---", ""}, {
                                "Highest Attendance Group",
                                m.HighestAttendanceGroup == null
                                    ? "No data"
                                    : $"{m.HighestAttendanceGroup.Summary} ({m.HighestAttendanceGroup.GetMetrics(_records).AverageAttendancePercentage}%)"
                            }, {
                                "Lowest Attendance Group",
                                m.LowestAttendanceGroup == null
                                    ? "No data"
                                    : $"{m.LowestAttendanceGroup.Summary} ({m.LowestAttendanceGroup.GetMetrics(_records).AverageAttendancePercentage}%)"
                            }
                        });
                    break;
                case "record":
                    var record = t.GetArgument<AttendanceRecord>();
                    var group2 = GetGroup(record.GroupId);
                    var recordMetrics = record.GetMetrics(group2);
                    AddMetric($"{group2.Summary} on {record.Date.ToLongDateString()} Statistics",
                        new Dictionary<string, string> {
                            {"Attendance", $"{record.Students.Count} students ({recordMetrics.AttendancePercentage}%)"},
                            {"Leaders Present", record.LeadersPresent ? "Yes" : "No"}
                        });
                    break;
                case "student":
                    var arg2 = t.GetArgument<dynamic>();
                    AddMetric($"{arg2.Name} Statistics", new Dictionary<string, string> {
                        {
                            "Average Attendance",
                            $"{((AttendanceGroup) arg2.Group).GetMetrics(_records).GetStudentAttendancePercentage(arg2.Name)}%"
                        }
                    });
                    break;
                case "mgweek":
                    var mgWeekMetrics = t.GetArgument<MergeGroupWeek>().GetMetrics(_mergeGroups);
                    AddMetric($"{mgWeekMetrics.MergeGroupWeek.Date.ToLongDateString()} Merge Group Statistics",
                        new Dictionary<string, string> {
                            {"Attendance", $"{mgWeekMetrics.StudentCount} students"},
                            {"---", ""}, {
                                "Highest Attendance Group",
                                mgWeekMetrics.HighestAttendanceMergeGroup == null
                                    ? "No data"
                                    : $"{mgWeekMetrics.HighestAttendanceMergeGroup.Name} ({mgWeekMetrics.HighestAttendanceMergeGroup.GetMetrics(_mergeGroupRecords).AverageStudentCount})"
                            }, {
                                "Lowest Attendance Group",
                                mgWeekMetrics.LowestAttendanceMergeGroup == null
                                    ? "No data"
                                    : $"{mgWeekMetrics.LowestAttendanceMergeGroup.Name} ({mgWeekMetrics.LowestAttendanceMergeGroup.GetMetrics(_mergeGroupRecords).AverageStudentCount})"
                            }
                        });
                    break;
                case "mggroup":
                    var mgGroupMetrics = t.GetArgument<MergeGroup>().GetMetrics(_mergeGroupRecords);
                    AddMetric($"{mgGroupMetrics.MergeGroup.Name} Merge Group Statistics",
                        new Dictionary<string, string> {
                            {"Average Attendance", $"{mgGroupMetrics.AverageStudentCount} students"},
                            {"---", ""}, {
                                "Highest Attendance Record",
                                mgGroupMetrics.HighestAttendanceRecord == null
                                    ? "No data"
                                    : $"{mgGroupMetrics.HighestAttendanceRecord.StudentCount} students on {mgGroupMetrics.HighestAttendanceRecord.Date.ToLongDateString()}"
                            }, {
                                "Lowest Attendance Record",
                                mgGroupMetrics.LowestAttendanceRecord == null
                                    ? "No data"
                                    : $"{mgGroupMetrics.LowestAttendanceRecord.StudentCount} students on {mgGroupMetrics.LowestAttendanceRecord.Date.ToLongDateString()}"
                            }, {
                                "Most Recent Attendance Record",
                                mgGroupMetrics.MostRecentAttendanceRecord == null
                                    ? "No data"
                                    : $"{mgGroupMetrics.MostRecentAttendanceRecord.StudentCount} students on {mgGroupMetrics.MostRecentAttendanceRecord.Date.ToLongDateString()}"
                            }
                        });
                    break;
                case "mgrecord":
                    var mgRecord = t.GetArgument<MergeGroupAttendanceRecord>();
                    var mgGroup = _mergeGroups.First(g => g.Id == mgRecord.MergeGroupId);
                    AddMetric($"{mgGroup.Name} on {mgRecord.Date.ToLongDateString()} Merge Group Statistics",
                        new Dictionary<string, string> {
                            {"Student Count", $"{mgRecord.StudentCount} students"},
                            {"Image", $"mgimage://{mgRecord.Image}"}
                        });
                    break;
            }
        }

        private void AddMetric(string name, Dictionary<string, string> values) {
            var grid = new Grid {
                ColumnDefinitions = {
                    new ColumnDefinition {
                        Width = new GridLength(0.33d, GridUnitType.Star)
                    },
                    new ColumnDefinition {
                        Width = new GridLength(0.66d, GridUnitType.Star)
                    }
                }
            };
            foreach (var pair in values) {
                var i = values.IndexOf(pair);
                grid.RowDefinitions.Add(new RowDefinition {
                    Height = GridLength.Auto
                });
                var nameBlock = new TextBlock {
                    Text = pair.Key.StartsWith("---") ? "" : pair.Key,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center
                };
                nameBlock.SetValue(Grid.ColumnProperty, 0);
                nameBlock.SetValue(Grid.RowProperty, i);
                UIElement valueBlock;
                if (pair.Value.StartsWith("mgimage://"))
                    if (pair.Value.Replace("mgimage://", "") == "")
                        valueBlock = new TextBlock {
                            Text = "No image",
                            TextWrapping = TextWrapping.Wrap,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                    else
                        valueBlock = new Button {
                            Content = "View",
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Center
                        }.Manipulate(b => {
                            b.Click += (s, e) => Process.Start(pair.Value.Replace("mgimage://", ""));
                            return b;
                        });
                else
                    valueBlock = new TextBlock {
                        Text = pair.Value,
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                valueBlock.SetValue(Grid.ColumnProperty, 1);
                valueBlock.SetValue(Grid.RowProperty, i);
                grid.Children.Add(nameBlock);
                grid.Children.Add(valueBlock);
            }
            __statPanel.Children.Add(new GroupBox {
                Header = name,
                Content = grid
            });
        }

        private AttendanceGroup GetGroup(string id) {
            try {
                return _groups.First(g => g.Id == id);
            } catch (Exception e) {
                throw new ArgumentException($"Could not find group by ID {id}.");
            }
        }

        private async void Load(bool fetch) {
            if (fetch) {
                var reference = new LoaderReference(this);
                reference.StartLoading("Loading attendance...");
                _records = (await MergeDatabase.ListAsync<AttendanceRecord>()).ToList();
                _groups = (await MergeDatabase.ListAsync<AttendanceGroup>()).ToList();
                _mergeGroups = (await MergeDatabase.ListAsync<MergeGroup>()).ToList();
                _mergeGroupRecords = (await MergeDatabase.ListAsync<MergeGroupAttendanceRecord>()).ToList();
                reference.StopLoading();
            }
            tree.Items.Clear();
            if (_groupByDate) {
                var oam = new OverallAttendanceMetrics(_records, _groups, _mergeGroups, _mergeGroupRecords);
                var weeks =
                    oam.Weeks
                       .OrderByDescending(week => week.Date);
                var mgWeeks = oam.MergeGroupWeeks.OrderByDescending(week => week.Date);
                foreach (var w in weeks) {
                    var weekItem = new TreeViewItem {
                        Header = w.Date.ToLongDateString(),
                        Tag = new Tag("week", w)
                    };
                    TreeViewItem jh, hs;
                    weekItem.Items.Add(jh = new TreeViewItem {
                        Header = "Junior High",
                        Tag = new Tag("ministry", new {Ministry = "jh", Week = w})
                    });
                    weekItem.Items.Add(hs = new TreeViewItem {
                        Header = "High School",
                        Tag = new Tag("ministry", new {Ministry = "hs", Week = w})
                    });
                    foreach (var r in w.Records.OrderBy(rec => (int) GetGroup(rec.GroupId).GradeLevel)
                        .ThenBy(rec => rec.GroupId)) {
                        var group = GetGroup(r.GroupId);
                        TreeViewItem recordItem;
                        ((int) group.GradeLevel <= 8 ? jh : hs).Items.Add(recordItem = new TreeViewItem {
                            Header = group.Summary,
                            Tag = new Tag("record", r)
                        });
                        foreach (var s in r.Students)
                            recordItem.Items.Add(new TreeViewItem {
                                Header = s,
                                Tag = new Tag("student", new {Name = s, Group = group, Record = r})
                            });
                    }
                    tree.Items.Add(weekItem);
                }
                TreeViewItem mgItem;
                tree.Items.Add(mgItem = new TreeViewItem {
                    Header = "Merge Groups"
                });
                foreach (var w in mgWeeks) {
                    var weekItem = new TreeViewItem {
                        Header = w.Date.ToLongDateString(),
                        Tag = new Tag("mgweek", w)
                    };
                    foreach (var r in w.Records.OrderBy(rec => rec.MergeGroupId)) {
                        var group = _mergeGroups.First(g => g.Id == r.MergeGroupId);
                        weekItem.Items.Add(new TreeViewItem {
                            Header = group.Name,
                            Tag = new Tag("mgrecord", r)
                        });
                    }
                    if (weekItem.Items.Count > 0)
                        mgItem.Items.Add(weekItem);
                }
            } else {
                TreeViewItem jhItem = new TreeViewItem {
                        Header = "Junior High",
                        Tag = new Tag("ministry", new {Ministry = "jh"})
                    },
                    hsItem = new TreeViewItem {
                        Header = "High School",
                        Tag = new Tag("ministry", new {Ministry = "hs"})
                    },
                    mgItem = new TreeViewItem {
                        Header = "Merge Groups"
                    };
                foreach (var g in _groups.OrderBy(g => (int) g.GradeLevel).ThenBy(g => g.Id)) {
                    var groupItem = new TreeViewItem {
                        Header = g.Summary,
                        Tag = new Tag("group", g)
                    };
                    foreach (var r in _records.OrderByDescending(r => r.Date).Where(r => r.GroupId == g.Id)) {
                        var recordItem = new TreeViewItem {
                            Header = r.Date.ToLongDateString(),
                            Tag = new Tag("record", r)
                        };
                        r.Students.ForEach(s => recordItem.Items.Add(new TreeViewItem {
                            Header = s,
                            Tag = new Tag("student", new {Name = s, Group = g, Record = r})
                        }));
                        groupItem.Items.Add(recordItem);
                    }
                    ((int) g.GradeLevel <= 8 ? jhItem : hsItem).Items.Add(groupItem);
                }
                foreach (var g in _mergeGroups) {
                    var groupItem = new TreeViewItem {
                        Header = g.Name,
                        Tag = new Tag("mggroup", g)
                    };
                    foreach (var r in _mergeGroupRecords.OrderByDescending(r => r.Date)
                        .Where(r => r.MergeGroupId == g.Id))
                        groupItem.Items.Add(new TreeViewItem {
                            Header = r.Date.ToLongDateString(),
                            Tag = new Tag("mgrecord", r)
                        });
                    if (groupItem.Items.Count > 0)
                        mgItem.Items.Add(groupItem);
                }
                tree.Items.Add(jhItem);
                tree.Items.Add(hsItem);
                tree.Items.Add(mgItem);
            }
            if (tree.Items.Count == 0)
                tree.Items.Add(new TreeViewItem {
                    Header = "No data"
                });
            TreeViewItemSelected(null, null);
        }

        private void ToggleGroupingMode(object sender, RoutedEventArgs e) {
            _groupByDate = !_groupByDate;
            groupDate.IsChecked = _groupByDate;
            groupGroup.IsChecked = !_groupByDate;
            Load(false);
        }

        private void TreeViewItemSelected(object sender, RoutedPropertyChangedEventArgs<object> e) {
            ClearMetricsAndActions();
            var overall = new OverallAttendanceMetrics(_records, _groups, _mergeGroups, _mergeGroupRecords);
            var metricsDictionary = new Dictionary<string, string> {
                {"Total Students", overall.TotalStudents.ToString()},
                {"Total Groups", overall.Groups.Count.ToString()},
                {"Total Records", overall.Records.Count.ToString()},
                {"Total Merge Groups", overall.MergeGroups.Count.ToString()},
                {"Total Merge Group Records", overall.MergeGroupRecords.Count.ToString()},
                {"---1", ""},
                {"Average Merge Group Student Count", overall.AverageMergeGroupStudentCount.ToString()}, {
                    "Average Student Attendance",
                    $"{overall.AverageStudentCount} per week ({overall.AverageAttendancePercentage}%)"
                },
                {"Average Leader Attendance", $"{overall.AverageLeaderAttendancePercentage}%"}, {
                    "Highest Attendance Week",
                    overall.HighestAttendanceWeek == null
                        ? "No data"
                        : $"{overall.HighestAttendanceWeek.GetMetrics(_groups).TotalStudents} ({overall.HighestAttendanceWeek.GetMetrics(_groups).AverageAttendancePercentage}%) on {overall.HighestAttendanceWeek.Date.ToLongDateString()}"
                },
                {"---2", ""}, {
                    "Lowest Attendance Week",
                    overall.LowestAttendanceWeek == null
                        ? "No data"
                        : $"{overall.LowestAttendanceWeek.GetMetrics(_groups).TotalStudents} ({overall.LowestAttendanceWeek.GetMetrics(_groups).AverageAttendancePercentage}%) on {overall.LowestAttendanceWeek.Date.ToLongDateString()}"
                }, {
                    "Recent Attendance Week",
                    overall.MostRecentAttendanceWeek == null
                        ? "No data"
                        : $"{overall.MostRecentAttendanceWeek.GetMetrics(_groups).TotalStudents} ({overall.MostRecentAttendanceWeek.GetMetrics(_groups).AverageAttendancePercentage}%) on {overall.MostRecentAttendanceWeek.Date.ToLongDateString()}"
                },
                {"---3", ""}, {
                    "Highest Attendance Group",
                    overall.HighestAttendanceGroup == null
                        ? "No data"
                        : $"{overall.HighestAttendanceGroup.Summary} ({overall.HighestAttendanceGroup.GetMetrics(_records).AverageAttendancePercentage}%)"
                }, {
                    "Lowest Attendance Group",
                    overall.LowestAttendanceGroup == null
                        ? "No data"
                        : $"{overall.LowestAttendanceGroup.Summary} ({overall.LowestAttendanceGroup.GetMetrics(_records).AverageAttendancePercentage}%)"
                },
                {"---4", ""}, {
                    "Highest Merge Group Attendance Week",
                    overall.HighestMergeGroupAttendanceWeek == null
                        ? "No data"
                        : $"{overall.HighestMergeGroupAttendanceWeek.GetMetrics(_mergeGroups).StudentCount} on {overall.HighestMergeGroupAttendanceWeek.Date.ToLongDateString()}"
                }, {
                    "Lowest Merge Group Attendance Week",
                    overall.LowestMergeGroupAttendanceWeek == null
                        ? "No data"
                        : $"{overall.LowestMergeGroupAttendanceWeek.GetMetrics(_mergeGroups).StudentCount} on {overall.LowestMergeGroupAttendanceWeek.Date.ToLongDateString()}"
                }, {
                    "Recent Merge Group Attendance Week",
                    overall.MostRecentMergeGroupAttendanceWeek == null
                        ? "No data"
                        : $"{overall.MostRecentMergeGroupAttendanceWeek.GetMetrics(_mergeGroups).StudentCount} on {overall.MostRecentMergeGroupAttendanceWeek.Date.ToLongDateString()}"
                },
                {"---5", ""}, {
                    "Highest Attendance Merge Group",
                    overall.HighestAttendanceMergeGroup == null
                        ? "No data"
                        : $"{overall.HighestAttendanceMergeGroup.Name} ({overall.HighestAttendanceMergeGroup.GetMetrics(_mergeGroupRecords).AverageStudentCount})"
                }, {
                    "Lowest Attendance Merge Group",
                    overall.LowestAttendanceMergeGroup == null
                        ? "No data"
                        : $"{overall.LowestAttendanceMergeGroup.Name} ({overall.LowestAttendanceMergeGroup.GetMetrics(_mergeGroupRecords).AverageStudentCount})"
                },
                {"---6", ""}, {
                    "Highest Attendance Student",
                    overall.HighestAttendanceStudent == null
                        ? "No data"
                        : $"{overall.HighestAttendanceStudent.Item1} ({overall.HighestAttendanceStudent.Item2}%)"
                }, {
                    "Lowest Attendance Student",
                    overall.LowestAttendanceStudent == null
                        ? "No data"
                        : $"{overall.LowestAttendanceStudent.Item1} ({overall.LowestAttendanceStudent.Item2}%)"
                }
            };
            var tag = (Tag) (tree.SelectedItem as TreeViewItem)?.Tag;
            if (tag != null)
                if (_groupByDate)
                    switch (tag.Type) {
                        case "record":
                            var record = tag.GetArgument<AttendanceRecord>();
                            var group = GetGroup(record.GroupId);
                            SetActions(new Dictionary<string, Action> {
                                {
                                    "Edit Group", () => {
                                        EditorWindow.Create(group, false, r => {
                                            if (r == EditorWindow.ResultType.Published)
                                                Load(true);
                                        }).ShowDialog();
                                    }
                                }, {
                                    "Delete Group", async () => {
                                        if (MessageBox.Show(this,
                                                "This operation cannot be undone!  Are you sure you want to delete this group?",
                                                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation,
                                                MessageBoxResult.No) == MessageBoxResult.Yes) {
                                            /*var eligibleGroups =
                                                _groups.Where(
                                                    g => g.Id != group.Id && g.GradeLevel == group.GradeLevel &&
                                                         g.Gender == group.Gender).ToList();
                                            var choice = ChoiceWindow.GetChoice("Select New Group",
                                                $"Select a group to migrate the students of {group.Summary} to.",
                                                eligibleGroups.Select(g => g.Summary).ToArray());
                                            if (choice == -1) {
                                                MessageBox.Show(this,
                                                    "The operation was aborted.  No changes have been made.",
                                                    "Operation Aborted", MessageBoxButton.OK,
                                                    MessageBoxImage.Information);
                                                return;
                                            }
                                            */
                                            //var selectedGroup = eligibleGroups.ElementAt(choice);
                                            var reference = new LoaderReference(this);
                                            reference.StartLoading("Migrating students...");
                                            //selectedGroup.StudentNames.AddRange(group.StudentNames);
                                            //var newRecords = new List<AttendanceRecord>();
                                            /*
                                            foreach (var oldRecord in _records.Where(r => r.GroupId == group.Id)) {
                                                var correspondingRecord =
                                                    _records.FirstOrDefault(r => r.GroupId == selectedGroup.Id &&
                                                                                 r.Date.ToLongDateString() ==
                                                                                 oldRecord.Date
                                                                                     .ToLongDateString());
                                                if (correspondingRecord == null) {
                                                    newRecords.Add(new AttendanceRecord {
                                                        Date = oldRecord.Date,
                                                        GroupId = selectedGroup.Id,
                                                        LeadersPresent = false,
                                                        Students = oldRecord.Students
                                                    });
                                                } else {
                                                    correspondingRecord.Students.AddRange(oldRecord.Students);
                                                    newRecords.Add(correspondingRecord);
                                                }
                                            }
                                            reference.SetMessage("Updating group...");
                                            await MergeDatabase.UpdateAsync(selectedGroup);
                                            foreach (var nr in newRecords) {
                                                reference.SetMessage(
                                                    $"Updaing record {newRecords.IndexOf(nr) + 1} of {newRecords.Count}...");
                                                await MergeDatabase.UpdateAsync(nr);
                                            }*/
                                            var oldRecords = _records.Where(r => r.GroupId == group.Id).ToList();
                                            foreach (var or in oldRecords) {
                                                reference.SetMessage(
                                                    $"Deleting record {oldRecords.IndexOf(or) + 1} of {oldRecords.Count}...");
                                                await MergeDatabase.DeleteAsync(or);
                                            }
                                            reference.SetMessage("Deleting group...");
                                            await MergeDatabase.DeleteAsync(group);
                                            Load(true);
                                        }
                                    }
                                }, {
                                    "Edit Record", () => {
                                        EditorWindow.Create(record, false, r => {
                                            if (r == EditorWindow.ResultType.Published)
                                                Load(true);
                                        }).ShowDialog();
                                    }
                                }, {
                                    "Delete Record", async () => {
                                        if (MessageBox.Show(this,
                                                "Are you sure you want to delete this record?  This will affect attendance statistics.",
                                                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation,
                                                MessageBoxResult.No) == MessageBoxResult.Yes) {
                                            var reference = new LoaderReference(this);
                                            reference.StartLoading("Deleting record...");
                                            await MergeDatabase.DeleteAsync(record);
                                            reference.StopLoading();
                                            Load(true);
                                        }
                                    }
                                }
                            });
                            break;
                        case "student":
                            var arg = tag.GetArgument<dynamic>();
                            var record2 = (AttendanceRecord) arg.Record;
                            SetActions(new Dictionary<string, Action> {
                                {
                                    "Remove Student from Record", async () => {
                                        if (MessageBox.Show(this,
                                                "Are you sure you want to remove this student from this record?",
                                                "Confirm",
                                                MessageBoxButton.YesNo, MessageBoxImage.Exclamation,
                                                MessageBoxResult.No) ==
                                            MessageBoxResult.Yes) {
                                            var reference = new LoaderReference(this);
                                            reference.StartLoading("Removing student...");
                                            record2.Students.Remove(arg.Name);
                                            await MergeDatabase.UpdateAsync(record2);
                                            reference.StopLoading();
                                            Load(true);
                                        }
                                    }
                                }
                            });
                            break;
                        case "mgrecord":
                            var mgRecord = tag.GetArgument<MergeGroupAttendanceRecord>();
                            SetActions(new Dictionary<string, Action> {
                                {
                                    "Edit Record", () => {
                                        EditorWindow.Create(mgRecord, false, r => {
                                            if (r == EditorWindow.ResultType.Published)
                                                Load(true);
                                        }).ShowDialog();
                                    }
                                }, {
                                    "Delete Record", async () => {
                                        if (MessageBox.Show(this,
                                                "Are you sure you want to delete this record?  This will affect attendance statistics.",
                                                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation,
                                                MessageBoxResult.No) == MessageBoxResult.Yes) {
                                            var reference = new LoaderReference(this);
                                            reference.StartLoading("Deleting record...");
                                            await MergeDatabase.DeleteAsync(mgRecord);
                                            reference.StopLoading();
                                            Load(true);
                                        }
                                    }
                                }
                            });
                            break;
                    }
                else
                    switch (tag.Type) {
                        case "group":
                            var group = tag.GetArgument<AttendanceGroup>();
                            SetActions(new Dictionary<string, Action> {
                                {
                                    "Edit Group", () => {
                                        EditorWindow.Create(group, false, r => {
                                            if (r == EditorWindow.ResultType.Published)
                                                Load(true);
                                        }).ShowDialog();
                                    }
                                }, {
                                    "Delete Group", async () => {
                                        if (MessageBox.Show(this,
                                                "If you choose to delete this group, its students will be moved to another group of the same grade level and gender.  This operation cannot be undone!  Are you sure you want to delete this group?",
                                                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation,
                                                MessageBoxResult.No) == MessageBoxResult.Yes) {
                                            var eligibleGroups =
                                                _groups.Where(
                                                    g => g.Id != group.Id && g.GradeLevel == group.GradeLevel &&
                                                         g.Gender == group.Gender).ToList();
                                            var choice = ChoiceWindow.GetChoice("Select New Group",
                                                $"Select a group to migrate the students of {group.Summary} to.",
                                                eligibleGroups.Select(g => g.Summary).ToArray());
                                            if (choice == -1) {
                                                MessageBox.Show(this,
                                                    "The operation was aborted.  No changes have been made.",
                                                    "Operation Aborted", MessageBoxButton.OK,
                                                    MessageBoxImage.Information);
                                                return;
                                            }
                                            var selectedGroup = eligibleGroups.ElementAt(choice);
                                            var reference = new LoaderReference(this);
                                            reference.StartLoading("Migrating students...");
                                            selectedGroup.StudentNames.AddRange(group.StudentNames);
                                            var newRecords = new List<AttendanceRecord>();
                                            foreach (var oldRecord in _records.Where(r => r.GroupId == group.Id)) {
                                                var correspondingRecord =
                                                    _records.FirstOrDefault(r => r.GroupId == selectedGroup.Id &&
                                                                                 r.Date.ToLongDateString() ==
                                                                                 oldRecord.Date
                                                                                     .ToLongDateString());
                                                if (correspondingRecord == null) {
                                                    newRecords.Add(new AttendanceRecord {
                                                        Date = oldRecord.Date,
                                                        GroupId = selectedGroup.Id,
                                                        LeadersPresent = false,
                                                        Students = oldRecord.Students
                                                    });
                                                } else {
                                                    correspondingRecord.Students.AddRange(oldRecord.Students);
                                                    newRecords.Add(correspondingRecord);
                                                }
                                            }
                                            reference.SetMessage("Updating group...");
                                            await MergeDatabase.UpdateAsync(selectedGroup);
                                            foreach (var nr in newRecords) {
                                                reference.SetMessage(
                                                    $"Updaing record {newRecords.IndexOf(nr) + 1} of {newRecords.Count}...");
                                                await MergeDatabase.UpdateAsync(nr);
                                            }
                                            var oldRecords = _records.Where(r => r.GroupId == group.Id).ToList();
                                            foreach (var or in oldRecords) {
                                                reference.SetMessage(
                                                    $"Deleting record {oldRecords.IndexOf(or) + 1} of {oldRecords.Count}...");
                                                await MergeDatabase.DeleteAsync(or);
                                            }
                                            reference.SetMessage("Deleting group...");
                                            await MergeDatabase.DeleteAsync(group);
                                            Load(true);
                                        }
                                    }
                                }
                            });
                            break;
                        case "record":
                            var record = tag.GetArgument<AttendanceRecord>();
                            SetActions(new Dictionary<string, Action> {
                                {
                                    "Edit Record", () => {
                                        EditorWindow.Create(record, false, r => {
                                            if (r == EditorWindow.ResultType.Published)
                                                Load(true);
                                        }).ShowDialog();
                                    }
                                }, {
                                    "Delete Record", async () => {
                                        if (MessageBox.Show(this,
                                                "Are you sure you want to delete this record?  This will affect attendance statistics.",
                                                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation,
                                                MessageBoxResult.No) == MessageBoxResult.Yes) {
                                            var reference = new LoaderReference(this);
                                            reference.StartLoading("Deleting record...");
                                            await MergeDatabase.DeleteAsync(record);
                                            reference.StopLoading();
                                            Load(true);
                                        }
                                    }
                                }
                            });
                            break;
                        case "student":
                            var arg = tag.GetArgument<dynamic>();
                            var record2 = (AttendanceRecord) arg.Record;
                            SetActions(new Dictionary<string, Action> {
                                {
                                    "Remove Student from Record", async () => {
                                        if (MessageBox.Show(this,
                                                "Are you sure you want to remove this student from this record?",
                                                "Confirm",
                                                MessageBoxButton.YesNo, MessageBoxImage.Exclamation,
                                                MessageBoxResult.No) ==
                                            MessageBoxResult.Yes) {
                                            var reference = new LoaderReference(this);
                                            reference.StartLoading("Removing student...");
                                            record2.Students.Remove(arg.Name);
                                            await MergeDatabase.UpdateAsync(record2);
                                            reference.StopLoading();
                                            Load(true);
                                        }
                                    }
                                }
                            });
                            break;
                        case "mgrecord":
                            var mgRecord = tag.GetArgument<MergeGroupAttendanceRecord>();
                            SetActions(new Dictionary<string, Action> {
                                {
                                    "Edit Record", () => {
                                        EditorWindow.Create(mgRecord, false, r => {
                                            if (r == EditorWindow.ResultType.Published)
                                                Load(true);
                                        }).ShowDialog();
                                    }
                                }, {
                                    "Delete Record", async () => {
                                        if (MessageBox.Show(this,
                                                "Are you sure you want to delete this record?  This will affect attendance statistics.",
                                                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation,
                                                MessageBoxResult.No) == MessageBoxResult.Yes) {
                                            var reference = new LoaderReference(this);
                                            reference.StartLoading("Deleting record...");
                                            await MergeDatabase.DeleteAsync(mgRecord);
                                            reference.StopLoading();
                                            Load(true);
                                        }
                                    }
                                }
                            });
                            break;
                    }
            AddMetric("Overall Statistics", metricsDictionary);
            if (tree.SelectedItem == null) return;
            var ancestory = new List<TreeViewItem> {(TreeViewItem) tree.SelectedItem};
            TreeViewItem parent;
            while ((parent = ancestory.Last().Parent as TreeViewItem) != null)
                ancestory.Add(parent);
            ancestory.Reverse();
            foreach (var item in ancestory)
                AddMetric((Tag) item.Tag);
        }

        public void SetExpansionAll(TreeViewItem i, bool isExpanded = true) {
            tree.UpdateLayout();
            i.IsExpanded = isExpanded;
            foreach (var item in i.Items.OfType<TreeViewItem>()) {
                item.IsExpanded = isExpanded;
                SetExpansionAll(item, isExpanded);
            }
        }

        private void ExpandAll(object sender, RoutedEventArgs e) {
            tree.Items.OfType<TreeViewItem>().ToList().ForEach(i => SetExpansionAll(i));
        }

        private void CollapseAll(object sender, RoutedEventArgs e) {
            tree.Items.OfType<TreeViewItem>().ToList().ForEach(i => SetExpansionAll(i, false));
        }

        private void NewGroup_Clicked(object sender, RoutedEventArgs e) {
            EditorWindow.Create<AttendanceGroup>(null, false, r => {
                if (r == EditorWindow.ResultType.Published)
                    Load(true);
            }).ShowDialog();
        }

        private void NewRecord_Clicked(object sender, RoutedEventArgs e) {
            EditorWindow.Create<AttendanceRecord>(null, false, r => {
                if (r == EditorWindow.ResultType.Published)
                    Load(true);
            }).ShowDialog();
        }

        private void Refresh_Clicked(object sender, RoutedEventArgs e) {
            Load(true);
        }

        private void Close_Clicked(object sender, RoutedEventArgs e) {
            Close();
        }

        private void NewMergeGroup_Clicked(object sender, RoutedEventArgs e) {
            EditorWindow.Create(default(MergeGroup), false, r => {
                if (r == EditorWindow.ResultType.Published)
                    Load(true);
            }).Show();
        }

        private void NewMergeGroupRecord_Clicked(object sender, RoutedEventArgs e) {
            EditorWindow.Create(default(MergeGroupAttendanceRecord), false, r => {
                if (r == EditorWindow.ResultType.Published)
                    Load(true);
            }).Show();
        }

        private sealed class Tag {
            private readonly object _argument;

            public Tag(string t, object arg) {
                Type = t;
                _argument = arg;
            }

            public string Type { get; }

            public T GetArgument<T>() => (T) _argument;
        }

        private async void Clean_Clicked(object sender, RoutedEventArgs e) {
            List<T> RemoveDuplicates<T>(IEnumerable<T> objects, Func<T, string> selector) {
                var final = new List<T>();
                foreach (var o in objects) {
                    if (final.Select(selector).Contains(selector(o)))
                        continue;
                    final.Add(o);
                }
                return final;
            }

            if (MessageBox.Show(this, "This will delete duplicate groups and records.  Do you want to continue?",
                    "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes) ==
                MessageBoxResult.Yes) {
                var reference = new LoaderReference(this);
                reference.StartLoading("Looking for duplicates...");
                var duplicateGroups = RemoveDuplicates(_groups.Where(g => _groups.Count(g2 => g2.Id == g.Id) > 1), g => g.Id);
                var duplicateRecords = RemoveDuplicates(_records.Where(r =>
                    _records.Count(r2 => r2.GroupId == r.GroupId && r2.DateString == r.DateString) > 1), r => $"{r.GroupId}{r.DateString}");
                var message =
                    $"The following duplicate groups will be deleted:\n{duplicateGroups.Aggregate("", (current, item) => current + $"{item.Id} ({item.Summary})\n")}\nThe following duplicate records will be deleted:\n{duplicateRecords.Aggregate("", (current, item) => current + $"{item.GroupId} ({_groups.First(g => g.Id == item.GroupId).Summary})/{item.Date.ToLongDateString()}\n")}";
                MessageBox.Show(this, message, "Notice", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                foreach (var g in duplicateGroups) {
                    reference.SetMessage($"Deleting attendance/groups/{g.Id}");
                    await MergeDatabase.DeleteAsync(g);
                }
                foreach (var r in duplicateRecords) {
                    reference.SetMessage($"Deleting attendance/groups/{r.GroupId}/{r.DateString}");
                    await MergeDatabase.DeleteAsync(r);
                }
                reference.StopLoading();
                Load(true);
            }
        }
    }
}
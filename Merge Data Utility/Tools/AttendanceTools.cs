#region LICENSE

// Project Merge Data Utility:  AttendanceTools.cs (in Solution Merge Data Utility)
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
using MergeApi.Models.Core;
using MergeApi.Models.Core.Attendance;
using MergeApi.Tools;

#endregion

namespace Merge_Data_Utility.Tools {
    public static class AttendanceTools {
        private static List<Week> GetWeeks(IList<AttendanceRecord> records) {
            var raw = new Dictionary<List<AttendanceRecord>, DateTime>();
            foreach (var r in records)
                if (!raw.ContainsValue(r.Date.WithoutTime()))
                    raw.Add(records.Where(r2 => r2.Date.WithoutTime() == r.Date.WithoutTime()).ToList(),
                        r.Date.WithoutTime());
            return raw.Select(pair => new Week(pair.Key)).ToList();
        }

        private static List<MergeGroupWeek> GetMergeGroupWeeks(IList<MergeGroupAttendanceRecord> records) {
            var raw = new Dictionary<List<MergeGroupAttendanceRecord>, DateTime>();
            foreach (var r in records)
                if (!raw.ContainsValue(r.Date.WithoutTime()))
                    raw.Add(records.Where(r2 => r2.Date.WithoutTime() == r.Date.WithoutTime()).ToList(),
                        r.Date.WithoutTime());
            return raw.Select(pair => new MergeGroupWeek(pair.Key)).ToList();
        }

        public static AttendanceGroupMetrics GetMetrics(this AttendanceGroup g, List<AttendanceRecord> allRecords) =>
            new AttendanceGroupMetrics(g, allRecords);

        public static AttendanceRecordMetrics GetMetrics(this AttendanceRecord r, AttendanceGroup group) =>
            new AttendanceRecordMetrics(r, group);

        public static AttendanceWeekMetrics GetMetrics(this Week w, List<AttendanceGroup> allGroups) =>
            new AttendanceWeekMetrics(w, allGroups);

        public static MergeGroupAttendanceMetrics GetMetrics(this MergeGroup g,
            List<MergeGroupAttendanceRecord> allRecords) => new MergeGroupAttendanceMetrics(g, allRecords);

        public static OverallAttendanceMetrics GetMetrics(List<AttendanceRecord> allRecords,
            List<AttendanceGroup> allGroups, List<MergeGroup> allMergeGroups,
            List<MergeGroupAttendanceRecord> allMergeGroupRecords) => new OverallAttendanceMetrics(allRecords,
            allGroups, allMergeGroups, allMergeGroupRecords);

        public static MergeGroupAttendanceWeekMetrics GetMetrics(this MergeGroupWeek w, List<MergeGroup> allGroups) =>
            new MergeGroupAttendanceWeekMetrics(w, allGroups);

        private static int NotZero(this int i) => i == 0 ? 1 : i;

        private static int GetPercentage(int top, int bottom, bool multiply) => Convert.ToInt32(
            Convert.ToDouble(top) / Convert.ToDouble(bottom.NotZero()) * (multiply ? 100 : 1));

        private sealed class StudentPercentage : IComparable, IComparable<StudentPercentage> {
            public StudentPercentage(Tuple<string, int> source) {
                Tuple = source;
            }

            public Tuple<string, int> Tuple { get; }

            public int CompareTo(object other) {
                return Tuple?.Item2.CompareTo(((StudentPercentage) other).Tuple?.Item2 ?? 0) ?? 0;
            }

            public int CompareTo(StudentPercentage percentage) {
                return CompareTo(other: percentage);
            }
        }

        public sealed class MergeGroupAttendanceMetrics {
            public MergeGroupAttendanceMetrics(MergeGroup group, List<MergeGroupAttendanceRecord> allRecords) {
                MergeGroup = group;
                Records = allRecords.Where(r => r.MergeGroupId == group.Id).ToList();
                AverageStudentCount =
                    Convert.ToInt32(Math.Round(
                        Convert.ToDouble(Records.Sum(r => r.StudentCount)) / Convert.ToDouble(Records.Count.NotZero()),
                        0));
                HighestAttendanceRecord = Records.Max(r => r.StudentCount);
                LowestAttendanceRecord = Records.Min(r => r.StudentCount);
                MostRecentAttendanceRecord = Records.Max(r => r.Date);
            }

            public int AverageStudentCount { get; }

            public MergeGroupAttendanceRecord HighestAttendanceRecord { get; }

            public MergeGroupAttendanceRecord LowestAttendanceRecord { get; }

            public MergeGroupAttendanceRecord MostRecentAttendanceRecord { get; }

            public MergeGroup MergeGroup { get; }

            public List<MergeGroupAttendanceRecord> Records { get; }
        }

        public sealed class MergeGroupAttendanceWeekMetrics {
            public MergeGroupAttendanceWeekMetrics(MergeGroupWeek week, List<MergeGroup> groups) {
                MergeGroupWeek = week;
                Groups = groups.Where(g => week.Records.Select(r => r.MergeGroupId).Contains(g.Id)).ToList();
                StudentCount = MergeGroupWeek.Records.Sum(r => r.StudentCount);
                if (Groups.Count == 0 || MergeGroupWeek.Records.Count == 0) {
                    HighestMergeGroupAttendanceRecord = null;
                    LowestMergeGroupAttendanceRecord = null;
                    HighestAttendanceMergeGroup = null;
                    LowestAttendanceMergeGroup = null;
                } else {
                    var groupMetrics = Groups.Select(g => g.GetMetrics(MergeGroupWeek.Records.ToList())).ToList();
                    HighestMergeGroupAttendanceRecord = groupMetrics.Where(m => m.HighestAttendanceRecord != null)
                        .Max(m => m.HighestAttendanceRecord.StudentCount).HighestAttendanceRecord;
                    LowestMergeGroupAttendanceRecord = groupMetrics.Where(m => m.LowestAttendanceRecord != null)
                        .Min(m => m.LowestAttendanceRecord.StudentCount).LowestAttendanceRecord;
                    HighestAttendanceMergeGroup =
                        Groups.First(g => g.Id == HighestMergeGroupAttendanceRecord.MergeGroupId);
                    LowestAttendanceMergeGroup =
                        Groups.First(g => g.Id == LowestMergeGroupAttendanceRecord.MergeGroupId);
                }
            }

            public int StudentCount { get; }

            public MergeGroupAttendanceRecord HighestMergeGroupAttendanceRecord { get; }

            public MergeGroupAttendanceRecord LowestMergeGroupAttendanceRecord { get; }

            public MergeGroup HighestAttendanceMergeGroup { get; }

            public MergeGroup LowestAttendanceMergeGroup { get; }

            public MergeGroupWeek MergeGroupWeek { get; }

            public List<MergeGroup> Groups { get; }
        }

        public sealed class AttendanceRecordMetrics {
            public AttendanceRecordMetrics(AttendanceRecord record, AttendanceGroup group) {
                AttendanceRecord = record;
                AttendanceGroup = group;
                AttendancePercentage = GetPercentage(record.Students.Count, group.StudentNames.Count, true);
            }

            public int AttendancePercentage { get; }

            public AttendanceRecord AttendanceRecord { get; }

            public AttendanceGroup AttendanceGroup { get; }
        }

        public sealed class AttendanceGroupMetrics {
            public AttendanceGroupMetrics(AttendanceGroup group, List<AttendanceRecord> allRecords) {
                AttendanceGroup = group;
                GroupRecords = allRecords.Where(r => r.GroupId == group.Id).OrderByDescending(r => r.Date).ToList();
                AverageLeaderAttendancePercentage = GetPercentage(GroupRecords.Sum(r => r.LeadersPresent ? 1 : 0),
                    GroupRecords.Count, true);
                AverageStudentCount =
                    Convert.ToInt32(Math.Round(
                        Convert.ToDouble(GroupRecords.Sum(r => r.Students.Count)) /
                        Convert.ToDouble(GroupRecords.Count.NotZero()), 0));
                if (GroupRecords.Count == 0) {
                    AverageAttendancePercentage = 0;
                    HighestAttendanceRecord = null;
                    LowestAttendanceRecord = null;
                    MostRecentAttendanceRecord = null;
                } else {
                    var recordMetrics = GroupRecords.Select(r => r.GetMetrics(group)).ToList();
                    AverageAttendancePercentage = GetPercentage(recordMetrics.Sum(m => m.AttendancePercentage),
                        recordMetrics.Count, false);
                    HighestAttendanceRecord = GroupRecords.Max(r => r.GetMetrics(group).AttendancePercentage);
                    LowestAttendanceRecord = GroupRecords.Min(r => r.GetMetrics(group).AttendancePercentage);
                    MostRecentAttendanceRecord = !GroupRecords.Any() ? null : GroupRecords.First();
                }
            }

            public int AverageLeaderAttendancePercentage { get; }

            public int AverageAttendancePercentage { get; }

            public int AverageStudentCount { get; }

            public AttendanceRecord HighestAttendanceRecord { get; }

            public AttendanceRecord LowestAttendanceRecord { get; }

            public AttendanceRecord MostRecentAttendanceRecord { get; }

            public AttendanceGroup AttendanceGroup { get; }

            public List<AttendanceRecord> GroupRecords { get; }

            public Tuple<string, int> HighestStudentAttendancePercentage {
                get {
                    if (GroupRecords.Count == 0)
                        return null;
                    var rates = AttendanceGroup.StudentNames.ToDictionary(name => name, GetStudentAttendancePercentage);
                    return rates.Max(pair => pair.Value)
                        .Manipulate(pair => new Tuple<string, int>(pair.Key, pair.Value));
                }
            }

            public Tuple<string, int> LowestStudentAttendancePercentage {
                get {
                    if (GroupRecords.Count == 0)
                        return null;
                    var rates = AttendanceGroup.StudentNames.ToDictionary(name => name, GetStudentAttendancePercentage);
                    return rates.Min(pair => pair.Value)
                        .Manipulate(pair => new Tuple<string, int>(pair.Key, pair.Value));
                }
            }

            public int GetStudentAttendancePercentage(string name) {
                return GetPercentage(GroupRecords.Sum(r => r.Students.Contains(name) ? 1 : 0), GroupRecords.Count,
                    true);
            }
        }

        public sealed class AttendanceWeekMetrics {
            public AttendanceWeekMetrics(Week week, List<AttendanceGroup> allGroups) {
                Week = week;
                Groups = allGroups.Where(g => Week?.Records.Any(r => r.GroupId == g.Id) ?? false).ToList();
                TotalStudents = Week?.Records.Sum(r => r.Students.Count) ?? 0;
                if (Week == null || Groups.Count == 0 || Week.Records.Count == 0) {
                    AverageLeaderAttendancePercentage = 0;
                    AverageAttendancePercentage = 0;
                    HighestAttendanceRecord = null;
                    LowestAttendanceRecord = null;
                    HighestAttendanceGroup = null;
                    LowestAttendanceGroup = null;
                } else {
                    var groupMetrics = Groups.Select(g => g.GetMetrics(Week.Records.ToList())).ToList();
                    AverageLeaderAttendancePercentage = GetPercentage(
                        groupMetrics.Sum(m => m.AverageLeaderAttendancePercentage),
                        groupMetrics.Count, false);
                    AverageAttendancePercentage = GetPercentage(groupMetrics.Sum(m => m.AverageStudentCount),
                        allGroups.Sum(g => g.StudentNames.Count), true);
                    HighestAttendanceRecord = groupMetrics.Where(m => m.HighestAttendanceRecord != null)
                        .Max(m => m.HighestAttendanceRecord.GetMetrics(m.AttendanceGroup)
                            .AttendancePercentage).HighestAttendanceRecord;
                    LowestAttendanceRecord = groupMetrics.Where(m => m.LowestAttendanceRecord != null)
                        .Min(m => m.LowestAttendanceRecord.GetMetrics(m.AttendanceGroup)
                            .AttendancePercentage).LowestAttendanceRecord;
                    HighestAttendanceGroup = Groups.First(g => g.Id == HighestAttendanceRecord.GroupId);
                    LowestAttendanceGroup = Groups.First(g => g.Id == LowestAttendanceRecord.GroupId);
                }
            }

            public int AverageLeaderAttendancePercentage { get; }

            public int AverageAttendancePercentage { get; }

            public AttendanceRecord HighestAttendanceRecord { get; }

            public AttendanceRecord LowestAttendanceRecord { get; }

            public AttendanceGroup HighestAttendanceGroup { get; }

            public AttendanceGroup LowestAttendanceGroup { get; }

            public int TotalStudents { get; }

            public Week Week { get; }

            public List<AttendanceGroup> Groups { get; }
        }

        public sealed class OverallAttendanceMetrics {
            public OverallAttendanceMetrics(List<AttendanceRecord> allRecords, List<AttendanceGroup> allGroups,
                List<MergeGroup> mergeGroups, List<MergeGroupAttendanceRecord> mergeGroupRecords) {
                Groups = allGroups;
                Records = allRecords;
                MergeGroups = mergeGroups;
                MergeGroupRecords = mergeGroupRecords;
                MergeGroupWeeks = GetMergeGroupWeeks(mergeGroupRecords);
                Weeks = GetWeeks(allRecords);
                TotalStudents = Groups.Sum(g => g.StudentNames.Count);
                AverageStudentCount = Convert.ToInt32(Math.Round(
                        Convert.ToDouble(Weeks.Sum(w => w.Records.Sum(r => r.Students.Count))) /
                        Convert.ToDouble(Weeks.Count.NotZero()), 0));
                AverageMergeGroupStudentCount =
                    Convert.ToInt32(Math.Round(
                        Convert.ToDouble(MergeGroupWeeks.Sum(w => w.Records.Sum(r => r.StudentCount))) /
                        Convert.ToDouble(MergeGroupWeeks.Count.NotZero()), 0));
                AverageLeaderAttendancePercentage =
                    GetPercentage(Weeks.Sum(w => w.GetMetrics(Groups).AverageLeaderAttendancePercentage), Weeks.Count.NotZero(),
                        false);
                AverageAttendancePercentage = GetPercentage(
                    Weeks.Sum(w => w.GetMetrics(Groups).AverageAttendancePercentage),
                    Weeks.Count.NotZero(), false);
                if (Weeks.Count == 0) {
                    HighestAttendanceWeek = null;
                    LowestAttendanceWeek = null;
                    MostRecentAttendanceWeek = null;
                } else {
                    HighestAttendanceWeek = Weeks.Max(w => {
                        var record = w.GetMetrics(Groups).HighestAttendanceRecord;
                        return record.GetMetrics(Groups.First(g => g.Id == record.GroupId)).AttendancePercentage;
                    });
                    LowestAttendanceWeek = Weeks.Min(w => {
                        var record = w.GetMetrics(Groups).LowestAttendanceRecord;
                        return record.GetMetrics(Groups.First(g => g.Id == record.GroupId)).AttendancePercentage;
                    });
                    MostRecentAttendanceWeek = Weeks.Max(w => w.Date);
                }
                if (Groups.Count == 0 || Records.Count == 0) {
                    HighestAttendanceGroup = null;
                    LowestAttendanceGroup = null;
                    HighestAttendanceStudent = null;
                    LowestAttendanceStudent = null;
                } else {
                    HighestAttendanceGroup = Groups.Select(g => g.GetMetrics(Records))
                        .Max(m => m.AverageAttendancePercentage).AttendanceGroup;
                    LowestAttendanceGroup = Groups.Select(g => g.GetMetrics(Records))
                        .Min(m => m.AverageAttendancePercentage).AttendanceGroup;
                    HighestAttendanceStudent = Groups.Select(g => g.GetMetrics(Records))
                        .Select(m => new StudentPercentage(m.HighestStudentAttendancePercentage))
                        .Max().Tuple;
                    LowestAttendanceStudent = Groups.Select(g => g.GetMetrics(Records))
                        .Select(m => new StudentPercentage(m.LowestStudentAttendancePercentage))
                        .Min().Tuple;
                }
                if (MergeGroupWeeks.Count == 0 || MergeGroups.Count == 0 || MergeGroupRecords.Count == 0) {
                    HighestMergeGroupAttendanceWeek = null;
                    LowestMergeGroupAttendanceWeek = null;
                    MostRecentMergeGroupAttendanceWeek = null;
                    HighestAttendanceMergeGroup = null;
                    LowestAttendanceMergeGroup = null;
                } else {
                    HighestMergeGroupAttendanceWeek = MergeGroupWeeks.Max(w => w.GetMetrics(MergeGroups).StudentCount);
                    LowestMergeGroupAttendanceWeek = MergeGroupWeeks.Min(w => w.GetMetrics(MergeGroups).StudentCount);
                    MostRecentMergeGroupAttendanceWeek = MergeGroupWeeks.Max(w => w.Date);
                    HighestAttendanceMergeGroup = MergeGroups.Select(g => g.GetMetrics(MergeGroupRecords))
                        .Max(m => m.AverageStudentCount).MergeGroup;
                    LowestAttendanceMergeGroup = MergeGroups.Select(g => g.GetMetrics(MergeGroupRecords))
                        .Min(m => m.AverageStudentCount).MergeGroup;
                }
            }

            public int AverageLeaderAttendancePercentage { get; }

            public int AverageAttendancePercentage { get; }

            public int TotalStudents { get; }

            public int AverageStudentCount { get; }

            public int AverageMergeGroupStudentCount { get; }

            public List<AttendanceGroup> Groups { get; }

            public List<AttendanceRecord> Records { get; }

            public List<MergeGroup> MergeGroups { get; }

            public List<MergeGroupAttendanceRecord> MergeGroupRecords { get; }

            public Week HighestAttendanceWeek { get; }

            public Week LowestAttendanceWeek { get; }

            public Week MostRecentAttendanceWeek { get; }

            public MergeGroupWeek HighestMergeGroupAttendanceWeek { get; }

            public MergeGroupWeek LowestMergeGroupAttendanceWeek { get; }

            public MergeGroupWeek MostRecentMergeGroupAttendanceWeek { get; }

            public AttendanceGroup HighestAttendanceGroup { get; }

            public AttendanceGroup LowestAttendanceGroup { get; }

            public MergeGroup HighestAttendanceMergeGroup { get; }

            public MergeGroup LowestAttendanceMergeGroup { get; }

            public Tuple<string, int> HighestAttendanceStudent { get; }

            public Tuple<string, int> LowestAttendanceStudent { get; }

            public List<Week> Weeks { get; }

            public List<MergeGroupWeek> MergeGroupWeeks { get; }
        }

        public sealed class MergeGroupWeek {
            public MergeGroupWeek(IList<MergeGroupAttendanceRecord> allRecords) {
                if (allRecords == null || allRecords.Count == 0)
                    throw new ArgumentException("Records must not be null or empty.", nameof(allRecords));
                var r = allRecords.Select(record => new MergeGroupAttendanceRecord {
                    Date = record.Date.WithoutTime(),
                    StudentCount = record.StudentCount,
                    MergeGroupId = record.MergeGroupId,
                    Image = record.Image
                }).ToList();
                Date = r[0].Date;
                Records = r.Where(record => record.Date == Date).ToList();
            }

            public DateTime Date { get; }

            public IList<MergeGroupAttendanceRecord> Records { get; }
        }

        public sealed class Week {
            public Week(IList<AttendanceRecord> allRecords) {
                if (allRecords == null || allRecords.Count == 0)
                    throw new ArgumentException("Records must not be null or empty.", nameof(allRecords));
                var r = allRecords.Select(record => new AttendanceRecord {
                    Date = record.Date.WithoutTime(),
                    Students = record.Students,
                    GroupId = record.GroupId,
                    LeadersPresent = record.LeadersPresent
                }).ToList();
                Date = r[0].Date;
                Records = r.Where(record => record.Date == Date).ToList();
            }

            public DateTime Date { get; }

            public IList<AttendanceRecord> Records { get; }
        }
    }
}
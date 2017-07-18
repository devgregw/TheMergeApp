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
using MergeApi.Models.Core.Attendance;

#endregion

namespace Merge_Data_Utility.Tools {
    public static class AttendanceTools {
        public static List<Week> GetWeeks(IList<AttendanceRecord> records) {
            var raw = new Dictionary<List<AttendanceRecord>, DateTime>();
            foreach (var r in records)
                if (!raw.ContainsValue(r.Date.WithoutTime()))
                    raw.Add(records.Where(r2 => r2.Date.WithoutTime() == r.Date.WithoutTime()).ToList(),
                        r.Date.WithoutTime());
            return raw.Select(pair => new Week(pair.Key)).ToList();
        }

        public static List<Tuple<int, DateTime>> GetWeeklyTotals(IEnumerable<Week> weeks) {
            var totals = weeks.Select(
                w =>
                    new Tuple<int, DateTime>(w.Records.Sum(r => r.Students.Count), w.Date)).ToList();
            totals.Sort(new TupleComparison<int, DateTime>());
            return totals;
        }

        public static Tuple<int, double> GetAverageAttendance(IList<AttendanceRecord> records,
            IList<AttendanceGroup> groups) {
            try {
                var weeks = GetWeeks(records);
                var avg = Convert.ToDouble(weeks.Sum(w => w.Records.Sum(r => r.Students.Count))) / weeks.Count;
                return new Tuple<int, double>(Convert.ToInt32(avg),
                    Math.Round(Convert.ToInt32(avg) / Convert.ToDouble(groups.Sum(g => g.StudentNames.Count)) * 100d,
                        1));
            } catch {
                return null;
            }
        }

        public static Tuple<int, DateTime> GetLowestRecordedAttendance(IList<AttendanceRecord> records) {
            return GetWeeklyTotals(GetWeeks(records)).FirstOrDefault();
        }

        public static Tuple<int, DateTime> GetHighestRecordedAttendance(IList<AttendanceRecord> records) {
            return GetWeeklyTotals(GetWeeks(records)).LastOrDefault();
        }

        public static Tuple<int, DateTime> GetMostRecentAttendance(IList<AttendanceRecord> records) {
            try {
                var totals = GetWeeklyTotals(GetWeeks(records));
                return totals.First(t => t.Item2 == totals.Select(_t => t.Item2).Max());
            } catch {
                return null;
            }
        }

        public static Tuple<int, int> GetTotals(IList<AttendanceGroup> groups) {
            try {
                return new Tuple<int, int>(groups.Count, groups.Sum(g => g.StudentNames.Count));
            } catch {
                return null;
            }
        }

        private sealed class TupleComparison<T1, T2> : IComparer<Tuple<T1, T2>> where T1 : IComparable {
            public int Compare(Tuple<T1, T2> x, Tuple<T1, T2> y) {
                return x.Item1.CompareTo(y.Item1);
            }
        }

        public sealed class Week {
            public Week(IList<AttendanceRecord> records) {
                if (records == null || records.Count == 0)
                    throw new ArgumentException("Records must not be null or empty.", nameof(records));
                var r = records.Select(record => new AttendanceRecord {
                    Date = record.Date.WithoutTime(),
                    Students = record.Students,
                    GroupId = record.GroupId
                }).ToList();
                Date = r[0].Date;
                Records = r.Where(record => record.Date == Date).ToList();
            }

            public DateTime Date { get; }

            public IList<AttendanceRecord> Records { get; }
        }
    }
}
#region LICENSE

// Project Merge Data Utility:  AttendanceControl.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 04/02/2017 at 8:07 AM.
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
using System.Windows;
using System.Windows.Controls;
using MergeApi.Models.Core.Attendance;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Windows;

#endregion

namespace Merge_Data_Utility.UI.Controls {
    /// <summary>
    ///     Interaction logic for AttendanceControl.xaml
    /// </summary>
    public partial class AttendanceControl : UserControl {
        private DateTime _date;

        private Action<AttendanceGroup> _edit, _delete;

        private AttendanceGroup _group;

        private IEnumerable<AttendanceGroup> _groups;

        private int _mode; // 0 = record, 1 = group

        private IEnumerable<AttendanceRecord> _records;

        public AttendanceControl() {
            InitializeComponent();
        }

        public AttendanceControl(IEnumerable<AttendanceRecord> records, IEnumerable<AttendanceGroup> allGroups,
            DateTime recordDate) : this() {
            _records = records.Where(r => r.Date.WithoutTime() == recordDate.WithoutTime());
            _groups = allGroups;
            _date = recordDate;
            _mode = 0;
            Update();
        }

        public AttendanceControl(IEnumerable<AttendanceRecord> records, AttendanceGroup group,
            Action<AttendanceGroup> edit, Action<AttendanceGroup> delete) : this() {
            _records = records;
            _group = group;
            _mode = 1;
            _edit = edit;
            _delete = delete;
            Update();
        }


        private void Update() {
            if (_mode == 0) {
                editButton.Visibility = Visibility.Collapsed;
                deleteButton.Visibility = Visibility.Collapsed;
                header.Text = _date.ToLongDateString();
                subheader.Text = $"{_records.Sum(r => r.Students.Count)} students (in {_records.Count()} records)";
                id.Text = "";
            } else {
                editButton.Visibility = Visibility.Visible;
                deleteButton.Visibility = Visibility.Visible;
                header.Text = _group.Summary;
                subheader.Text = $"{_group.StudentNames.Count} students";
                id.Text = $"attendance/groups/{_group.Id}";
            }
        }

        private void Details_Requested(object sender, RoutedEventArgs e) {
            if (_mode == 0)
                new AttendanceDetailWindow(_records.ToList()).ShowDialog();
            else
                new AttendanceDetailWindow(_group).ShowDialog();
        }

        private void Edit_Requested(object sender, RoutedEventArgs e) {
            // assume _mode == 1
            _edit(_group);
        }

        private void Delete_Requested(object sender, RoutedEventArgs e) {
            // assume _mode == 1
            _delete(_group);
        }
    }
}
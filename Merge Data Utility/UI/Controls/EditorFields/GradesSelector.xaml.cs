#region LICENSE

// Project Merge Data Utility:  GradesSelector.xaml.cs (in Solution Merge Data Utility)
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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Enumerations.Converters;

#endregion

namespace Merge_Data_Utility.UI.Controls.EditorFields {
    /// <summary>
    ///     Interaction logic for GradesSelector.xaml
    /// </summary>
    public partial class GradesSelector : UserControl {
        public GradesSelector() {
            InitializeComponent();
        }

        public GradesSelector(IEnumerable<GradeLevel> levels) : this() {
            Value = levels.ToList();
        }

        public List<GradeLevel> Value {
            get {
                var bools = new bool[6];
                for (var i = 0; i < bools.Length; i++)
                    bools[i] = gradesBox.SelectedItems.Contains(gradesBox.Items[i]);
                var levels = new List<GradeLevel>();
                for (var i = 0; i < bools.Length; i++)
                    if (bools[i])
                        levels.Add(GradeLevelConverter.FromInt32(i + 7));
                return levels;
            }
            set {
                gradesBox.SelectedItems.Clear();
                foreach (var l in EnumConsts.AllGradeLevels)
                    if (value.Contains(l))
                        gradesBox.SelectedItems.Add(gradesBox.Items[GradeLevelConverter.ToInt32(l) - 7]);
            }
        }

        private void gradesSelectAll_Click(object sender, RoutedEventArgs e) {
            gradesBox.SelectedItems.Clear();
            for (var i = 0; i < 6; i++)
                gradesBox.SelectedItems.Add(gradesBox.Items[i]);
        }

        private void gradesSelectNone_Click(object sender, RoutedEventArgs e) {
            gradesBox.SelectedItems.Clear();
        }

        private void gradesSelectJH_Click(object sender, RoutedEventArgs e) {
            gradesBox.SelectedItems.Clear();
            gradesBox.SelectedItems.Add(gradesBox.Items[0]);
            gradesBox.SelectedItems.Add(gradesBox.Items[1]);
        }

        private void gradesSelectHS_Click(object sender, RoutedEventArgs e) {
            gradesBox.SelectedItems.Clear();
            gradesBox.SelectedItems.Add(gradesBox.Items[2]);
            gradesBox.SelectedItems.Add(gradesBox.Items[3]);
            gradesBox.SelectedItems.Add(gradesBox.Items[4]);
            gradesBox.SelectedItems.Add(gradesBox.Items[5]);
        }

        private void gradesSelectUnder_Click(object sender, RoutedEventArgs e) {
            gradesBox.SelectedItems.Clear();
            gradesBox.SelectedItems.Add(gradesBox.Items[2]);
            gradesBox.SelectedItems.Add(gradesBox.Items[3]);
        }

        private void gradesSelectUpper_Click(object sender, RoutedEventArgs e) {
            gradesBox.SelectedItems.Clear();
            gradesBox.SelectedItems.Add(gradesBox.Items[4]);
            gradesBox.SelectedItems.Add(gradesBox.Items[5]);
        }
    }
}
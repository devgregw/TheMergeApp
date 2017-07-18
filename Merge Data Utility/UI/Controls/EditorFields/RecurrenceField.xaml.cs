#region LICENSE

// Project Merge Data Utility:  RecurrenceField.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 06/30/2017 at 5:47 PM.
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
using MergeApi.Framework.Enumerations;
using MergeApi.Tools;

#endregion

namespace Merge_Data_Utility.UI.Controls.EditorFields {
    /// <summary>
    ///     Interaction logic for RecurrenceField.xaml
    /// </summary>
    public partial class RecurrenceField : UserControl {
        public RecurrenceField() {
            InitializeComponent();
        }

        public List<string> GetValidationErrors() {
            if (!enableBox.IsChecked.GetValueOrDefault(false))
                return new List<string>();
            return new List<string> {
                frequencyBox.SelectedIndex == -1 ? "A recurrence frequency must be selected." : "",
                endBehaviorBox.SelectedIndex == 1 && !endDateBox.Value.HasValue
                    ? "A recurrence end date and time must be specified."
                    : "",
                endBehaviorBox.SelectedIndex == -1 ? "A recurrence end behavior must be selected." : ""
            };
        }

        public RecurrenceRule GetRule() =>
            enableBox.IsChecked.GetValueOrDefault(false)
                ? new RecurrenceRule {
                    Frequency = ((ComboBoxItem) frequencyBox.Items[frequencyBox.SelectedIndex]).Content.ToString()
                        .ToEnum<RecurrenceFrequency>(),
                    Interval = intervalBox.Value.GetValueOrDefault(1),
                    Count = endBehaviorBox.SelectedIndex == 2 ? occurrencesBox.Value.GetValueOrDefault(1) : (int?) null,
                    // ReSharper disable once PossibleInvalidOperationException
                    End = endBehaviorBox.SelectedIndex == 1 ? endDateBox.Value.Value : (DateTime?) null
                }
                : null;

        public void SetSource(RecurrenceRule source) {
            if (source != null) {
                enableBox.IsChecked = true;
                frequencyBox.SelectedIndex = frequencyBox.Items.OfType<ComboBoxItem>().Select(i => i.Content.ToString())
                    .ToList()
                    .IndexOf(source.Frequency.ToString());
                intervalBox.Value = source.Interval;
                if (source.End.HasValue) {
                    endBehaviorBox.SelectedIndex = 1;
                    endDateBox.Value = source.End;
                } else if (source.Count.HasValue) {
                    endBehaviorBox.SelectedIndex = 2;
                    occurrencesBox.Value = source.Count;
                } else {
                    endBehaviorBox.SelectedIndex = 0;
                }
            } else {
                enableBox.IsChecked = false;
            }
        }

        private void Enable_Changed(object sender, RoutedEventArgs e) {
            main.IsEnabled = enableBox.IsChecked.GetValueOrDefault(false);
        }

        private void EndBehaviorChanged(object sender, SelectionChangedEventArgs e) {
            void SetStates(bool until, bool number) {
                endDatePanel.Visibility = until ? Visibility.Visible : Visibility.Collapsed;
                occurrencesPanel.Visibility = number ? Visibility.Visible : Visibility.Collapsed;
            }

            switch (endBehaviorBox.SelectedIndex) {
                case -1:
                case 0:
                    SetStates(false, false);
                    break;
                case 1:
                    SetStates(true, false);
                    break;
                case 2:
                    SetStates(false, true);
                    break;
            }
        }
    }
}
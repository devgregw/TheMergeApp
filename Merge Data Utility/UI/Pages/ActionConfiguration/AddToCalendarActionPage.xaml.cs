#region LICENSE

// Project Merge Data Utility:  AddToCalendarActionPage.xaml.cs (in Solution Merge Data Utility)
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Client;
using MergeApi.Framework.Abstractions;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages.Base;
using Merge_Data_Utility.UI.Windows;

#endregion

namespace Merge_Data_Utility.UI.Pages.ActionConfiguration {
    /// <summary>
    ///     Interaction logic for AddToCalendarActionPage.xaml
    /// </summary>
    public partial class AddToCalendarActionPage : ActionConfigurationPage {
        public AddToCalendarActionPage() : this(null) { }

        public AddToCalendarActionPage(AddToCalendarAction source) {
            InitializeComponent();
            Initialize(source);
            recurrenceField.DropDownClosed += (s, e) => {
                r2.IsChecked = false;
                r2.IsChecked = true;
            };
        }

        public override void Update() {
            if (!HasCurrentValue) return;
            var value = GetCurrentAction<AddToCalendarAction>();
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (value.ParamGroup) {
                case "1":
                    r1.IsChecked = true;
                    idBox.SetId("events", value.EventId1);
                    break;
                case "2":
                    r2.IsChecked = true;
                    titleBox.Text = value.Title2;
                    locationBox.Text = value.Location2;
                    startBox.Value = value.StartDate2;
                    endBox.Value = value.EndDate2;
                    recurrenceField.SetSource(value.RecurrenceRule2);
                    break;
            }
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public override ActionBase GetAction() {
            if (r1.IsChecked.GetValueOrDefault(false)) {
                if (!string.IsNullOrWhiteSpace(idBox.Text)) return AddToCalendarAction.FromEventId(idBox.GetId());
                DisplayErrorMessage(new[] {"No event selected."});
                return null;
            }
            if (r2.IsChecked.GetValueOrDefault(false)) {
                var errors = new List<string> {
                    string.IsNullOrWhiteSpace(titleBox.Text) ? "No title specified." : "",
                    string.IsNullOrWhiteSpace(locationBox.Text) ? "No location specified." : "",
                    !startBox.Value.HasValue ? "No start date specified." : "",
                    !endBox.Value.HasValue ||
                    endBox.Value.GetValueOrDefault(DateTime.MinValue) <
                    startBox.Value.GetValueOrDefault(DateTime.MaxValue)
                        ? "No end date specified or it is earlier than the start date."
                        : ""
                };
                errors.AddRange(recurrenceField.GetValidationErrors());
                errors.RemoveAll(string.IsNullOrWhiteSpace);
                if (!errors.Any())
                    return AddToCalendarAction.FromEventInfo(titleBox.Text, locationBox.Text, startBox.Value.Value,
                        endBox.Value.Value, recurrenceField.GetRule());
                DisplayErrorMessage(errors);
                return null;
            }
            DisplayErrorMessage(new[] {"No parameter group selected."});
            return null;
        }

        private void ChooseEvent(object sender, RoutedEventArgs args) {
            var window =
                new ObjectChooserWindow(
                    async () => (await MergeDatabase.ListAsync<MergeEvent>()).Select(e => new ListViewItem {
                        Content = $"{e.Title} (events/{e.Id})",
                        Tag = e
                    }));
            window.ShowDialog();
            if (window.ObjectSelected)
                idBox.SetId("events", window.GetSelectedObject<MergeEvent>().Id);
        }
    }
}
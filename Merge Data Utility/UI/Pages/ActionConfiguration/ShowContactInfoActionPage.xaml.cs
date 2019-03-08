#region LICENSE

// Project Merge Data Utility:  ShowContactInfoActionPage.xaml.cs (in Solution Merge Data Utility)
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

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MergeApi;
using MergeApi.Framework.Abstractions;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages.Base;
using Merge_Data_Utility.UI.Windows;
using Merge_Data_Utility.UI.Windows.Choosers;

#endregion

namespace Merge_Data_Utility.UI.Pages.ActionConfiguration {
    /// <summary>
    ///     Interaction logic for ShowContactInfoActionPage.xaml
    /// </summary>
    public partial class ShowContactInfoActionPage : ActionConfigurationPage {
        public ShowContactInfoActionPage() : this(null) { }

        public ShowContactInfoActionPage(ShowContactInfoAction source) {
            InitializeComponent();
            Initialize(source);
        }

        public override void Update() {
            mediumsList.Prepare(() => {
                var window = new ContactMediumChooser(m => true, true, new[] {""});
                window.ShowDialog();
                return window.SelectedMedium == null
                    ? null
                    : new ListViewItem {
                        Content = window.SelectedMedium.ToFriendlyString(),
                        Tag = window.SelectedMedium
                    };
            }, i => {
                return
                    MessageBox.Show("Are you sure you want to delete this contact medium?", "Confirm",
                        MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) ==
                    MessageBoxResult.Yes;
            }, i => {
                var window = new ContactMediumChooser(m => true, true, new[] {""});
                window.ShowDialog();
                return window.SelectedMedium == null
                    ? null
                    : new ListViewItem {
                        Content = window.SelectedMedium.ToFriendlyString(),
                        Tag = window.SelectedMedium
                    };
            });
            if (!HasCurrentValue) return;
            var cv = GetCurrentAction<ShowContactInfoAction>();
            switch (cv.ParamGroup) {
                case "1":
                    r1.IsChecked = true;
                    groupIdBox.SetId("groups", cv.GroupId1);
                    break;
                case "2":
                    r2.IsChecked = true;
                    nameBox.Text = cv.Name2;
                    mediumsList.SetContactMediums(cv.ContactMediums2);
                    break;
            }
        }

        public override ActionBase GetAction() {
            if (r1.IsChecked.GetValueOrDefault(false)) {
                if (!string.IsNullOrWhiteSpace(groupIdBox.Text))
                    return ShowContactInfoAction.FromGroupId(groupIdBox.GetId());
                DisplayErrorMessage(new[] {"No group selected."});
                return null;
            }
            if (r2.IsChecked.GetValueOrDefault(false)) {
                if (mediumsList.Count != 0 && !string.IsNullOrWhiteSpace(nameBox.Text))
                    return ShowContactInfoAction.FromContactMediums(nameBox.Text, mediumsList.GetContactMediums());
                DisplayErrorMessage(new[] {
                    string.IsNullOrWhiteSpace(nameBox.Text) ? "No name specified." : "",
                    mediumsList.Count == 0 ? "No contact mediums specified." : ""
                });
                return null;
            }
            DisplayErrorMessage(new[] {"No parameter group selected."});
            return null;
        }

        private void ChooseGroup(object sender, RoutedEventArgs args) {
            var window =
                new ObjectChooserWindow(
                    async () => (await MergeDatabase.ListAsync<MergeGroup>()).Select(e => new ListViewItem {
                        Content = $"{e.Name} (groups/{e.Id})",
                        Tag = e
                    }));
            window.ShowDialog();
            if (window.ObjectSelected)
                groupIdBox.SetId("groups", window.GetSelectedObject<MergeGroup>().Id);
        }

        private void ChooseLeader(object sender, RoutedEventArgs args) {
            MessageBox.Show("Currently not supported");
            /*var window =
                new ObjectChooserWindow(
                    async () => (await MergeDatabase.ListLeadersAsync()).Items.Select(e => new ListViewItem {
                        Content = $"{e.Name} (leaders/{e.Id})",
                        Tag = e
                    }));
            window.ShowDialog();
            if (window.ObjectSelected)
                leaderIdBox.SetId("leaders", window.GetSelectedObject<MergeLeader>().Id);*/
        }
    }
}
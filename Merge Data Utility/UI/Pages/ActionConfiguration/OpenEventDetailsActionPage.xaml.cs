#region LICENSE

// Project Merge Data Utility:  OpenEventDetailsActionPage.xaml.cs (in Solution Merge Data Utility)
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

#endregion

namespace Merge_Data_Utility.UI.Pages.ActionConfiguration {
    /// <summary>
    ///     Interaction logic for OpenEventDetailsActionPage.xaml
    /// </summary>
    public partial class OpenEventDetailsActionPage : ActionConfigurationPage {
        public OpenEventDetailsActionPage() : this(null) { }

        public OpenEventDetailsActionPage(OpenEventDetailsAction source) {
            InitializeComponent();
            Initialize(source);
        }

        public override void Update() {
            if (!HasCurrentValue) return;
            idBox.SetId("events", GetCurrentAction<OpenEventDetailsAction>().EventId1);
        }

        public override ActionBase GetAction() {
            if (!string.IsNullOrWhiteSpace(idBox.Text)) return OpenEventDetailsAction.FromEventId(idBox.GetId());
            DisplayErrorMessage(new[] {"No event selected."});
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
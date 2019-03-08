#region LICENSE

// Project Merge Data Utility:  GetDirectionsActionPage.xaml.cs (in Solution Merge Data Utility)
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

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MergeApi;
using MergeApi.Framework.Abstractions;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using MergeApi.Tools;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages.Base;
using Merge_Data_Utility.UI.Windows;

#endregion

namespace Merge_Data_Utility.UI.Pages.ActionConfiguration {
    /// <summary>
    ///     Interaction logic for GetDirectionsActionPage.xaml
    /// </summary>
    public partial class GetDirectionsActionPage : ActionConfigurationPage {
        public GetDirectionsActionPage() : this(null) { }

        public GetDirectionsActionPage(GetDirectionsAction source) {
            InitializeComponent();
            Initialize(source);
        }

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        public override void Update() {
            if (!HasCurrentValue) return;
            var cv = GetCurrentAction<GetDirectionsAction>();
            switch (cv.ParamGroup) {
                case "1":
                    r1.IsChecked = true;
                    eventIdBox.SetId("events", cv.EventId1);
                    break;
                case "2":
                    r2.IsChecked = true;
                    groupIdBox.SetId("groups", cv.GroupId2);
                    break;
                case "3":
                    r3.IsChecked = true;
                    addressBox.Text = cv.Address3;
                    break;
                case "4":
                    r4.IsChecked = true;
                    lat.Value = cv.Coordinates4.Latitude;
                    lng.Value = cv.Coordinates4.Longitude;
                    break;
            }
        }

        public override ActionBase GetAction() {
            if (r1.IsChecked.GetValueOrDefault(false)) {
                if (!string.IsNullOrWhiteSpace(eventIdBox.Text))
                    return GetDirectionsAction.FromEventId(eventIdBox.GetId());
                DisplayErrorMessage(new[] {"No event selected."});
                return null;
            }
            if (r2.IsChecked.GetValueOrDefault(false)) {
                if (!string.IsNullOrWhiteSpace(groupIdBox.Text))
                    return GetDirectionsAction.FromGroupId(groupIdBox.GetId());
                DisplayErrorMessage(new[] {"No group selected"});
                return null;
            }
            if (r3.IsChecked.GetValueOrDefault(false)) {
                if (!string.IsNullOrWhiteSpace(addressBox.Text))
                    return GetDirectionsAction.FromAddress(addressBox.Text);
                DisplayErrorMessage(new[] {"No address specified."});
                return null;
            }
            if (r4.IsChecked.GetValueOrDefault(false)) {
                if (lat.Value.HasValue && lng.Value.HasValue)
                    return GetDirectionsAction.FromCoordinates(new CoordinatePair(lat.Value.Value, lng.Value.Value));
                DisplayErrorMessage(new[] {"The specified coordinate pair is invalid."});
                return null;
            }
            DisplayErrorMessage(new[] {"No parameter group selected."});
            return null;
        }

        private void ChooseEvent(object sender, RoutedEventArgs args) {
            var window =
                new ObjectChooserWindow(
                    async () =>
                        (await MergeDatabase.ListAsync<MergeEvent>()).Where(
                            e => !string.IsNullOrWhiteSpace(e.Address)).Select(e => new ListViewItem {
                            Content = $"{e.Title} (events/{e.Id})",
                            Tag = e
                        }));
            window.ShowDialog();
            if (window.ObjectSelected)
                eventIdBox.SetId("events", window.GetSelectedObject<MergeEvent>().Id);
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

        private async void Geocode(object sender, RoutedEventArgs e) {
            var reference = new LoaderReference(geocodeLoader);
            var input = TextInputWindow.GetInput("Address", "To get geographical coordinates, type an address:",
                scope: InputScopeNameValue.PostalAddress);
            if (string.IsNullOrWhiteSpace(input))
                return;
            reference.StartLoading("Geocoding...");
            IsEnabled = false;
            var info = await GoogleMapsGeocoding.GetCoordinates(input);
            if (info == null) {
                MessageBox.Show("No results were found for that address.", "Geocode Error",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            } else {
                lat.Value = info.Item2.Latitude;
                lng.Value = info.Item2.Longitude;
                geocodeBlock.Text = info.Item1;
            }
            IsEnabled = true;
            reference.StopLoading();
        }
    }
}
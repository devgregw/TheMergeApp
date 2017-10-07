#region LICENSE

// Project Merge.iOS:  SettingsPage.xaml.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/07/2017 at 10:27 AM.
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
using Firebase.CloudMessaging;
using Foundation;
using Merge.Classes.Helpers;
using Merge.Classes.UI.Pages;
using MergeApi.Tools;
using UIKit;
using Xamarin.Forms;

#endregion

namespace Merge.Classes.UI {
    public partial class SettingsPage : ContentPage {
        private bool _initializing;

        public SettingsPage() {
            _initializing = true;
            InitializeComponent();
            Title = "Settings";
            BackgroundColor = Color.FromHex(ColorConsts.PrimaryLightColor);
            cachingCell.On = PreferenceHelper.Caching;
            leaderFeaturesCell.On = PreferenceHelper.LeaderFeaturesEnabled;
            invalidObjectsCell.On = PreferenceHelper.ShowInvalidObjects;
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                extrasSection.Add(new ViewCell {
                    View = new Button {
                        Text = "More Settings"
                    }.Manipulate(b => {
                        b.Clicked +=
                            (s, e) => UIApplication.SharedApplication.OpenUrl(NSUrl.FromString(UIApplication
                                .OpenSettingsUrlString));
                        return b;
                    })
                });
            _initializing = false;
        }

        private async void ManageFilters_Clicked(object sender, EventArgs e) {
            await Navigation.PushModalAsync(new TargetingPage().WrapInNavigationPage());
        }

        private void ShowSimpleDialog(string title, string message, string[] buttons, Action[] buttonClicks) {
            AlertHelper.ShowAlert(title, message, b => buttonClicks[Array.IndexOf(buttons, b)](), null, buttons);
        }

        private void SetLeaderFeaturesCellSafely(bool value) {
            _initializing = true;
            leaderFeaturesCell.On = value;
            PreferenceHelper.LeaderFeaturesEnabled = value;
            if (!value) {
                PreferenceHelper.Token = "";
                PreferenceHelper.TokenExpiration = DateTime.MinValue;
                PreferenceHelper.LeaderUsername = "";
                PreferenceHelper.LeaderPassword = "";
                PreferenceHelper.AuthenticationState = PreferenceHelper.LeaderAuthenticationState.NoAttempt;
                Messaging.SharedInstance.Unsubscribe("/topics/verified_leader");
            }
            _initializing = false;
        }

        private void ShowLeaderDialog() {
            ShowSimpleDialog("Are You a Leader?",
                "Leaders have special access to protected features.  If you are a leader and have been given a username and password, tap 'Yes'.  Otherwise, tap 'No'.",
                new[] {"No", "Yes"},
                new Action[] {() => SetLeaderFeaturesCellSafely(false), ShowLeaderAuthenticationPage});
        }

        private async void ShowLeaderAuthenticationPage() {
            await Navigation.PushModalAsync(new LeaderAuthenticationPage(SetLeaderFeaturesCellSafely)
                .WrapInNavigationPage());
        }

        private void LeaderFeatures_Changed(object sender, ToggledEventArgs e) {
            if (!_initializing)
                if (leaderFeaturesCell.On) ShowLeaderDialog();
                else
                    SetLeaderFeaturesCellSafely(false);
        }

        private void Caching_Changed(object sender, ToggledEventArgs e) {
            PreferenceHelper.Caching = cachingCell.On;
        }

        private void ShowInvalidObjects_Changed(object sender, ToggledEventArgs e) {
            PreferenceHelper.ShowInvalidObjects = invalidObjectsCell.On;
        }

        private void ClearTips_Clicked(object sender, EventArgs e) {
            PreferenceHelper.DismissedTips = new string[] { };
            AlertHelper.ShowAlert("Tips Cleared", "", delegate { }, "Dismiss");
        }
    }
}
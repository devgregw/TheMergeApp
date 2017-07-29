#region LICENSE

// Project Merge.iOS:  LeaderAuthenticationPage.xaml.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/07/2017 at 11:27 AM.
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
using System.Threading.Tasks;
using Firebase.CloudMessaging;
using Merge.Classes.Helpers;
using Merge.iOS;
using MergeApi.Client;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

#endregion

namespace Merge.Classes.UI.Pages {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LeaderAuthenticationPage : ContentPage {
        private Action<bool> _callback;

        public LeaderAuthenticationPage(Action<bool> callback) {
            InitializeComponent();
            Title = "Leader Verification";
            _callback = callback;
        }

        public LeaderAuthenticationPage() : this(b => { }) { }

        private async void Back_Clicked(object sender, EventArgs e) {
            PreferenceHelper.Token = "";
            PreferenceHelper.TokenExpiration = DateTime.MinValue;
            PreferenceHelper.LeaderUsername = "";
            PreferenceHelper.LeaderPassword = "";
            PreferenceHelper.AuthenticationState = PreferenceHelper.LeaderAuthenticationState.NoAttempt;
            PreferenceHelper.LeaderFeaturesEnabled = false;
            Messaging.SharedInstance.Unsubscribe("/topics/verified_leader");
            await Navigation.PopModalAsync();
            _callback(false);
        }

        private async Task SetView(bool loading) {
            await Content.FadeTo(0d);
            credentialsLayout.IsVisible = !loading;
            indicator.IsVisible = loading;
            await Content.FadeTo(1d);
        }

        private async void SignIn_Clicked(object sender, EventArgs e) {
            UIApplication.SharedApplication.KeyWindow.EndEditing(true);
            await SetView(true);
            try {
                string u = emailAddress.Text, p = password.Text;
                AppDelegate.AuthLink = await MergeDatabase.AuthenticateAsync(u, p);
                PreferenceHelper.LeaderFeaturesEnabled = true;
                PreferenceHelper.Token = AppDelegate.AuthLink.FirebaseToken;
                PreferenceHelper.TokenExpiration =
                    AppDelegate.AuthLink.Created.AddSeconds(AppDelegate.AuthLink.ExpiresIn);
                PreferenceHelper.AuthenticationState = PreferenceHelper.LeaderAuthenticationState.Successful;
                PreferenceHelper.LeaderUsername = u;
                PreferenceHelper.LeaderPassword = p;
                Messaging.SharedInstance.Subscribe("/topics/verified_leader");
                await Navigation.PopModalAsync();
                _callback(true);
            } catch {
                emailAddress.Text = "";
                password.Text = "";
                await SetView(false);
                AlertHelper.ShowAlert("Invalid Credentials",
                    "The given credentials are invalid.  Try again or tap 'Back'.",
                    b => { }, "OK");
                PreferenceHelper.AuthenticationState = PreferenceHelper.LeaderAuthenticationState.Failed;
            }
        }
    }
}
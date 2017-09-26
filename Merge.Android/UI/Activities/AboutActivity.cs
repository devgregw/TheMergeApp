#region LICENSE

// Project Merge.Android:  AboutActivity.cs (in Solution Merge.Android)
// Created by Greg Whatley on 06/30/2017 at 9:18 AM.
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
using System.Diagnostics.CodeAnalysis;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Merge.Android.Helpers;
using MergeApi.Models.Actions;
using MergeApi.Tools;
using Toolbar = Android.Support.V7.Widget.Toolbar;

#endregion

namespace Merge.Android.UI.Activities {
    [Activity(Label = "About Merge", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public class AboutActivity : AppCompatActivity {
        [BindView(Resource.Id.aboutVersion)] private TextView _aboutVersion;

        [OnClick(Resource.Id.aboutDevgregw)]
        public void AboutDevgregw_OnClick(object sender, EventArgs e) => LaunchUriAction
            .FromUri("https://devgregw.com").Invoke();

        [OnClick(Resource.Id.aboutPantego)]
        public void AboutPantego_OnClick(object sender, EventArgs e) => LaunchUriAction
            .FromUri("http://pantego.org").Invoke();

        [OnClick(Resource.Id.aboutLicenses)]
        public void AboutLicenses_OnClick(object sender, EventArgs e) => LaunchUriAction
            .FromUri("https://merge.devgregw.com/licenses").Invoke();

        [OnClick(Resource.Id.aboutSendFeedback)]
        public void AboutSendFeedback_OnClick(object sender, EventArgs e) => EmailAction
            .FromAddress("devgregw@outlook.com").Invoke();

        [OnClick(Resource.Id.viewRoadmap)]
        public void ViewRoadmap_OnClick(object sender, EventArgs e) => LaunchUriAction
            .FromUri("https://trello.com/b/nAzvRa7R/roadmap").Invoke();

        public override bool OnOptionsItemSelected(IMenuItem item) {
            if (item.ItemId != global::Android.Resource.Id.Home) return base.OnOptionsItemSelected(item);
            OnBackPressed();
            return true;
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AboutActivity);
            Cheeseknife.Bind(this);
            if (SdkChecker.KitKat)
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            _aboutVersion.Text =
                $"Version {VersionConsts.Version} (update {VersionConsts.Update}) {VersionConsts.Classification}\nUtilizing MergeApi version {VersionInfo.VersionString} (update {VersionInfo.Update})";
        }
    }
}
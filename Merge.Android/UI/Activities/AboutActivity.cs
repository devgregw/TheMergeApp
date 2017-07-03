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

using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Merge.Android.Helpers;
using MergeApi.Models.Actions;
using MergeApi.Tools;
using Toolbar = Android.Support.V7.Widget.Toolbar;

#endregion

namespace Merge.Android.UI.Activities {
    [Activity(Label = "About Merge")]
    public class AboutActivity : AppCompatActivity, View.IOnClickListener {
        public void OnClick(View v) {
            switch (v.Id) {
                case Resource.Id.aboutDevgregw:
                    LaunchUriAction.FromUri("http://www.devgregw.com").Invoke();
                    break;
                case Resource.Id.aboutPantego:
                    LaunchUriAction.FromUri("http://www.pantego.org").Invoke();
                    break;
                case Resource.Id.aboutLicenses:
                    LaunchUriAction.FromUri("https://api.mergeonpoint.com/merge.android.html").Invoke();
                    break;
                case Resource.Id.aboutSendFeedback:
                    EmailAction.FromAddress("devgregw@outlook.com").Invoke();
                    break;
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            if (item.ItemId != global::Android.Resource.Id.Home) return base.OnOptionsItemSelected(item);
            OnBackPressed();
            return true;
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AboutActivity);
            FindViewById<Button>(Resource.Id.aboutDevgregw).SetOnClickListener(this);
            FindViewById<Button>(Resource.Id.aboutPantego).SetOnClickListener(this);
            FindViewById<Button>(Resource.Id.aboutLicenses).SetOnClickListener(this);
            FindViewById<Button>(Resource.Id.aboutSendFeedback).SetOnClickListener(this);
            if (SdkChecker.KitKat)
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            FindViewById<TextView>(Resource.Id.aboutVersion).Text =
                $"Version {VersionConsts.Version} (update {VersionConsts.Update}) {VersionConsts.Classification}\nUtilizing MergeApi version {VersionInfo.VERSION_STRING} (update {VersionInfo.Update})";
        }
    }
}
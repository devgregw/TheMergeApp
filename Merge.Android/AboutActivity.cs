using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Merge.Android.Classes.Helpers;
using MergeApi.Models.Actions;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Merge.Android {
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
                $"Version {VersionConsts.Version} (update {VersionConsts.Update}) {VersionConsts.Classification}\nUtilizing MergeApi version {MergeApi.Tools.VersionInfo.VERSION_STRING} (update {MergeApi.Tools.VersionInfo.Update})";
        }
    }
}
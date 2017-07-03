#region LICENSE

// Project Merge.Android:  AttendanceManagerActivity.cs (in Solution Merge.Android)
// Created by Greg Whatley on 06/30/2017 at 9:20 AM.
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
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Merge.Android.Helpers;
using Merge.Android.UI.Fragments;
using Newtonsoft.Json;

#endregion

namespace Merge.Android.UI.Activities.LeadersOnly {
    [Activity(Label = "Attendance Manager",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class AttendanceManagerActivity : AppCompatActivity {
        private bool _done;

        private AttendanceListFragment _fragment;
        public IMenu Menu { get; private set; }

        public override bool OnPrepareOptionsMenu(IMenu menu) {
            Menu = menu;
            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            switch (item.ItemId) {
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                case 12345:
                    var intent = new Intent(this, typeof(AttendanceGroupEditorActivity));
                    intent.PutExtra("groupJson", JsonConvert.SerializeObject(_fragment.SelectedGroup));
                    StartActivity(intent);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnBackPressed() {
            if (!_fragment.GoBack())
                Finish();
        }

        protected override void OnResumeFragments() {
            base.OnResumeFragments();
            if (_done)
                _fragment.OnResume();
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AttendanceManagerActivity);
            if (SdkChecker.KitKat)
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            _fragment = new AttendanceListFragment(this);
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.container, _fragment)
                .Commit();
            _done = true;
        }
    }
}
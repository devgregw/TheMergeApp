#region LICENSE

// Project Merge.Android:  SettingsActivity.cs (in Solution Merge.Android)
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
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Preferences;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Merge.Android.Helpers;
using AlertDialog = Android.Support.V7.App.AlertDialog;

#endregion

namespace Merge.Android.UI.Activities {
    /// <summary>
    ///     Activity for managing preferences
    /// </summary>
    [Activity(Label = "Settings", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class SettingsActivity : AppCompatActivity {
        private PreferenceHelper.PreferenceChangeListener _listener;

        protected override void OnCreate(Bundle savedInstanceState) {
            SetTheme(Resource.Style.AppTheme);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Settings);
            if (SdkChecker.KitKat)
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            LogHelper.WriteMessage("INFO", "SettingsActivity opened");
            _listener = new PreferenceHelper.PreferenceChangeListener(this);
            PreferenceManager.GetDefaultSharedPreferences(this).RegisterOnSharedPreferenceChangeListener(_listener);
            FragmentManager.BeginTransaction().Replace(Resource.Id.container, new MergePreferenceFragment()).Commit();
        }

        protected override void OnStart() {
            base.OnStart();
            PreferenceManager.GetDefaultSharedPreferences(this).RegisterOnSharedPreferenceChangeListener(_listener);
        }

        protected override void OnStop() {
            base.OnStop();
            LogHelper.WriteMessage("INFO", "SettingsActivity stopping");
            PreferenceManager.GetDefaultSharedPreferences(this).UnregisterOnSharedPreferenceChangeListener(_listener);
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            switch (item.ItemId) {
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                case Resource.Id.MenuShowSetup:
                    StartActivity(typeof(WelcomeActivity));
                    return true;
                case Resource.Id.MenuClearTips:
                    PreferenceHelper.DismissedTips = new string[] { };
                    new AlertDialog.Builder(this).SetMessage("Tips have been reset.").SetPositiveButton("OK",
                        (s, e) => { }).Show();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override bool OnPrepareOptionsMenu(IMenu menu) {
            menu.Clear();
            MenuInflater.Inflate(Resource.Menu.SettingsMenu, menu);
            return base.OnPrepareOptionsMenu(menu);
        }

        /// <summary>
        ///     Dedicated, nested class for managing Merge preferences
        /// </summary>
        public class MergePreferenceFragment : PreferenceFragment {
            private void InitializePreferences() {
                PreferenceScreen?.RemoveAll();
                AddPreferencesFromResource(Resource.Xml.Preferences);
                    PreferenceScreen?.RemovePreference(PreferenceScreen.FindPreference("secret"));
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
                var view = base.OnCreateView(inflater, container, savedInstanceState);
                view.SetBackgroundColor(Color.White);
                return view;
            }

            public override void OnStart() {
                base.OnStart();
                InitializePreferences();
            }

            public override void OnCreate(Bundle savedInstanceState) {
                base.OnCreate(savedInstanceState);
                LogHelper.WriteMessage("INFO", "PreferenceFragment initialized");
            }
        }
    }
}
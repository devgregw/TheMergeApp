#region LICENSE

// Project Merge.Android:  LeaderResourcesActivity.cs (in Solution Merge.Android)
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Merge.Android.Helpers;
using Merge.Android.Receivers;
using Merge.Android.UI.Views;
using MergeApi;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Core;
using MergeApi.Tools;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Utilities = Merge.Android.Helpers.Utilities;

#endregion

namespace Merge.Android.UI.Activities.LeadersOnly {
    [Activity(Label = "Leader Resources", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public class LeaderResourcesActivity : AppCompatActivity {
        private ViewApplier _applier;
        private bool _first = true;

        [BindView(Resource.Id.content_list)] private LinearLayout _mainList;

        private IEnumerable<MergePage> _pages;

        [OnClick(Resource.Id.resourcesAttendance)]
        private void AttendanceButton_OnClick(object sender, EventArgs e) => StartActivity(
            typeof(AttendanceManagerActivity));

        private void Nullify() => _pages = null;

        private async void LoadData() {
            if (_pages == null) {
                // Load data from the database if we haven't already (this will also return null if the user tapped refresh: see Nullify())
                _applier.ApplyLoadingCard(!_first);
                _first = false;
                try {
                    await Task.Run(async () => _pages = await MergeDatabase.ListAsync<MergePage>());
                } catch (Exception e) {
                    _applier.Apply(new BasicCard(this, e), false);
                    return;
                }
            }
            Task<(MergePage Object, ValidationResult Result)>[] validations = _pages
                .Where(p => p.LeadersOnly && Utilities.IfRelease(!p.Hidden, true))
                .Select(async o => (o,
                    await o.ValidateAsync())).ToArray();
            var filtered = (await Task.WhenAll(validations)).Where(o =>
                Utilities.IfRelease(
                    PreferenceHelper.ShowInvalidObjects || o.Result == null ||
                    o.Result.ResultType == ValidationResultType.Success, true));
            var content = filtered.Select(p => new DataCard(this, p.Object, p.Result)).ToList();
            if (content.Count > 0) {
                _applier.Apply(content, !_first);
            } else {
                var layoutParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                    ViewGroup.LayoutParams.WrapContent);
                layoutParams.AddRule(LayoutRules.CenterHorizontal);
                _applier.Apply(new BasicCard(this,
                    new IconView(this, Resource.Drawable.NoContent, "No content available", true, true) {
                        LayoutParameters = layoutParams
                    }), !_first);
            }
            _first = false;
        }

        private void ShowUnauthorizedDialog() {
            var dialog = new AlertDialog.Builder(this).SetTitle("Unauthorized")
                .SetMessage(
                    "You are not a verified leader, so you do not have permission to access these resources.  To verify your leader status, tap 'Settings' and then check 'Enable leader-only features'.")
                .SetCancelable(false)
                .SetPositiveButton("Close", (s, e) => Finish())
                .SetNegativeButton("Settings", (s, e) => {
                    StartActivity(typeof(SettingsActivity));
                    ShowUnauthorizedDialog();
                }).Create();
            dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
            dialog.Show();
        }

        public override bool OnCreateOptionsMenu(IMenu menu) {
            MenuInflater.Inflate(Resource.Menu.mainMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            switch (item.ItemId) {
                case Resource.Id.action_refresh:
                    FindViewById<AppBarLayout>(Resource.Id.appBarLayout).SetExpanded(true, true);
                    Nullify();
                    LoadData();
                    return true;
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        protected override void OnResume() {
            ((MergeActionInvocationReceiver) MergeDatabase.ActionInvocationReceiver).SetContext(this);
            base.OnResume();
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.LeaderResourcesActivity);
            Cheeseknife.Bind(this);
            if (SdkChecker.KitKat)
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            _applier = new ViewApplier(this, _mainList);
            if (!PreferenceHelper.IsValidLeader)
                ShowUnauthorizedDialog();
            else LoadData();
        }
    }
}
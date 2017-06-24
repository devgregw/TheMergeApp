using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.View.Menu;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Merge.Android.Classes.Controls;
using MergeApi.Models.Core;
using MergeApi.Models.Core.Tab;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Merge.Android.Classes.Helpers;
using MergeApi.Client;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Orientation = Android.Content.Res.Orientation;

namespace Merge.Android {
    [Activity(Label = "Leader Resources", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class LeaderResourcesActivity : AppCompatActivity, View.IOnClickListener {
        private LinearLayout _mainList;
        private ViewApplier _applier;
        private IEnumerable<MergePage> _pages;
        private Toolbar _toolbar;
        private Button _attendanceButton;
        private bool _first = true;

        private void Nullify() {
            _pages = null;
        }

        private async void LoadData() {
            if (_pages == null) { // Load data from the database if we haven't already (this will also return null if the user tapped refresh: see Nullify())
                _applier.ApplyLoadingCard(!_first);
                _first = false;
                try {
                    await Task.Run(async () => _pages = await MergeDatabase.ListAsync<MergePage>());
                } catch (Exception e) {
                    _applier.Apply(new BasicCard(this, e), false);
                    return;
                }
            }
            var filtered = _pages.Where(p => p.LeadersOnly).ToList();
            var content = filtered.Select(p => new DataCard(this, p)).ToList();
            if (content.Count > 0)
                _applier.Apply(content, !_first);
            else {
                var layoutParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                    ViewGroup.LayoutParams.WrapContent);
                layoutParams.AddRule(LayoutRules.CenterHorizontal);
                _applier.Apply(new BasicCard(this, new IconView(this, Resource.Drawable.NoContent, "No content available", true, true) {
                    LayoutParameters = layoutParams
                }), !_first);
            }
            _first = false;
        }

        private void ShowUnauthorizedDialog() {
            var dialog = new AlertDialog.Builder(this).SetTitle("Unauthorized").SetMessage("You are not a verified leader, so you do not have permission to access these resources.  To verify your leader status, tap 'Settings' and then check 'Enable leader-only features'.").SetCancelable(false)
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
            ((MergeActionInvocationReceiver)MergeDatabase.ActionInvocationReceiver).SetContext(this);
            base.OnResume();
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.LeaderResourcesActivity);
            if (SdkChecker.KitKat)
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            _toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(_toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            _attendanceButton = FindViewById<Button>(Resource.Id.resourcesAttendance);
            _attendanceButton.SetOnClickListener(this);
            _mainList = FindViewById<LinearLayout>(Resource.Id.content_list);
            _applier = new ViewApplier(this, _mainList);
            if (!PreferenceHelper.IsValidLeader)
                ShowUnauthorizedDialog();
            else {
                LoadData();
            }
        }

        public void OnClick(View v) {
            if (v.Id == _attendanceButton.Id)
                StartActivity(typeof(AttendanceManagerActivity));
        }
    }
}
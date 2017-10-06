#region LICENSE

// Project Merge.Android:  MainActivity.cs (in Solution Merge.Android)
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Locations;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Com.Nostra13.Universalimageloader.Core;
using Com.Nostra13.Universalimageloader.Core.Display;
using Merge.Android.Helpers;
using Merge.Android.Receivers;
using Merge.Android.UI.Activities.LeadersOnly;
using Merge.Android.UI.Views;
using MergeApi.Client;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Interfaces;
using MergeApi.Models.Actions;
using MergeApi.Models.Core.Tab;
using MergeApi.Tools;
using appcompat7 = Android.Support.V7.App;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Utilities = Merge.Android.Helpers.Utilities;

#endregion

namespace Merge.Android.UI.Activities {
#pragma warning disable 612, 618
    /// <summary>
    ///     The main activity
    /// </summary>
    [Activity(Label = "Merge", MainLauncher = true, Icon = "@mipmap/ic_launcher",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    [IntentFilter(new[] {"android.intent.action.MAIN"}, Categories = new[] {"android.intent.category.LAUNCHER"})]
    public class MainActivity : appcompat7.AppCompatActivity, ViewSwitcher.IViewFactory {
        private const int WelcomeActivityRequestCode = 63743;

        private const int TabHome = 0, TabEvents = 1, TabGroups = 2;
        //private const int HeaderInterval = 8000;
        //private const int HeaderAnimationDuration = 1000;

        private static GoogleApiClient _playServices;

        [BindView(Resource.Id.appBarLayout)] private AppBarLayout _appBar;

        /*private readonly Dictionary<int, string> _images = new Dictionary<int, string> {
            {Resource.Drawable.LargeHeader, "The Merge App"},
            {Resource.Drawable.header1, "RISE: Student Camp 2016"},
            {Resource.Drawable.header2, "PAUSE: DNOW 2016"},
            {Resource.Drawable.header3, "Michael McAndrew"}
        };*/

        private ViewApplier _applier;

        //private int _currentHeader = -1;
        [BindView(Resource.Id.drawerLayout)] private DrawerLayout _drawerLayout;

        private bool /*_headerRunning = true,*/
            _first = true;

        private Dictionary<int, TabHeader> _headers;

        [BindView(Resource.Id.content_list)] private LinearLayout _mainList;

        [BindView(Resource.Id.navView)] private NavigationView _navView;

        private int _selectedTab = -1; // See consts above
        private List<TabTip> _tips;

        [BindView(Resource.Id.toolbar)] private Toolbar _toolbar;

        public View MakeView() {
            var view = new ImageView(this);
            view.SetScaleType(ImageView.ScaleType.CenterCrop);
            view.LayoutParameters =
                new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            return view;
        }

        private void Nullify() {
            DataCache.Events = null;
            DataCache.Groups = null;
            DataCache.Pages = null;
            _headers = new Dictionary<int, TabHeader>();
            _tips = new List<TabTip>();
        }

        private Tab GetTabForInt(int t) => t == TabHome
            ? Tab.Home
            : t == TabEvents
                ? Tab.Events
                : Tab.Groups;

        private int GetDrawerItemForTab(int tab) =>
            tab == 0
                ? Resource.Id.drawerItemHome
                : tab == 1
                    ? Resource.Id.drawerItemEvents
                    : Resource.Id.drawerItemGroups;

        private ImageView CreateHeaderImageView(string image) {
            if (string.IsNullOrWhiteSpace(image))
                return null;
            var view = new ImageView(this) {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                    Resources.GetDimensionPixelSize(Resource.Dimension.TwoHundredDp))
            };
            if (SdkChecker.Lollipop)
                view.Elevation = Resources.GetDimension(Resource.Dimension.DefaultElevation);
            view.SetScaleType(ImageView.ScaleType.CenterCrop);
            Utilities.LoadImageForDisplay(image, view);
            return view;
        }

        private async Task HandleTab<T, TSortBy>(Func<IEnumerable<T>> variableGetter,
            Action<IEnumerable<T>> variableSetter,
            int tab, Func<T, TSortBy> sorter, Func<T, bool> filter, Func<(T Object, ValidationResult Result), View> viewCreator) where T : class, IIdentifiable {
            _navView.SetCheckedItem(GetDrawerItemForTab(tab));
            if (variableGetter() == null) {
                // Load data from the database if we haven't already (variableGetter() will also return null if the user tapped refresh: see Nullify())
                _applier.ApplyLoadingCard(!_first);
                _first = false;
                try {
                    await Task.Run(async () => variableSetter(await MergeDatabase.ListAsync<T>()));
                } catch (Exception e) {
                    _applier.Apply(new BasicCard(this, e), !_first);
                    return;
                }
            }
            if (_tips == null || _tips.Count == 0) {
                _applier.ApplyLoadingCard(!_first);
                _first = false;
                try {
                    await Task.Run(async () => _tips = (await MergeDatabase.ListAsync<TabTip>()).ToList());
                } catch {
                    _tips = new List<TabTip>();
                }
            }
            if (!_headers.ContainsKey(tab)) {
                _applier.ApplyLoadingCard(!_first);
                _first = false;
                try {
                    TabHeader info = null;
                    await Task.Run(async () => info = await MergeDatabase.GetAsync<TabHeader>(tab == TabHome
                        ? "Home"
                        : tab == TabEvents
                            ? "Events"
                            : "Groups"));
                    _headers[tab] = info;
                } catch (Exception e) {
                    _headers[tab] = null;
                    LogHelper.WriteException(e, false, null);
                }
            }
            if (_selectedTab == tab) {
                // If the user switched tabs, don't replace the content they're already viewing
                Task<(T Object, ValidationResult Result)>[] validations = variableGetter().Where(filter).OrderBy(sorter)
                    .Select(async o => (o,
                        o is IValidatable ? await ((IValidatable) o).ValidateAsync() : null)).ToArray();
                var filtered = (await Task.WhenAll(validations)).Where(t => Utilities.IfRelease(PreferenceHelper.ShowInvalidObjects || t.Result == null ||
                                    t.Result.ResultType == ValidationResultType.Success, true)).ToList();
                var tips = _tips.Where(t => t.Tab == GetTabForInt(tab) &&
                                            t.CheckTargeting(PreferenceHelper.GradeLevels, PreferenceHelper.Genders) &&
                                            !PreferenceHelper.DismissedTips.Contains(t.Id));
                var content = tips.Select(t => new TipCard(this, t)).Concat(filtered.Select(o => viewCreator((o.Object, o.Result))))
                    .ToList();
                var image = _headers.ContainsKey(tab) && _headers[tab] != null
                    ? CreateHeaderImageView(_headers[tab].Image)
                    : null;
                if (filtered.Count == 0) {
                    var layoutParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                        ViewGroup.LayoutParams.WrapContent);
                    layoutParams.AddRule(LayoutRules.CenterHorizontal);
                    content.Add(new BasicCard(this,
                        new IconView(this, Resource.Drawable.NoContent,
                            "No content available", true, true) {
                            LayoutParameters = layoutParams
                        }));
                }
                _applier.Apply(image, content, !_first);
            }
        }

        private async void SelectTab(int tab, bool force) {
            _appBar.SetExpanded(true, true);
            if (!force && tab == _selectedTab
            ) // If force is false, don't do anything if the user selected the same tab (if force is true, execution will continue normally even if _selectedTab == tab)
                return;
            _selectedTab = tab;
            switch (tab) {
                case TabHome:
                    await HandleTab(() => DataCache.Pages, v => DataCache.Pages = v, TabHome, p => p.Importance,
                        p => !p.LeadersOnly && Utilities.IfRelease(!p.Hidden, true) &&
                             p.CheckTargeting(PreferenceHelper.GradeLevels, PreferenceHelper.Genders),
                        p => new DataCard(this, p.Object, p.Result));
                    break;
                case TabEvents:
                    // ReSharper disable once PossibleInvalidOperationException
                    await HandleTab(() => DataCache.Events, v => DataCache.Events = v, TabEvents,
                        e => e.NextStartDate,
                        e => e.CheckTargeting(PreferenceHelper.GradeLevels, PreferenceHelper.Genders),
                        e => new DataCard(this, e.Object, e.Result));
                    break;
                case TabGroups:
                    await HandleTab(() => DataCache.Groups, v => DataCache.Groups = v, TabGroups, g => {
                            Location location = null;
                            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) ==
                                Permission.Granted ||
                                ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) ==
                                Permission.Granted)
                                location = LocationServices.FusedLocationApi.GetLocationAvailability(_playServices)
                                    .IsLocationAvailable
                                    ? LocationServices.FusedLocationApi.GetLastLocation(_playServices)
                                    : null;
                            if (location == null)
                                LogHelper.WriteMessage("WARN",
                                    "Location services unavailable; cannot sort Merge Groups");
                            return location == null
                                ? g.Id
                                : CoordinatePair
                                    .GetDistanceBetween(
                                        new CoordinatePair(Convert.ToDecimal(location.Latitude),
                                            Convert.ToDecimal(location.Longitude)), g.Coordinates)
                                    .ToString(CultureInfo.CurrentUICulture);
                        },
                        g => true, g => new DataCard(this, g.Object));
                    break;
            }
        }

        /*public override void Finish() {
            _headerRunning = false;
            base.Finish();
        }*/

        private void InitializeHeaderImageSwitcher() {
            var switcher = (ImageSwitcher) ((RelativeLayout) _navView.GetHeaderView(0)).GetChildAt(0);
            var caption = (TextView) ((RelativeLayout) _navView.GetHeaderView(0)).GetChildAt(1);
            switcher.SetFactory(this);
            switcher.SetImageResource(Resource.Drawable.LargeHeader);
            caption.Text = "The Merge App";
            /*Animation @in = AnimationUtils.LoadAnimation(this, global::Android.Resource.Animation.FadeIn),
                @out = AnimationUtils.LoadAnimation(this, global::Android.Resource.Animation.FadeOut);
            @in.Duration = HeaderAnimationDuration;
            @out.Duration = HeaderAnimationDuration;
            var switcher = (ImageSwitcher) ((RelativeLayout) _navView.GetHeaderView(0)).GetChildAt(0);
            var caption = (TextView) ((RelativeLayout) _navView.GetHeaderView(0)).GetChildAt(1);
            switcher.InAnimation = @in;
            switcher.OutAnimation = @out;
            switcher.SetFactory(this);
            var handler = new Handler();

            // ReSharper disable once InconsistentNaming
            void action() {
                if (!_headerRunning)
                    return;
                _currentHeader++;
                _currentHeader %= _images.Count;
                switcher.SetImageResource(_images.ElementAt(_currentHeader).Key);
                caption.Text = $"{_images.ElementAt(_currentHeader).Value} ({_currentHeader + 1}/{_images.Count})";
                handler.PostDelayed(action, HeaderInterval);
            }

            action();*/
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
            // If the user has returned from the WelcomeActivity, InitializeMain() now
            if (requestCode == WelcomeActivityRequestCode && InitializeGooglePlayServices())
                InitializeMain();
        }

        private void InitializeUniversalImageLoader() {
            var bkg = new ColorDrawable(Color.LightGray);
            ImageLoader.Instance.Init(new ImageLoaderConfiguration.Builder(this).DefaultDisplayImageOptions(
                new DisplayImageOptions.Builder().CacheOnDisk(PreferenceHelper.Caching)
                    .CacheInMemory(PreferenceHelper.Caching)
                    .Displayer(new FadeInBitmapDisplayer(200))
                    .ShowImageOnLoading(bkg)
                    .ShowImageForEmptyUri(bkg)
                    .ShowImageOnFail(new ColorDrawable(Color.Gray))
                    .BitmapConfig(Bitmap.Config.Rgb565)
                    .Build()).Build());
        }

        private bool InitializeGooglePlayServices() {
            LogHelper.WriteMessage("INFO", "Checking for Google Play Services");
            var gms = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (gms != ConnectionResult.Success) {
                LogHelper.WriteMessage("ERROR", "*** GOOGLE PLAY SERVICES UNAVAILABLE ***");
                var dialog = new appcompat7.AlertDialog.Builder(this).SetTitle("Google Play Services")
                    .SetMessage(
                        "Google Play Services (GMS) are unavailable on your device.  This may be a temporary error unless Google Play Services aren't installed on your device.\nUnfortunately, Merge requires these services to work.  This application must close.\n\n\"" +
                        GoogleApiAvailability.Instance.GetErrorString(gms) + "\"")
                    .SetNegativeButton("Exit", (s, e) => Process.KillProcess(Process.MyPid()))
                    .SetPositiveButton("Get Google Play Services",
                        (s, e) => GoogleApiAvailability.Instance.MakeGooglePlayServicesAvailable(this))
                    .SetCancelable(false)
                    .Create();
                dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
                dialog.Show();
                return false;
            }
            if (_playServices == null) {
                LogHelper.WriteMessage("INFO", "Google Play Services initializing");
                _playServices = new GoogleApiClient.Builder(this).AddApi(LocationServices.API).Build();
                _playServices?.Connect();
            }
            return true;
        }

        private void InitializeLeaderMenuItem() {
            _navView.Menu?.Clear();
            _navView.InflateMenu(Resource.Menu.drawer);
            _navView.SetCheckedItem(GetDrawerItemForTab(_selectedTab));
            if (!PreferenceHelper.IsValidLeader)
                _navView.Menu.RemoveItem(Resource.Id.drawerItemLeaderResources);
        }

        private void InitializeUserInterface() {
            _headers = new Dictionary<int, TabHeader>();
            _applier = new ViewApplier(this, _mainList);
            SetSupportActionBar(_toolbar);
            var toggle = new appcompat7.ActionBarDrawerToggle(this, _drawerLayout, _toolbar, Resource.String.app_name,
                Resource.String.app_name);
            _drawerLayout.AddDrawerListener(toggle);
            toggle.SyncState();
            InitializeLeaderMenuItem();
            _navView.NavigationItemSelected += (s, e) => {
                if (e.MenuItem.GroupId == Resource.Id.drawerGroupMain)
                    SelectTab(e.MenuItem.ItemId == Resource.Id.drawerItemHome
                        ? TabHome
                        : e.MenuItem.ItemId == Resource.Id.drawerItemEvents
                            ? TabEvents
                            : TabGroups, false);
                else
                    switch (e.MenuItem.ItemId) {
                        /*case Resource.Id.drawerItemLeaders:
                            StartActivity(typeof(LeadersActivity));
                            break;*/
                        case Resource.Id.drawerItemGroupMap:
                            StartActivity(typeof(GroupMapActivity));
                            break;
                        case Resource.Id.drawerItemLeaderResources:
                            StartActivity(typeof(LeaderResourcesActivity));
                            break;
                        case Resource.Id.drawerItemAbout:
                            StartActivity(typeof(AboutActivity));
                            break;
                        case Resource.Id.drawerItemSettings:
                            StartActivity(typeof(SettingsActivity));
                            break;
                        case Resource.Id.drawerItemFeedback:
                            LaunchUriAction.FromUri(
                                    "mailto:devgregw@outlook.com?subject=Merge+for+Android+Feedback+Submission")
                                .Invoke();
                            break;
                        /*case Resource.Id.drawerItemLogs:
                            new Action(async () => {
                                var strings = await LogHelper.GetAllLogs();
                                new AlertDialog.Builder(this).SetTitle("Submit Logs").SetItems(strings,
                                        (ss, ee) => { LogHelper.SendLog(strings[ee.Which]); })
                                    .SetPositiveButton("Delete All", (ss, ee) => {
                                        new AlertDialog.Builder(this).SetTitle("Delete Logs")
                                            .SetMessage(
                                                "Are you sure you want to delete all logs?  This cannot be undone.")
                                            .SetPositiveButton("Cancel", (sss, eee) => { })
                                            .SetNegativeButton("Continue",
                                                async (sss, eee) => await LogHelper.DeleteAllLogs())
                                            .Show();
                                    })
                                    .SetNegativeButton("Settings", (ss, ee) => StartActivity(typeof(SettingsActivity)))
                                    .Show();
                            }).Invoke();
                            break;*/
                    }
                _drawerLayout.CloseDrawers();
            };
        }

        private void InitializeMain() {
            if (SdkChecker.KitKat)
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            InitializeUniversalImageLoader();
            InitializeUserInterface();
            InitializeHeaderImageSwitcher();
            SelectTab(Intent.GetIntExtra("tab", TabHome), true);
            string action;
            if ((action = Intent.GetStringExtra("action")) != null)
                ActionBase.FromJson(action)?.Invoke();
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.NewMain);
            Cheeseknife.Bind(this);
            ((MergeActionInvocationReceiver) MergeDatabase.ActionInvocationReceiver).SetContext(this);
            if (PreferenceHelper.FirstRun) {
                // Open the full WelcomeActivity if this is the first time the user has opened the app
                StartActivityForResult(typeof(WelcomeActivity), WelcomeActivityRequestCode);
                return;
            }
            if (PreferenceHelper.AuthenticationState == PreferenceHelper.LeaderAuthenticationState.Failed) {
                // Open the WelcomeActivity asking for leader credentials only
                var intent = new Intent(this, typeof(WelcomeActivity));
                intent.PutExtra("leaderRoutine", true);
                StartActivityForResult(intent, WelcomeActivityRequestCode);
                return;
            }
            if (InitializeGooglePlayServices())
                InitializeMain(); // If the WelcomeActivity was shown, InitializeMain will be called from OnActivityResult()
        }

        protected override void OnResume() {
            InitializeGooglePlayServices();
            ((MergeActionInvocationReceiver) MergeDatabase.ActionInvocationReceiver).SetContext(this);
            base.OnResume();
        }

        protected override void OnStart() {
            LogHelper.WriteMessage("DEBUG", "Starting");
            //if (!_initializingGms)
            //_playServices?.Connect();
            base.OnStart();
        }

        protected override void OnRestart() {
            LogHelper.WriteMessage("DEBUG", "Restarting");
            InitializeLeaderMenuItem();
            SelectTab(_selectedTab, true);
            base.OnRestart();
        }

        protected override void OnStop() {
            LogHelper.WriteMessage("DEBUG", "Stopping");
            //if (!_initializingGms)
            //_playServices?.Disconnect();
            base.OnStop();
        }

        protected override void OnDestroy() {
            LogHelper.WriteMessage("INFO", "*** DESTROYING ***");
            base.OnDestroy();
        }

        protected override void OnSaveInstanceState(Bundle outState) {
            LogHelper.WriteMessage("DEBUG", "Saving instance state");
            // fix for AOSP bug 19917
            outState.PutString("WORKAROUND_FOR_BUG_19917_KEY", "WORKAROUND_FOR_BUG_19917_VALUE");
            base.OnSaveInstanceState(outState);
        }

        // ReSharper disable once RedundantOverriddenMember
        public override void OnConfigurationChanged(Configuration newConfig) => base.OnConfigurationChanged(newConfig);

        public override bool OnCreateOptionsMenu(IMenu menu) {
            MenuInflater.Inflate(Resource.Menu.mainMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            switch (item.ItemId) {
                case Resource.Id.action_refresh:
                    Nullify();
                    SelectTab(_selectedTab, true);
                    break;
            }
            return true;
        }
    }

#pragma warning restore 612, 618
}
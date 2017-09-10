#region LICENSE

// Project Merge.Android:  DataDetailActivity.cs (in Solution Merge.Android)
// Created by Greg Whatley on 06/30/2017 at 9:25 AM.
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
using System.Globalization;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.View.Menu;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using CheeseBind;
using Merge.Android.Helpers;
using Merge.Android.Receivers;
using Merge.Android.UI.Views;
using MergeApi.Client;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using MergeApi.Tools;
using Newtonsoft.Json;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Orientation = Android.Widget.Orientation;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Utilities = Merge.Android.Helpers.Utilities;

#endregion

namespace Merge.Android.UI.Activities {
    [Activity(Label = "Details", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class DataDetailActivity : AppCompatActivity, MenuBuilder.ICallback,
        AppBarLayout.IOnOffsetChangedListener, IOnMapReadyCallback {
        [BindView(Resource.Id.dataLayout)]
        private LinearLayout _dataLayout;
        [BindView(Resource.Id.fab)]
        private FloatingActionButton _fab;
        [BindView(Resource.Id.toolbar_layout)]
        private CollapsingToolbarLayout _toolbarLayout;
        [BindView(Resource.Id.mapFragmentContainer)]
        private FrameLayout _mapContainer;
        [BindView(Resource.Id.app_bar)]
        private AppBarLayout _appBarLayout;
        [BindView(Resource.Id.image)]
        private ImageView _image;

        private LinearLayout _pageContent;

        private MenuPopupHelper _menuHelper;
        private MergePage _page;
        private MergeEvent _event;
        private MergeGroup _group;

        public bool OnMenuItemSelected(MenuBuilder p0, IMenuItem p1) {
            switch (p1.ItemId) {
                case Resource.Id.MenuItemAddToCalendar:
                    AddToCalendarAction.FromEventId(_event.Id).Invoke();
                    return true;
                case Resource.Id.MenuItemGetDirections:
                    if (_event != null)
                        GetDirectionsAction.FromEventId(_event.Id).Invoke();
                    else
                        GetDirectionsAction.FromGroupId(_group.Id).Invoke();
                    return true;
                case Resource.Id.MenuItemRegister:
                    LaunchUriAction.FromUri(_event.RegistrationUrl).Invoke();
                    return true;
                case Resource.Id.MenuItemContactLeaders:
                    ShowContactInfoAction.FromGroupId(_group.Id).Invoke();
                    return true;
                case Resource.Id.MenuItemSeeAll:
                    new OpenGroupMapPageAction().Invoke();
                    return true;
                case Resource.Id.MenuItemReload:
                    SetupPageContent();
                    return true;
                case Resource.Id.MenuItemContactUs:
                    EmailAction.FromAddress("students@pantego.org").Invoke();
                    return true;
            }
            return false;
        }

        public void OnMenuModeChange(MenuBuilder p0) {
            // nothing
        }

        [OnClick(Resource.Id.fab)]
        public void Fab_OnClick(object sender, EventArgs e) => _menuHelper.Show();

        public void OnMapReady(GoogleMap map) {
            LatLng coordinates;
            // ReSharper disable once InconsistentNaming
            void repos() {
                map.MoveCamera(CameraUpdateFactory.NewLatLngZoom(coordinates, 15));
                map.AnimateCamera(CameraUpdateFactory.ZoomTo(14), 2000, null);
            }

            var options = new MarkerOptions();
            options.SetPosition(new LatLng((double)_group.Coordinates.Latitude,
                (double)_group.Coordinates.Longitude));
            options.SetTitle(_group.Name);
            var newMarker = map.AddMarker(options);
            coordinates = newMarker.Position;
            map.BuildingsEnabled = true;
            map.UiSettings.CompassEnabled = false;
            map.UiSettings.MapToolbarEnabled = false;
            map.UiSettings.SetAllGesturesEnabled(false);
            map.MapType = GoogleMap.MapTypeNormal;
            new Handler().PostDelayed(repos, 100);
        }

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset) {
            var collapsed = Math.Abs(verticalOffset) >= appBarLayout.TotalScrollRange / 2;
            var arrow = ContextCompat.GetDrawable(this, Resource.Drawable.ic_arrow_back_black_24dp);
            arrow.SetColorFilter(collapsed ? Color.White : Color.Transparent, PorterDuff.Mode.SrcIn);
            SupportActionBar.SetHomeAsUpIndicator(arrow);
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            if (item.ItemId == global::Android.Resource.Id.Home) {
                OnBackPressed();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnBackPressed() {
            _fab.Visibility = ViewStates.Invisible;
            _pageContent?.StartAnimation(new AlphaAnimation(1f, 0f) {
                Duration = 250L
            });
            base.OnBackPressed();
        }

        private void SetupMenu(int menuRes, bool removeRegister = false) {
            var menu = new MenuBuilder(this);
            menu.SetCallback(this);
            new MenuInflater(this).Inflate(menuRes, menu);
            _menuHelper = new MenuPopupHelper(this, menu, _fab);
            if (removeRegister)
                menu.RemoveItem(Resource.Id.MenuItemRegister);
            _menuHelper.SetForceShowIcon(true);
        }

        private async void SetupPageContent() {
            ((MergeElementCreationReceiver)MergeDatabase.ElementCreationReceiver).SetColorInfo(
                _page.Color.ToAndroidColor(), _page.Theme);
            if (_pageContent == null) {
                _pageContent = new LinearLayout(this) {
                    Orientation = Orientation.Vertical
                };
                _dataLayout.AddView(_pageContent);
            }
            _pageContent.RemoveAllViews();
            await Task.Run(() => {
                foreach (var e in _page.Content)
                    new Handler(MainLooper).Post(() => {
                        var view = e.CreateView<View>();
                        _pageContent.AddView(view);
                        _pageContent.AddView(new LinearLayout(this) {
                            LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                                Convert.ToInt32(TypedValue.ApplyDimension(ComplexUnitType.Dip, 4f,
                                    Resources.DisplayMetrics)))
                        });
                    });
            });
        }

        private void Colorize(Color c, Theme t) {
            if (SdkChecker.Lollipop) {
                _fab.BackgroundTintList = ColorStateList.ValueOf(c);
                var drawable = _fab.Drawable;
                drawable.SetColorFilter(c.ContrastColor(t), PorterDuff.Mode.SrcIn);
                _fab.SetImageDrawable(drawable);
            }
            _toolbarLayout.SetExpandedTitleColor(Color.Transparent);
        }

        private void SetupLayout(MergePage p) {
            LogHelper.FirebaseLog(this, "viewDetails", new Dictionary<string, string> {
                { "id", p.Id },
                { "type", "page" },
                { "name", p.Title }
            });
            if (p.LeadersOnly && !PreferenceHelper.IsValidLeader) {
                var dialog = new AlertDialog.Builder(this).SetTitle("Unauthorized")
                    .SetMessage(
                        "You are not authorized to view this page.  If you believe this is an error, turn on leader-only features in Settings or contact us.")
                    .SetPositiveButton("Dismiss", (s, e) => Finish()).SetNegativeButton("Contact Us", (s, e) => {
                        Finish();
                        EmailAction.FromAddress("students@pantego.org").Invoke();
                    }).SetNeutralButton("Settings", (s, e) => {
                        Finish();
                        StartActivity(typeof(SettingsActivity));
                    }).SetCancelable(false).Create();
                dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
                dialog.Show();
                return;
            }
            _page = p;
            SetupMenu(Resource.Menu.PageMenu);
            Colorize(p.Color.ToAndroidColor(), p.Theme);
            if (!string.IsNullOrWhiteSpace(p.Description))
                _dataLayout.AddView(new IconView(this, Resource.Drawable.Info, p.Description, true, true));
            SetupPageContent();
            if (p.LeadersOnly)
                _dataLayout.AddView(new IconView(this, Resource.Drawable.PasswordProtected, "Leaders Only"));
            _dataLayout.AddView(new IconView(this, Resource.Drawable.Targeting, Utilities.GetTargetingString(p)));
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        private void SetupLayout(MergeEvent e) {
            LogHelper.FirebaseLog(this, "viewDetails", new Dictionary<string, string> {
                { "id", e.Id },
                { "type", "event" },
                { "title", e.Title }
            });
            _event = e;
            SetupMenu(Resource.Menu.EventMenu, !e.HasRegistration);
            Colorize(e.Color.ToAndroidColor(), e.Theme);
            _dataLayout.AddView(new IconView(this, Resource.Drawable.Info, e.Description, true, true));
            var isFree = e.Price < 1d;
            _dataLayout.AddView(new IconView(this, Resource.Drawable.Cost, isFree ? "FREE" : $"${e.Price}", isFree,
                isFree));

            string MakeDateString(DateTime start, DateTime end) {
                return
                    $"Starts on {start.ToLongDateString()} at {start.ToString("h:mm tt", CultureInfo.CurrentUICulture)} and ends on {end.ToLongDateString()} at {end.ToString("h:mm tt", CultureInfo.CurrentUICulture)}";
            }

            var dateString = MakeDateString(e.StartDate.Value, e.EndDate.Value);
            if (e.RecurrenceRule != null) {
                var begin = RecurrenceRule.GetNextOccurrence(e.StartDate.Value, e.RecurrenceRule);
                if (begin.HasValue) {
                    var diff = e.EndDate.Value - e.StartDate.Value;
                    var end = begin.Value.AddMilliseconds(diff.TotalMilliseconds);
                    dateString = MakeDateString(begin.Value, end) +
                                 $"\n{RecurrenceRule.GetRuleDescription(e.StartDate.Value, e.RecurrenceRule)}";
                }
            }
            _dataLayout.AddView(new IconView(this, Resource.Drawable.DateTime,
                dateString));
            _dataLayout.AddView(new IconView(this, Resource.Drawable.Location,
                $"{e.Location}{(!string.IsNullOrWhiteSpace(e.Address) ? $" ({e.Address})" : "")}"));
            if (e.HasRegistration)
                _dataLayout.AddView(new IconView(this, Resource.Drawable.RegistrationRequired,
                    $"Registration required (closes on {e.RegistrationClosingDate.Value.ToString("dddd, d MMMM yyyy \"at\" h:mm tt", CultureInfo.CurrentUICulture)})"));
            _dataLayout.AddView(new IconView(this, Resource.Drawable.Targeting, Utilities.GetTargetingString(e)));
        }

        private void SetupLayout(MergeGroup g) {
            LogHelper.FirebaseLog(this, "viewDetails", new Dictionary<string, string> {
                { "id", g.Id },
                { "type", "group" },
                { "name", g.Name }
            });
            _group = g;
            SetupMenu(Resource.Menu.GroupMenu);
            Colorize(new Color(ContextCompat.GetColor(this, Resource.Color.colorPrimary)), MergeApi.Framework.Enumerations.Theme.Dark);
            _dataLayout.AddView(new IconView(this, Resource.Drawable.MergeGroups, g.LeadersFormatted, true, true));
            _dataLayout.AddView(new IconView(this, Resource.Drawable.Location, g.Address));
            _dataLayout.AddView(new IconView(this, Resource.Drawable.Home, $"Hosted by {g.Host}"));
            var fragment = SupportMapFragment.NewInstance();
            _mapContainer.Visibility = ViewStates.Visible;
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.mapFragmentContainer, fragment).Commit();
            fragment.GetMapAsync(this);
        }

        protected override void OnResume() {
            base.OnResume();
            ((MergeActionInvocationReceiver)MergeDatabase.ActionInvocationReceiver).SetContext(this);
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            if (SdkChecker.Lollipop) {
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
                Window.RequestFeature(WindowFeatures.ContentTransitions);
            }
            if (SdkChecker.Marshmallow)
                Window.DecorView.SystemUiVisibility = StatusBarVisibility.Visible;
            SetContentView(Resource.Layout.DataDetailActivity);
            Cheeseknife.Bind(this);
            _fab.SetImageResource(Resource.Drawable.MoreVertical);
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            _appBarLayout.AddOnOffsetChangedListener(this);
            Title = Intent.GetStringExtra("title");
            Utilities.LoadImageForDisplay(Intent.GetStringExtra("url"), _image);
            var json = Intent.GetStringExtra("json");
            switch (Intent.GetStringExtra("type")) {
                case "page":
                    SetupLayout(JsonConvert.DeserializeObject<MergePage>(json));
                    break;
                case "event":
                    SetupLayout(JsonConvert.DeserializeObject<MergeEvent>(json));
                    break;
                case "group":
                    SetupLayout(JsonConvert.DeserializeObject<MergeGroup>(json));
                    break;
            }
        }
    }
}
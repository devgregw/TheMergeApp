﻿#region LICENSE

// Project Merge.Android:  DataDetailActivity.cs (in Solution Merge.Android)
// Created by Greg Whatley on 05/24/2017 at 3:48 PM.
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
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.View.Menu;
using Android.Transitions;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Merge.Android.Classes.Controls;
using Merge.Android.Classes.Helpers;
using MergeApi.Client;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using MergeApi.Models.Mediums;
using MergeApi.Tools;
using Newtonsoft.Json;
using Orientation = Android.Widget.Orientation;
using Toolbar = Android.Support.V7.Widget.Toolbar;

#endregion

namespace Merge.Android {
    [Activity(Label = "DataDetailActivity")]
    public class DataDetailActivity : AppCompatActivity, View.IOnClickListener, MenuBuilder.ICallback, AppBarLayout.IOnOffsetChangedListener, IOnMapReadyCallback {
        private FloatingActionButton _fab;
        private MenuPopupHelper _menuHelper;
        private LinearLayout _dataLayout;

        public void OnClick(View v) {
            if (v.Id == Resource.Id.fab)
                _menuHelper.Show();
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            if (item.ItemId == global::Android.Resource.Id.Home) {
                OnBackPressed();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

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

        public override void OnBackPressed() {
            _fab.Visibility = ViewStates.Invisible;
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

        private LinearLayout _pageContent;

        private void SetupPageContent() {
            ((MergeElementCreationReceiver)MergeDatabase.ElementCreationReceiver).SetColorInfo(_page.Color.ToAndroidColor(), _page.Theme);
            if (_pageContent == null) {
                _pageContent = new LinearLayout(this) {
                    Orientation = Orientation.Vertical
                };
                _dataLayout.AddView(_pageContent);
            }
            _pageContent.RemoveAllViews();
            foreach (var e in _page.Content)
                _pageContent.AddView(e.CreateView<View>());
        }

        private Color _color;

        private void Colorize(Color c, Theme t) {
            _color = c;
            if (SdkChecker.Lollipop) {
                _fab.BackgroundTintList = ColorStateList.ValueOf(c);
                var drawable = _fab.Drawable;
                drawable.SetColorFilter(c.ContrastColor(t), PorterDuff.Mode.SrcIn);
                _fab.SetImageDrawable(drawable);
            }
            FindViewById<CollapsingToolbarLayout>(Resource.Id.toolbar_layout).SetExpandedTitleColor(c);
        }

        private void SetupLayout(MergePage p) {
            _page = p;
            SetupMenu(Resource.Menu.PageMenu);
            Colorize(p.Color.ToAndroidColor(), p.Theme);
            if (!string.IsNullOrWhiteSpace(p.Description))
                _dataLayout.AddView(new IconView(this, Resource.Drawable.Info, p.Description, true, true));
            SetupPageContent();
            if (p.LeadersOnly)
                _dataLayout.AddView(new IconView(this, Resource.Drawable.PasswordProtected, "Leaders Only"));
            _dataLayout.AddView(new IconView(this, Resource.Drawable.Targeting, ApiAccessor.GetTargetingString(p)));
        }

        private void SetupLayout(MergeEvent e) {
            _event = e;
            SetupMenu(Resource.Menu.EventMenu, !e.HasRegistration);
            Colorize(e.Color.ToAndroidColor(), e.Theme);
            _dataLayout.AddView(new IconView(this, Resource.Drawable.Info, e.Description, true, true));
            var isFree = e.Price < 1d;
            _dataLayout.AddView(new IconView(this, Resource.Drawable.Cost, isFree ? "FREE" : $"${e.Price}", isFree, isFree));
            // ReSharper disable once PossibleInvalidOperationException
            _dataLayout.AddView(new IconView(this, Resource.Drawable.DateTime, $"Starts on {e.StartDate.Value.ToLongDateString()} at {e.StartDate.Value.ToString("h:mm tt", CultureInfo.CurrentUICulture)} and ends on {e.EndDate.Value.ToLongDateString()} at {e.EndDate.Value.ToString("h:mm tt", CultureInfo.CurrentUICulture)}"));
            _dataLayout.AddView(new IconView(this, Resource.Drawable.Location, $"{e.Location}{(!string.IsNullOrWhiteSpace(e.Address) ? $" ({e.Address})" : "")}"));
            // TODO: add registration closing date/time
            if (e.HasRegistration)
                // ReSharper disable once PossibleInvalidOperationException
                _dataLayout.AddView(new IconView(this, Resource.Drawable.RegistrationRequired, $"Registration required (closes on {e.RegistrationClosingDate.Value.ToString("dddd, d MMMM yyyy \"at\" h:mm tt", CultureInfo.CurrentUICulture)})"));
            _dataLayout.AddView(new IconView(this, Resource.Drawable.Targeting, ApiAccessor.GetTargetingString(e)));
        }

        private void SetupLayout(MergeGroup g) {
            _group = g;
            SetupMenu(Resource.Menu.GroupMenu);
            Colorize(new Color(ContextCompat.GetColor(this, Resource.Color.colorPrimary)), MergeApi.Framework.Enumerations.Theme.Dark);
            _dataLayout.AddView(new IconView(this, Resource.Drawable.MergeGroups, g.LeadersFormatted, true, true));
            _dataLayout.AddView(new IconView(this, Resource.Drawable.Location, g.Address));
            _dataLayout.AddView(new IconView(this, Resource.Drawable.Home, $"Hosted by {g.Host}"));
            var fragment = SupportMapFragment.NewInstance();
            FindViewById<FrameLayout>(Resource.Id.mapFragmentContainer).Visibility = ViewStates.Visible;
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
            if ((int)Build.VERSION.SdkInt >= (int)BuildVersionCodes.M)
                Window.DecorView.SystemUiVisibility = StatusBarVisibility.Visible;
            SetContentView(Resource.Layout.DataDetailActivity);
            _dataLayout = FindViewById<LinearLayout>(Resource.Id.dataLayout);
            _fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            _fab.SetImageResource(Resource.Drawable.MoreVertical);
            _fab.SetOnClickListener(this);
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            FindViewById<AppBarLayout>(Resource.Id.app_bar).AddOnOffsetChangedListener(this);
            Title = Intent.GetStringExtra("title");
            ApiAccessor.LoadImageForDisplay(Intent.GetStringExtra("url"), FindViewById<ImageView>(Resource.Id.image));
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

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset) {
            var arrow = ContextCompat.GetDrawable(this, Resource.Drawable.ic_arrow_back_black_24dp);
            arrow.SetColorFilter(System.Math.Abs(verticalOffset) >= appBarLayout.TotalScrollRange / 2 ? Color.White : _color, PorterDuff.Mode.SrcIn);
            SupportActionBar.SetHomeAsUpIndicator(arrow);
        }

        public void OnMapReady(GoogleMap map) {
            LatLng coordinates;
            // ReSharper disable once InconsistentNaming
            void repos() {
                map.MoveCamera(CameraUpdateFactory.NewLatLngZoom(coordinates, 15));
                map.AnimateCamera(CameraUpdateFactory.ZoomTo(14), 2000, null);
            }
            var options = new MarkerOptions();
            options.SetPosition(new LatLng((double)_group.Coordinates.Latitude, (double)_group.Coordinates.Longitude));
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
    }
}
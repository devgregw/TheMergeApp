#region LICENSE

// Project Merge.Android:  GroupMapActivity.cs (in Solution Merge.Android)
// Created by Greg Whatley on 05/22/2017 at 9:50 PM.
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
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Merge.Android.Classes.Controls;
using Merge.Android.Classes.Helpers;
using MergeApi.Client;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using Newtonsoft.Json;
using appcompat = Android.Support.V7.App;
using Toolbar = Android.Support.V7.Widget.Toolbar;

#endregion

namespace Merge.Android {
    /// <summary>
    ///     The group map activity
    /// </summary>
    [Activity(Label = "Merge Groups", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class GroupMapActivity : appcompat.AppCompatActivity, IOnMapReadyCallback {
        #region Interface Implementations

        public void OnMapReady(GoogleMap map) {
            LogHelper.WriteMessage("INFO", "Group map ready");
            _repos = a => {
                if (a)
                    map.AnimateCamera(
                        CameraUpdateFactory.NewLatLngBounds(_bounds,
                            (int)(50 * Resources.DisplayMetrics.Density + 0.5f)), 1000, null);
                else
                    map.MoveCamera(CameraUpdateFactory.NewLatLngBounds(_bounds,
                        (int)(50 * Resources.DisplayMetrics.Density + 0.5f)));
            };
            var markers = new List<Tuple<Marker, MergeGroup>>();
            var builder = new LatLngBounds.Builder();
            foreach (var g in _groups) {
                var options = new MarkerOptions();
                options.SetPosition(new LatLng((double)g.Coordinates.Latitude, (double)g.Coordinates.Longitude));
                options.SetTitle(g.Name);
                var newMarker = map.AddMarker(options);
                markers.Add(new Tuple<Marker, MergeGroup>(newMarker, g));
                builder.Include(newMarker.Position);
            }
            map.MarkerClick += (s, e) => {
                foreach (var tuple in markers)
                    if (tuple.Item1.Id == e.Marker.Id) {
                        /*new AlertDialog.Builder(this).SetTitle(e.Marker.Title).SetCancelable(true).SetMessage(
                                $"Hosted by {tuple.Item2.Host}\n\nLead by {tuple.Item2.LeadersFormatted}\n\nLocated at {tuple.Item2.Address}")
                            .SetPositiveButton("Close", (S, E) => { })
                            .SetNegativeButton("Get Directions",
                                (S, E) => GetDirectionsAction.FromGroupId(tuple.Item2.Id).Invoke())
                            .SetNeutralButton("Contact",
                                (S, E) => ShowContactInfoAction.FromGroupId(tuple.Item2.Id).Invoke())
                            .Show();*/
                        var g = tuple.Item2;
                        var intent = new Intent(this, typeof(DataDetailActivity));
                        intent.PutExtra("json", JsonConvert.SerializeObject(g));
                        intent.PutExtra("title", g.Name);
                        intent.PutExtra("url", g.CoverImage);
                        intent.PutExtra("type", "group");
                        StartActivity(intent);
                    }
            };
            _bounds = builder.Build();
            var loc = CheckSelfPermission(Manifest.Permission.AccessFineLocation) ==
                                    Permission.Granted;
            if (!loc)
                Toast.MakeText(this, "Your location cannot be shown on the map because you denied location permissions.",
                    ToastLength.Long).Show();
            map.MyLocationEnabled = loc;
            map.BuildingsEnabled = true;
            map.UiSettings.CompassEnabled = true;
            map.UiSettings.MapToolbarEnabled = true;
            map.UiSettings.SetAllGesturesEnabled(true);
            map.UiSettings.ZoomControlsEnabled = true;
            map.MapType = GoogleMap.MapTypeNormal;
            new Handler().PostDelayed(() => _repos.Invoke(false), 100);
        }

        #endregion

        #region Variables

        private List<MergeGroup> _groups;
        private SupportMapFragment _mapFragment;
        private Action<bool> _repos;
        private IMenu _mainMenu;
        private LatLngBounds _bounds;

        #endregion

        #region Activity Life

        private async Task DoWork() {
            try {
                _groups = SessionRepo.Groups;
                _mapFragment = SupportMapFragment.NewInstance();
                // ReSharper disable once AccessToStaticMemberViaDerivedType
                var progressDialog = new ProgressDialog(this) {
                    Indeterminate = true
                };
                progressDialog.SetMessage("Loading...");
                progressDialog.SetTitle("Group Map");
                progressDialog.SetCancelable(false);
                progressDialog.Show();
                if (_groups == null || _groups.Count == 0) {
                    progressDialog.Show();
                    _groups = (await Task.Run(async () => await MergeDatabase.ListAsync<MergeGroup>())).ToList();
                    progressDialog.Dismiss();
                    if (_groups == null || !_groups.Any()) {
                        var dialog = new appcompat.AlertDialog.Builder(this).SetTitle("No Content").SetMessage("There are no Merge Groups to display on the map.").SetPositiveButton("Close", (s, e) => Finish()).Create();
                        dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
                        dialog.Show();
                        return;
                    }
                }
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.container, _mapFragment).Commit();
                _mapFragment.GetMapAsync(this);
            } catch (Exception e) {
                var dialog = new appcompat.AlertDialog.Builder(this).SetCancelable(false).SetTitle("Error").SetMessage($"An error occurred while loading content.\n{BasicCard.MakeExceptionString(e)}").SetPositiveButton("Close", (s, args) => Finish()).Create();
                dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
                dialog.Show();
            }
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            SetTheme(Resource.Style.AppTheme);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.GroupsMap);
            if (SdkChecker.KitKat)
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            new Action(async () => await DoWork()).Invoke();
        }

        public override bool OnPrepareOptionsMenu(IMenu menu) {
            if (_mainMenu != null) return base.OnPrepareOptionsMenu(menu);
            _mainMenu = menu;
            MenuInflater.Inflate(Resource.Menu.mapTypes, _mainMenu);
            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            switch (item.ItemId) {
                case Resource.Id.map_reposition:
                    _repos.Invoke(true);
                    return true;
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        #endregion
    }
}
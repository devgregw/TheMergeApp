#region LICENSE

// Project Merge.Android:  PageActivity.cs (in Solution Merge.Android)
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
using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Hardware.Display;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Merge.Android.Classes.Helpers;
using MergeApi.Framework.Abstractions;
using MergeApi.Models.Core;
using AlertDialog = Android.App.AlertDialog;
using Toolbar = Android.Support.V7.Widget.Toolbar;

#endregion

namespace Merge.Android {
    /// <summary>
    ///     An activity that renders pages
    /// </summary>
    /*[Activity(Label = "Page Details", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class PageActivity : AppCompatActivity {
        #region Utils

        private List<Tuple<View, ElementBase>> _updateQueue = new List<Tuple<View, ElementBase>>();

        /// <summary>
        ///     Finds the greatest common divisor of the given numbers
        /// </summary>
        private int Gcd(int w, int h) {
            while (true) {
                if (h == 0) return w;
                var w1 = w;
                w = h;
                h = w1 % h;
            }
        }

        #endregion

        #region Activity Life

        private CollapsingToolbarLayout _toolbarLayout;

        private Toolbar _toolbar;

        private void PasswordLoop(MergePage page, Action callback, int count = 0) {
            LogHelper.WriteMessage("INFO", $"Requesting password for page {page.Id}. Try {count}.");
            if (page.PasswordHash == "") {
                callback();
            } else {
                var dialog = new AlertDialog.Builder(this).Create();
                dialog = PasswordInputDialog.NewInstance(this, "Password Required",
                    "This page is protected with a password.  Please enter it to continue.", count != 0, Finish, p => {
                        if (p == page.PasswordHash)
                            callback();
                        else PasswordLoop(page, callback, ++count);
                    });
                dialog.Show();
            }
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.PageActivity);
            _toolbarLayout = FindViewById<CollapsingToolbarLayout>(Resource.Id.toolbarLayout);
            _toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(_toolbar);
            var data = (MergePage) SessionRepo.Get(Intent.GetStringExtra("repoId"));
            LogHelper.WriteMessage("INFO", $"Showing details for MergePage {data.Id}");
            PasswordLoop(data, () => {
                var contrast = data.Theme == MergeApi.Framework.Enumerations.Theme.Auto
                    ? data.Color.ToAndroidColor().ContrastColor()
                    : data.Theme == MergeApi.Framework.Enumerations.Theme.Dark
                        ? Color.Argb(255, 0, 0, 0)
                        : Color.Argb(255, 255, 255, 255);
                _toolbarLayout.SetTitle(data.Title);
                _toolbarLayout.SetContentScrimColor(data.Color.ToAndroidColor());
                _toolbarLayout.SetStatusBarScrimColor(data.Color.ToAndroidColor());
                _toolbarLayout.SetCollapsedTitleTextColor(contrast);
                _toolbarLayout.SetExpandedTitleColor(data.Color.ToAndroidColor());
                if (SdkChecker.LollipopOrLater) {
                    var window = Window;
                    window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                    window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    var barColor = data.Color.ToAndroidColor();
                    barColor.R -= Convert.ToByte(barColor.R * 0.15);
                    barColor.G -= Convert.ToByte(barColor.G * 0.15);
                    barColor.B -= Convert.ToByte(barColor.B * 0.15);
                    window.SetStatusBarColor(barColor);
                }
                if (string.IsNullOrWhiteSpace(data.Description))
                    FindViewById<TextView>(Resource.Id.view_description).Visibility = ViewStates.Gone;
                else
                    FindViewById<TextView>(Resource.Id.view_description).Text = data.Description;
                var content = FindViewById<LinearLayout>(Resource.Id.view_content);
                foreach (var e in data.Content) {
                    var v = e.CreateView<View>();
                    switch (v.GetType().Name.ToLower()) {
                        case "button":
                            if (SdkChecker.LollipopOrLater) {
                                ((Button) v).SetTextColor(contrast);
                                v.BackgroundTintList = ColorStateList.ValueOf(data.Color.ToAndroidColor());
                            }
                            v.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                                ViewGroup.LayoutParams.WrapContent) {
                                BottomMargin = 5,
                                TopMargin = 5
                            };
                            break;
                        case "textview":
                            v.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                                ViewGroup.LayoutParams.WrapContent) {
                                BottomMargin = 5,
                                TopMargin = 5
                            };
                            break;
                        case "imageview":
                            break;
                        case "webview":
                            _updateQueue.Add(new Tuple<View, ElementBase>(v, null));
                            v.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0);
                            var orientation =
                                ((DisplayManager) GetSystemService(DisplayService)).GetDisplay(Display.DefaultDisplay)
                                .Rotation;
                            new Handler().PostDelayed(
                                () =>
                                    v.LayoutParameters =
                                        new LinearLayout.LayoutParams(v.Width,
                                            orientation == SurfaceOrientation.Rotation0 ||
                                            orientation == SurfaceOrientation.Rotation180
                                                ? v.Width
                                                : v.Width / 2) {
                                            BottomMargin = 5,
                                            TopMargin = 5
                                        }, 100);
                            break;
                    }
                    content.AddView(v);
                }
                FindViewById<TextView>(Resource.Id.view_targeting).Text = ApiAccessor.GetTargetingString(data);
                var bkg = FindViewById<ImageView>(Resource.Id.view_cover);
                ApiAccessor.LoadImageForDisplay(data.CoverImage, bkg);
                //bkg.SetImageBitmap(data.Cover);
            });
        }

        public override void OnConfigurationChanged(Configuration newConfig) {
            base.OnConfigurationChanged(newConfig);
            foreach (var t in _updateQueue) {
                var v = t.Item1;
                var e = t.Item2;
                switch (v.GetType().Name.ToLower()) {
                    case "imageview":
                        break;
                    case "webview":
                        v.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0);
                        var orientation =
                            ((DisplayManager) GetSystemService(DisplayService)).GetDisplay(Display.DefaultDisplay)
                            .Rotation;
                        new Handler().PostDelayed(
                            () =>
                                v.LayoutParameters =
                                    new LinearLayout.LayoutParams(v.Width,
                                        orientation == SurfaceOrientation.Rotation0 ||
                                        orientation == SurfaceOrientation.Rotation180
                                            ? v.Width
                                            : v.Width / 2) {
                                        BottomMargin = 5,
                                        TopMargin = 5
                                    }, 100);
                        break;
                }
            }
        }

        #endregion
    }*/
}
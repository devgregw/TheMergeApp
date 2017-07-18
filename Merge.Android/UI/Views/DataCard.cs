#region LICENSE

// Project Merge.Android:  DataCard.cs (in Solution Merge.Android)
// Created by Greg Whatley on 06/30/2017 at 9:19 AM.
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
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Merge.Android.Helpers;
using Merge.Android.UI.Activities;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using Newtonsoft.Json;
using MergeApi.Tools;
using Utilities = Merge.Android.Helpers.Utilities;

#endregion

namespace Merge.Android.UI.Views {
    public sealed class DataCard : CardView, View.IOnClickListener {
        private string _title, _json, _type, _url;
        public DataCard(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

        public DataCard(Context context, MergePage p) : base(context) =>
            Initialize(p.Title, p.ShortDescription, JsonConvert.SerializeObject(p), "page", p.CoverImage,
                p.Color.ToAndroidColor(), p.Theme,
                !p.LeadersOnly
                    ? null
                    : new IconView(Context, Resource.Drawable.PasswordProtected, "Leaders Only"), p.ButtonAction != null ? new Button(Context) {
                    Text = p.ButtonLabel
                }.Manipulate(b => {
                    if (!SdkChecker.Lollipop) return b;
                    b.SetTextColor(p.Color.ToAndroidColor().ContrastColor(p.Theme));
                    b.BackgroundTintList = ColorStateList.ValueOf(p.Color.ToAndroidColor());
                    b.Click += (s, e) => {
                        OpenPageAction pageAction;
                        if ((pageAction = p.ButtonAction as OpenPageAction) != null) {
                            if (pageAction.PageId1 == p.Id)
                                ShowDetailsWithTransition();
                            else
                                p.ButtonAction.Invoke();
                        } else
                            p.ButtonAction.Invoke();
                    };
                    return b;
                }) : null);

        public DataCard(Context context, MergeEvent e) : base(context) =>
            Initialize(e.Title, e.ShortDescription, JsonConvert.SerializeObject(e), "event", e.CoverImage,
                e.Color.ToAndroidColor(), e.Theme);

        public DataCard(Context context, MergeGroup g) : base(context) =>
            Initialize(g.Name, $"Lead by {g.LeadersFormatted} and hosted by {g.Host}.", JsonConvert.SerializeObject(g),
                "group", g.CoverImage, Color.White, Theme.Auto);

        public DataCard(Context context, IAttributeSet attrs) : base(context, attrs) { }

        public DataCard(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) { }

        private void ShowDetailsWithTransition() {
            if (string.IsNullOrWhiteSpace(_type)) {
                Toast.MakeText(Context, "Invalid data.", ToastLength.Short).Show();
                return;
            }
            var options =
                ActivityOptionsCompat.MakeSceneTransitionAnimation((Activity)Context,
                    FindViewById<ImageView>(Resource.Id.image), "imageTransition");
            var intent = new Intent(Context, typeof(DataDetailActivity));
            intent.PutExtra("json", _json);
            intent.PutExtra("title", _title);
            intent.PutExtra("url", _url);
            intent.PutExtra("type", _type);
            Context.StartActivity(intent, options.ToBundle());
        }

        public void OnClick(View v) {
            if (v.Id == Resource.Id.card) {
                ShowDetailsWithTransition();
            }
        }

        private void Initialize(string title, string desc, string json, string type, string url, Color color,
            Theme theme, params View[] extraViews) {
            _title = title;
            _json = json;
            _type = type;
            _url = url;
            var v = Inflate(Context, Resource.Layout.DataCard, this);
            SetBackgroundColor(Color.Transparent);
            if (SdkChecker.Lollipop)
                v.FindViewById<CardView>(Resource.Id.card).Foreground =
                    new RippleDrawable(ColorStateList.ValueOf(Color.Argb(64, 0, 0, 0)), null,
                        new ColorDrawable(Color.Black));
            Utilities.LoadImageForDisplay(_url, v.FindViewById<ImageView>(Resource.Id.image));
            var titleView = v.FindViewById<TextView>(Resource.Id.dataCardTitle);
            titleView.Text = _title;
            //titleView.SetTextColor(color.ContrastColor(theme));
            var descView = v.FindViewById<TextView>(Resource.Id.dataCardDesc);
            descView.Text = desc;
            //descView.SetTextColor(color.ContrastColor(theme));
            var cardView = v.FindViewById<CardView>(Resource.Id.card);
            cardView.SetOnClickListener(this);
            //v.FindViewById<LinearLayout>(Resource.Id.dataCardRoot).SetBackgroundColor(color);
            var ll1 = v.FindViewById<LinearLayout>(Resource.Id.ll1);
            foreach (var view in extraViews.Where(_ => _ != null))
                ll1.AddView(view);
        }
    }
}
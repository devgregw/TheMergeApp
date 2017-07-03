#region LICENSE

// Project Merge.Android:  TipCard.cs (in Solution Merge.Android)
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
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Merge.Android.Helpers;
using MergeApi.Models.Core.Tab;

#endregion

namespace Merge.Android.UI.Views {
    public sealed class TipCard : CardView, View.IOnClickListener {
        private readonly TabTip _tip;

        public TipCard(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

        public TipCard(Context context, TabTip tip) : base(context) {
            _tip = tip;
            Tag = new ObjectWrapper<string>(_tip.Id);
            var v = Inflate(context, Resource.Layout.TipCard, this);
            SetBackgroundColor(Color.Transparent);
            var hasAction = tip.Action != null;
            var root = v.FindViewById<CardView>(Resource.Id.card);
            v.FindViewById<TextView>(Resource.Id.tipMessage).Text = tip.Message;
            v.FindViewById<LinearLayout>(Resource.Id.tipViewClickableLayout).Visibility =
                hasAction ? ViewStates.Visible : ViewStates.Gone;
            if (hasAction) {
                v.FindViewById<ImageView>(Resource.Id.tipCanClick).Alpha = 0.5f;
                root.Clickable = true;
                root.SetOnClickListener(this);
                if (SdkChecker.Lollipop)
                    root.Foreground =
                        new RippleDrawable(ColorStateList.ValueOf(Color.Argb(64, 0, 0, 0)), null,
                            new ColorDrawable(Color.Black));
            }
            var dismiss = v.FindViewById<ImageButton>(Resource.Id.tipDismissButton);
            dismiss.Visibility =
                tip.Persistent ? ViewStates.Invisible : ViewStates.Visible;
            if (!tip.Persistent)
                dismiss.SetOnClickListener(this);
        }

        public TipCard(Context context, IAttributeSet attrs) : base(context, attrs) { }
        public TipCard(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) { }

        public void OnClick(View v) {
            switch (v.Id) {
                case Resource.Id.card:
                    _tip.Action.Invoke();
                    break;
                case Resource.Id.tipDismissButton:
                    Dismiss();
                    break;
            }
        }

        private void Dismiss() {
            PreferenceHelper.AddDismissedTip(_tip.Id);
            ((ViewGroup) Parent).RemoveView(this);
        }
    }
}
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
using System.Diagnostics.CodeAnalysis;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Merge.Android.Helpers;
using MergeApi.Models.Core.Tab;

#endregion

namespace Merge.Android.UI.Views {
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public sealed class TipCard : CardView {
        private readonly TabTip _tip;

        [BindView(Resource.Id.card)] private CardView _card;

        public TipCard(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

        public TipCard(Context context, TabTip tip) : base(context) {
            _tip = tip;
            Tag = new ObjectWrapper<string>(_tip.Id);
            var v = Inflate(context, Resource.Layout.TipCard, this);
            Cheeseknife.Bind(this, v);
            SetBackgroundColor(Color.Transparent);
            var hasAction = tip.Action != null;
            v.FindViewById<TextView>(Resource.Id.tipMessage).Text = _tip.Message;
            v.FindViewById<LinearLayout>(Resource.Id.tipViewClickableLayout).Visibility =
                hasAction ? ViewStates.Visible : ViewStates.Gone;
            if (hasAction) {
                v.FindViewById<ImageView>(Resource.Id.tipCanClick).Alpha = 0.5f;
                _card.Clickable = true;
                if (SdkChecker.Lollipop)
                    _card.Foreground =
                        new RippleDrawable(ColorStateList.ValueOf(Color.Argb(64, 0, 0, 0)), null,
                            new ColorDrawable(Color.Black));
            }
            v.FindViewById<ImageButton>(Resource.Id.tipDismissButton).Visibility =
                _tip.Persistent ? ViewStates.Invisible : ViewStates.Visible;
        }

        public TipCard(Context context, IAttributeSet attrs) : base(context, attrs) { }
        public TipCard(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) { }

        [OnClick(Resource.Id.card)]
        private void Card_OnClick(object sender, EventArgs e) => _tip.Action.Invoke();

        [OnClick(Resource.Id.tipDismissButton)]
        private void DismissButton_OnClick(object sender, EventArgs e) {
            if (!_tip.Persistent)
                Dismiss();
        }

        private void Dismiss() {
            PreferenceHelper.AddDismissedTip(_tip.Id);
            ((ViewGroup) Parent).RemoveView(this);
        }
    }
}
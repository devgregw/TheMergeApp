#region LICENSE

// Project Merge.Android:  IconView.cs (in Solution Merge.Android)
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
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

#endregion

namespace Merge.Android.UI.Views {
    public sealed class IconView : RelativeLayout {
        public IconView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

        public IconView(Context context, int icon, string text, bool black = false,
            bool large = false) : base(context) =>
            Initialize(icon, text, black, large);

        public IconView(Context context, int icon, View view) : base(context) => Initialize(icon, view);
        public IconView(Context context, IAttributeSet attrs) : base(context, attrs) { }
        public IconView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) { }

        public IconView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs,
            defStyleAttr, defStyleRes) { }

        private void Initialize(int icon, View view) {
            var v = Inflate(Context, Resource.Layout.IconView, this);
            v.FindViewById<ImageView>(Resource.Id.itIcon).SetImageResource(icon);
            v.FindViewById<FrameLayout>(Resource.Id.itContainer).AddView(view);
        }

        private void Initialize(int icon, string text, bool black, bool large) {
            var tv = new TextView(Context) {Text = text};
            if (black)
                tv.SetTextColor(Color.Black);
            if (large)
                tv.TextSize = 18f;
            Initialize(icon, tv);
        }
    }
}
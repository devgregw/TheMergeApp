#region LICENSE

// Project Merge.Android:  BasicCard.cs (in Solution Merge.Android)
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
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;

#endregion

namespace Merge.Android.UI.Views {
    public sealed class BasicCard : CardView {
        public BasicCard(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

        public BasicCard(Context context, View content) : base(context) {
            var v = Inflate(context, Resource.Layout.BasicCard, this);
            v.FindViewById<RelativeLayout>(Resource.Id.root).AddView(content);
            SetBackgroundColor(Color.Transparent);
        }

        public BasicCard(Context context, Exception error) : this(context,
            new IconView(context, Resource.Drawable.Error,
                $"An error occurred while loading content.\n{MakeExceptionString(error)}", true, true) {
                LayoutParameters = MakeLayoutParams()
            }) { }

        public BasicCard(Context context, IAttributeSet attrs) : base(context, attrs) { }

        public BasicCard(Context context, IAttributeSet attrs, int defStyleAttr) :
            base(context, attrs, defStyleAttr) { }

        public static string MakeExceptionString(Exception e, string current = "") => e == null
            ? current
            : MakeExceptionString(e.InnerException, $"{current}{e.GetType().FullName}: {e.Message}\n");

        private static RelativeLayout.LayoutParams MakeLayoutParams() {
            var p = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent);
            p.AddRule(LayoutRules.CenterHorizontal);
            return p;
        }
    }
}
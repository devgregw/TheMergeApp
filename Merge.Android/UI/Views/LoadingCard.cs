﻿#region LICENSE

// Project Merge.Android:  LoadingCard.cs (in Solution Merge.Android)
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

#endregion

namespace Merge.Android.UI.Views {
    public sealed class LoadingCard : CardView {
        public LoadingCard(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) =>
            Initialize();

        public LoadingCard(Context context) : base(context) =>
            Initialize();

        public LoadingCard(Context context, IAttributeSet attrs) : base(context, attrs) =>
            Initialize();

        public LoadingCard(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
            =>
                Initialize();

        private void Initialize() {
            Inflate(Context, Resource.Layout.LoadingCard, this);
            SetBackgroundColor(Color.Transparent);
        }
    }
}
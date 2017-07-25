#region LICENSE

// Project Merge.Android:  Extensions.cs (in Solution Merge.Android)
// Created by Greg Whatley on 06/30/2017 at 9:26 AM.
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
using Android.Graphics;
using Android.Views;
using Java.Text;
using MergeApi.Framework.Enumerations;

#endregion

namespace Merge.Android.Helpers {
    public static class Extensions {
        #region System.DateTime

        public static long GetMillisecondsSinceEpoch(this DateTime dt) =>
            new SimpleDateFormat("M/d/yyyy h:mm:ss a").Parse(dt.ToString(CultureInfo.CurrentCulture)).Time;

        #endregion

        #region Android.Views.ViewGroup

        public static IEnumerable<View> GetChildren(this ViewGroup vg) {
            var children = new List<View>();
            for (var i = 0; i < vg.ChildCount; i++)
                children.Add(vg.GetChildAt(i));
            return children;
        }

        #endregion

        #region Android.Graphics.Color

        public static Color ContrastColor(this Color c, Theme t) => t == Theme.Dark
            ? Color.Black
            : t == Theme.Light
                ? Color.White
                : c.ContrastColor();

        public static Color ContrastColor(this Color c) {
            var a = 1 - (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255;
            var d = a < 0.5 ? 0 : 255;
            return Color.Argb(255, d, d, d);
        }

        #endregion

        #region System.String

        public static T ToEnum<T>(this string s) => (T) Enum.Parse(typeof(T), s, true);

        public static Color ToAndroidColor(this string s) => Color.ParseColor(s);

        #endregion
    }
}
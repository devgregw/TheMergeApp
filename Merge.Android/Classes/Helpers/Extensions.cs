#region LICENSE

// Project Merge.Android:  Extensions.cs (in Solution Merge.Android)
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
using System.Globalization;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Java.Text;
using Java.Util;
using MergeApi.Framework.Enumerations;
using Newtonsoft.Json.Linq;

#endregion

namespace Merge.Android.Classes.Helpers {
    public static class Extensions {
        #region Newtonsoft.Json.Linq.JObject

        public static Dictionary<string, string> ToDictionary(this JObject obj) {
            var dict = new Dictionary<string, string>();
            foreach (var item in obj.Properties())
                dict.Add(item.Name, item.Value.ToString());
            return dict;
        }

        #endregion

        #region Newtonsoft.Json.Linq.JArray

        public static List<Dictionary<string, string>> ToDictionary(this JArray array) {
            var l = new List<Dictionary<string, string>>();
            foreach (var token in array.Children())
                if (token is JObject)
                    l.Add((token as JObject).ToDictionary());
            return l;
        }

        #endregion

        #region System.Boolean

        /// <summary>
        ///     Inverts a boolean (<c>true</c> -> <c>false</c>; <c>false</c> -> <c>true</c>)
        /// </summary>
        /// <param name="b">Input boolean</param>
        /// <param name="outbool">Output boolean</param>
        public static void Invert(this bool b, out bool outbool) {
            outbool = !b;
        }

        #endregion

        #region Android.Graphics.Color

        public static Color ContrastColor(this Color c, Theme t) {
            return t == Theme.Dark ? Color.Black : t == Theme.Light ? Color.White : c.ContrastColor();
        }

        public static Color ContrastColor(this Color c) {
            var a = 1 - (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255;
            var d = a < 0.5 ? 0 : 255;
            return Color.Argb(255, d, d, d);
        }

        #endregion

        #region System.Collections.IEnumerable<T>

        /// <summary>
        ///     Transforms a list into a human-readable string
        /// </summary>
        /// <param name="ie">Ie.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static string Format<T>(this IEnumerable<T> ie) {
            var l = new List<T>(ie);
            if (l.Count == 0)
                return "";
            if (l.Count == 1)
                return l[0].ToString();
            if (l.Count == 2)
                return $"{l[0]} and {l[1]}";
            if (l.Count > 2) {
                var result = "";
                foreach (var i in l) {
                    var isLast = i.Equals(l.Last());
                    result += $"{(isLast ? "and " : "")}{i}{(isLast ? "" : ", ")}";
                }
                return result;
            }
            throw new Exception("Unable to format");
        }

        #endregion

        #region System.DateTime

        public static long GetMillisecondsSinceEpoch(this DateTime dt) {
            //return new Java.Util.GregorianCalendar(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second).TimeInMillis;
            //return Convert.ToInt64((dt.ToLocalTime() - new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime()).TotalMilliseconds);
            return new SimpleDateFormat("M/d/yyyy h:mm:ss a").Parse(dt.ToString(CultureInfo.CurrentCulture)).Time;
        }

        #endregion

        #region Systen.Colections.Generic.Dictionary<string , string>

        /// <summary>
        ///     Gets an item
        /// </summary>
        /// <returns>The item</returns>
        /// <param name="d">The parent dictionary</param>
        /// <param name="key">The item's key</param>
        /// <param name="def">A default value</param>
        public static string GetItem(this Dictionary<string, string> d, string key, string def) {
            if (d.ContainsKey(key)) {
                string obj;
                if (d.TryGetValue(key, out obj))
                    return obj;
            }
            return def;
        }

        #endregion

        #region System.Collections.Generic.KeyValuePair<TKey, TValue>

        /// <summary>
        ///     Converts a <c>KeyValuePair</c> to a single-element dictionary
        /// </summary>
        /// <returns>The dictionary</returns>
        /// <param name="pair">The original pair</param>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the value</typeparam>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this KeyValuePair<TKey, TValue> pair) {
            var d = new Dictionary<TKey, TValue> {{pair.Key, pair.Value}};
            return d;
        }

        #endregion

        #region System.String

        /// <summary>
        ///     Determines if a string is a number
        /// </summary>
        /// <returns><c>true</c> if is the string is a number; otherwise, <c>false</c></returns>
        /// <param name="s">The string to check</param>
        public static bool IsNumber(this string s) {
            int a;
            return int.TryParse(s, out a);
        }

        public static T ToEnum<T>(this string s) {
            return (T) Enum.Parse(typeof(T), s, true);
        }

        public static Color ToAndroidColor(this string s) {
            return Color.ParseColor(s);
        }

        #endregion

        #region Android.Views.ViewGroup

        /// <summary>
        ///     Removes the and disposes the children of a layout
        /// </summary>
        /// <param name="vg">The layout to act upon</param>
        public static void RemoveAndDisposeChildren(this ViewGroup vg) {
            for (var i = 0; i < vg.ChildCount; i++) {
                var v = vg.GetChildAt(i);
                v.Dispose();
            }
            vg.RemoveAllViews();
        }

        public static IEnumerable<View> GetChildren(this ViewGroup vg) {
            var children = new List<View>();
            for (var i = 0; i < vg.ChildCount; i++)
                children.Add(vg.GetChildAt(i));
            return children;
        }

        #endregion

        #region System.Collections.Generic.Dictionary<string, object>

        /// <summary>
        ///     Adds or replaces an item in a dictionary
        /// </summary>
        /// <param name="d">The parent dictionary</param>
        /// <param name="key">The item's key</param>
        /// <param name="value">The item's value</param>
        public static void AddOrReplace(this Dictionary<string, object> d, string key, object value) {
            if (d.ContainsKey(key)) d.Remove(key);
            d.Add(key, value);
        }

        /// <summary>
        ///     Gets an item
        /// </summary>
        /// <returns>The item</returns>
        /// <param name="d">The parent dictionary</param>
        /// <param name="key">The item's key</param>
        /// <param name="def">A default value</param>
        /// <typeparam name="T">The type of the item</typeparam>
        public static T GetItem<T>(this Dictionary<string, object> d, string key, T def) {
            if (d.ContainsKey(key)) {
                object obj;
                if (d.TryGetValue(key, out obj))
                    return (T) obj;
            }
            return def;
        }

        /// <summary>
        ///     Checks if a list of strings are all keys in a dictionary
        /// </summary>
        /// <returns><c>true</c>, if all strings are keys, <c>false</c> otherwise</returns>
        /// <param name="d">The dictionary to act upon</param>
        /// <param name="keys">The keys to check</param>
        public static bool CheckForKeys(this Dictionary<string, object> d, params string[] keys) {
            foreach (var k in keys)
                if (!d.ContainsKey(k))
                    return false;
            return true;
        }

        #endregion
    }
}
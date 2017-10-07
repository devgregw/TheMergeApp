#region LICENSE

// Project Merge.iOS:  Extensions.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 08/05/2016 at 1:19 PM.
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
using Foundation;
using MergeApi.Framework.Enumerations;
using MergeApi.Tools;
using Newtonsoft.Json.Linq;
using UIKit;
using Xamarin.Forms;

#endregion

namespace Merge.Classes.Helpers {
    public static class Extensions {
        public static int IndexOf<T>(this IEnumerable<T> e, T item) => Array.IndexOf(e.ToArray(), item);

        public static UIViewController GetTopmostViewController(this UIWindow w) {
            var controller = UIApplication.SharedApplication.KeyWindow.RootViewController;
            while (controller.PresentedViewController != null)
                controller = controller.PresentedViewController;
            return controller;
        }

        public static void AddToolbarItem(this Page p, string text, string icon, EventHandler clicked) =>
            p.ToolbarItems.Add(new ToolbarItem {
                Text = text,
                Icon = icon
            }.Manipulate(i => {
                i.Clicked += clicked;
                return i;
            }));

        public static NavigationPage WrapInNavigationPage(this Page p) => new NavigationPage(p);

        public static FontAttributes ToFontAttributes(this LabelStyle style) {
            switch (style) {
                case LabelStyle.Normal:
                    return FontAttributes.None;
                case LabelStyle.Bold:
                    return FontAttributes.Bold;
                case LabelStyle.Italic:
                    return FontAttributes.Italic;
                case LabelStyle.BoldItalic:
                    return FontAttributes.Bold | FontAttributes.Italic;
                default:
                    return FontAttributes.None;
            }
        }

        public static Aspect ToAspect(this ScaleType scale) {
            switch (scale) {
                case ScaleType.None:
                    return 0;
                case ScaleType.Fill:
                    return Aspect.Fill;
                case ScaleType.Uniform:
                    return Aspect.AspectFill;
                case ScaleType.UniformToFill:
                    return Aspect.AspectFit;
                default:
                    return 0;
            }
        }

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

        public static void Invert(this bool @in, out bool @out) {
            @out = !@in;
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

        #region Xamarin.Forms.Color

        public static Color ContrastColor(this Color color, Theme theme) {
            if (theme == Theme.Light)
                return Color.White;
            if (theme == Theme.Dark)
                return Color.Black;
            double r = color.R * 255, g = color.G * 255, b = color.B * 255;
            var a = 1 - (0.299 * r + 0.587 * g + 0.114 * b) / 255;
            var d = a < 0.5 ? 0 : 255;
            return Color.FromRgba(d, d, d, 255);
        }

        #endregion

        #region System.Int64

        public static DateTime ToDateTime(this long l) {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(l).ToLocalTime();
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

        public static Color ToFormsColor(this string s) {
            return Color.FromHex(s);
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

        #region System.DateTime

        public static long GetTimestamp(this DateTime dt) {
            return (long) dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static NSDate ToNsDate(this DateTime dt) {
            var localDate = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(2001, 1, 1, 0, 0, 0));
            return NSDate.FromTimeIntervalSinceReferenceDate((dt - localDate).TotalSeconds);
        }

        #endregion
    }
}
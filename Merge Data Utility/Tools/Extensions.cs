#region LICENSE

// Project Merge Data Utility:  Extensions.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 06/23/2017 at 10:45 AM.
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#endregion

namespace Merge_Data_Utility.Tools {
    public static class Extensions {
        public static int IndexOf<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            KeyValuePair<TKey, TValue> item) => dict.Keys.ToList().IndexOf(item.Key);

        public static IEnumerable<TSource> AsOne<TSource>(this IEnumerable<IEnumerable<TSource>> lists) {
            var list = new List<TSource>();
            foreach (var l in lists)
                list.AddRange(l);
            return list;
        }

        public static TSource Max<TSource>(this IEnumerable<TSource> enumerable, Func<TSource, IComparable> selector) {
            var list = enumerable.ToList();
            var indices = new Dictionary<int, IComparable>();
            for (var i = 0; i < list.Count; i++)
                indices.Add(i, selector(list[i]));
            indices = indices.OrderBy(p => p.Value).ToDictionary(p => p.Key, p => p.Value);
            return !list.Any() ? default(TSource) : list[indices.Last().Key];
        }

        public static TSource Min<TSource>(this IEnumerable<TSource> enumerable, Func<TSource, IComparable> selector) {
            var list = enumerable.ToList();
            var indices = new Dictionary<int, IComparable>();
            for (var i = 0; i < list.Count; i++)
                indices.Add(i, selector(list[i]));
            indices = indices.OrderBy(p => p.Value).ToDictionary(p => p.Key, p => p.Value);
            return !list.Any() ? default(TSource) : list[indices.First().Key];
        }

        public static DateTime ToDateTime(this int ts) {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(ts).ToLocalTime();
        }

        public static string YesOrNo(this bool b) {
            return b ? "Yes" : "No";
        }

        public static DateTime WithoutTime(this DateTime dt) {
            return new DateTime(dt.Year, dt.Month, dt.Day);
        }

        public static string ToHex(this Color c) {
            return
                $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        public static string GetId(this TextBox tb) {
            return tb.Text.Substring(tb.Text.IndexOf("/", StringComparison.CurrentCulture) + 1);
        }

        public static void SetId(this TextBox tb, string prefix, string id) {
            tb.Text = $"{prefix}/{id}";
        }

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        public static string Format(this IEnumerable<string> enumerable) {
            var list = enumerable.ToList();
            switch (list.Count) {
                case 0:
                    return "";
                case 1:
                    return list.ElementAt(0);
                case 2:
                    return $"{list.ElementAt(0)} and {list.ElementAt(1)}";
            }
            var result = "";
            foreach (var i in list) {
                var isLast = i.Equals(list.Last());
                result += $"{(isLast ? "and " : "")}{i}{(isLast ? "" : ", ")}";
            }
            return result;
        }

        public static bool TryCast<T>(this object o, out dynamic result) where T : class {
            var casted = o as T;
            result = casted;
            return casted != null;
        }

        public static void ForEach<T>(this IEnumerable<T> e, Action<T> action) {
            foreach (var _ in e)
                action(_);
        }

        public static string Sum<T>(this IEnumerable<T> e, Func<T, string> selector) {
            return e.Aggregate("", (current, i) => current + selector(i));
        }

        public static ComparableKeyValuePair<TKey, TValue> AsComparable<TKey, TValue>(
            this KeyValuePair<TKey, TValue> kvp) where TValue : IComparable where TKey : IComparable {
            return new ComparableKeyValuePair<TKey, TValue>(kvp);
        }

        public static T GetValueOrDefault<T>(this IDictionary<string, object> dict, string key, T def) {
            object value;
            if (dict.TryGetValue(key, out value))
                return (T) value;
            return def;
        }

        public static ListeningCollection<T> AsListeningCollection<T>(this IList<T> list, Action<T> added,
            Action<T> removed) {
            return new ListeningCollection<T>(list, added, removed);
        }

        public static Color ToColor(this string s) {
            return (Color) ColorConverter.ConvertFromString("#FF" + s.Replace("#", ""));
        }

        public static Window GetWindow(this Page p) {
            FrameworkElement parent = p;
            do {
                parent = (FrameworkElement) parent.Parent;
            } while (!(parent is Window));
            return (Window) parent;
        }
    }
}
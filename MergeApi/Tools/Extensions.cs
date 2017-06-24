#region LICENSE

// Project MergeApi:  Extensions.cs (in Solution MergeApi)
// Created by Greg Whatley on 03/20/2017 at 6:44 PM.
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

#endregion

namespace MergeApi.Tools {
    public static class Extensions {
        /*public static ListResponse<T> Apply<T>(this ListResponse<T> r, Predicate<T> p) where T : IIdentifiable {
            return new ListResponse<T>(r.Items.Where(i => p(i)), r.Type, r.Data, r.Errors.ToArray());
        }

        public static void Log(this object o, LogLevel level, string message) {
            MergeDatabase.LogReceiver.Log(level, o.GetType().FullName, message);
        }

        public static void Log(this object o, LogLevel level, Exception e) {
            MergeDatabase.LogReceiver.Log(level, o.GetType().FullName, e);
        }*/

        public static TResult Cast<TValue, TResult>(this TValue obj) where TResult : TValue {
            return (TResult) obj;
        }

        public static DateTime ToDateTime(this long timestamp) {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
        }

        public static Dictionary<TKey, TValue> WithoutKeys<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            params TKey[] keys) {
            Utilities.AssertCondition<object>(o => o != null, dict, keys);
            return
                new Dictionary<TKey, TValue>(dict.Where(p => !keys.Contains(p.Key))
                    .ToDictionary(x => x.Key, x => x.Value));
        }

        public static TResult Convert<T, TResult>(this T obj, Func<T, TResult> converter) {
            Utilities.AssertCondition<object>(o => o != null, obj, converter);
            return converter(obj);
        }

        public static string ToMD5Hash(this string s) {
            return Utilities.HashMD5(s);
        }

        public static TEnum ToEnum<TEnum>(this string str, bool ignoreCase) {
            return (TEnum) Enum.Parse(typeof(TEnum), str, ignoreCase);
        }

        public static string Format<T>(this IEnumerable<T> ie) {
            if (ie == null)
                return "";
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
    }
}
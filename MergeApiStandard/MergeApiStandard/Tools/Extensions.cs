#region LICENSE

// Project MergeApi:  Extensions.cs (in Solution MergeApi)
// Created by Greg Whatley on 06/23/2017 at 10:43 AM.
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
using MergeApi.Framework.Enumerations;

#endregion

namespace MergeApi.Tools {
    public static class Extensions {
        public static string GetDescription(this ValidationResultType r) {
            switch (r) {
                case ValidationResultType.Success:
                    return "No errors detected";
                case ValidationResultType.EventNotFound:
                    return "This action points to an event that does not exist";
                case ValidationResultType.PageNotFound:
                    return "This action points to a page that does not exist";
                case ValidationResultType.GroupNotFound:
                    return "This action points to a Merge group that does not exist";
                case ValidationResultType.PageValidationFailure:
                    return "This action points to a page that failed validation";
                case ValidationResultType.EventValidationFailure:
                    return "This action points to an event that failed validation";
                case ValidationResultType.OutdatedAction:
                    return "This action is configured to add an event that has already ended to the calendar";
                case ValidationResultType.ButtonActionValidationFailure:
                    return "The action featured by this element failed validation";
                case ValidationResultType.NoAddress:
                    return "This action is configured to get directions to an event with no address";
                case ValidationResultType.Exception:
                    return "Validation failed unexpectedly";
                case ValidationResultType.PageActionValidationFailure:
                    return "The action featured by this page's button failed validation";
                case ValidationResultType.PageContentValidationFailure:
                    return "One or more of the content elements featured by this page failed validation";
                case ValidationResultType.OutdatedEvent:
                    return "This event already occurred";
                default:
                    return "Uh oh";
            }
        }

        public static TResult Manipulate<T, TResult>(this T obj, Func<T, TResult> manipulator) {
            Utilities.AssertCondition<object>(o => o != null, obj, manipulator);
            return manipulator(obj);
        }

        // ReSharper disable once InconsistentNaming
        public static string MD5(this string s) => Utilities.HashMD5(s);

        public static TEnum ToEnum<TEnum>(this string str, bool ignoreCase = true) => (TEnum)Enum.Parse(typeof(TEnum), str, ignoreCase);

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
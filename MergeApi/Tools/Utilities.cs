#region LICENSE

// Project MergeApi:  Utilities.cs (in Solution MergeApi)
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
using System.Text;
using XLabs.Cryptography;

#endregion

namespace MergeApi.Tools {
    public static class Utilities {
        public static void ThrowIfNull(string sender, string method, Dictionary<string, object> items) {
            foreach (var p in items)
                if (p.Value == null)
                    throw new ArgumentException($"Null reference passed to '{p.Key}' in '{method}' in '{sender}'",
                        p.Key);
        }

        public static void AssertCondition<T>(Func<T, bool> condition, params T[] arguments) {
            foreach (var argument in arguments)
                if (!condition(argument))
                    throw new ArgumentException(
                        $"The object '{(argument == null ? "<null>" : argument.ToString())}' ({typeof(T).FullName}) did not meet the specified condition(s)");
        }

        public static void AssertCondition<T>(bool condition, T argument) {
            AssertCondition(o => condition, argument);
        }

        public static string HashMD5(string plain) {
            using (var md5 = MD5.Create()) {
                return string.Join("",
                    from b in md5.ComputeHash(Encoding.UTF8.GetBytes(plain)) select b.ToString("x2"));
            }
        }
    }
}
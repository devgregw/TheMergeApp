#region LICENSE

// Project Merge Data Utility:  ComparableKeyValuePair.cs (in Solution Merge Data Utility)
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

#endregion

namespace Merge_Data_Utility.Tools {
    public sealed class ComparableKeyValuePair<TKey, TValue> : IComparable<ComparableKeyValuePair<TKey, TValue>>
        where TValue : IComparable where TKey : IComparable {
        public ComparableKeyValuePair(TKey key, TValue value) {
            Key = key;
            Value = value;
        }

        public ComparableKeyValuePair(KeyValuePair<TKey, TValue> kvp) : this(kvp.Key, kvp.Value) { }
        public TKey Key { get; set; }

        public TValue Value { get; set; }

        public int CompareTo(ComparableKeyValuePair<TKey, TValue> other) {
            return Value.CompareTo(other.Value);
        }

        public KeyValuePair<TKey, TValue> ToKeyValuePair() {
            return new KeyValuePair<TKey, TValue>(Key, Value);
        }

        public ComparableKeyValuePair<TValue, TKey> Switch() {
            return new ComparableKeyValuePair<TValue, TKey>(Value, Key);
        }
    }
}
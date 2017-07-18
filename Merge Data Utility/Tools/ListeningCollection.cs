#region LICENSE

// Project Merge Data Utility:  ListeningCollection.cs (in Solution Merge Data Utility)
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Merge_Data_Utility.Tools {
    public sealed class ListeningCollection<T> : IList<T> {
        private Action<T> _added, _removed;
        private IList<T> _items;

        public ListeningCollection(Action<T> added, Action<T> removed) {
            _added = added;
            _removed = removed;
            _items = new List<T>();
        }

        public ListeningCollection(IList<T> items, Action<T> added, Action<T> removed)
            : this(added, removed) {
            _items = items;
        }

        public IEnumerator<T> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(T item) {
            _items.Add(item);
            _added(item);
        }

        public void Clear() {
            _items.Clear();
        }

        public bool Contains(T item) {
            return _items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            var x = _items.Remove(item);
            if (!x) return false;
            _removed(item);
            return true;
        }

        public int Count => _items.Count;
        public bool IsReadOnly => _items.IsReadOnly;

        public int IndexOf(T item) {
            return _items.IndexOf(item);
        }

        public void Insert(int index, T item) {
            _items.Insert(index, item);
            _added(item);
        }

        public void RemoveAt(int index) {
            _removed(_items.ElementAt(index));
            _items.RemoveAt(index);
        }

        public T this[int index] {
            get => _items[index];
            set => throw new NotImplementedException();
        }
    }
}
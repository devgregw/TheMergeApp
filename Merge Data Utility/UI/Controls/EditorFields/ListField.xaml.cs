#region LICENSE

// Project Merge Data Utility:  ListField.xaml.cs (in Solution Merge Data Utility)
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Framework.Abstractions;

#endregion

namespace Merge_Data_Utility.UI.Controls.EditorFields {
    /// <summary>
    ///     Interaction logic for ListField.xaml
    /// </summary>
    public partial class ListField : UserControl {
        private Func<ListViewItem> _add;

        private Func<ListViewItem, ListViewItem> _edit;

        private Func<ListViewItem, bool> _remove;

        public ListField() {
            InitializeComponent();
        }

        public ListField(bool allowMovement) : this() {
            moveButtons.Visibility = Visibility.Collapsed;
        }

        public int Count => list.Items.Count;

        public void Prepare(Func<ListViewItem> add, Func<ListViewItem, bool> remove,
            Func<ListViewItem, ListViewItem> edit) {
            _add = add;
            _remove = remove;
            _edit = edit;
        }

        private void UpdateCount() {
            count.Text = $"{Count} items";
        }

        public IEnumerable<ListViewItem> GetItems() {
            return list.Items.Cast<ListViewItem>();
        }

        public IEnumerable<T> GetItems<T>(Func<ListViewItem, T> selector) {
            return GetItems().Select(selector);
        }

        public void SetItems(IEnumerable<ListViewItem> items) {
            list.Items.Clear();
            foreach (var i in items)
                list.Items.Add(i);
            UpdateCount();
        }

        public IEnumerable<ElementBase> GetElements() {
            return GetItems(i => (ElementBase) i.Tag);
        }

        public void SetElements(IEnumerable<ElementBase> e) {
            if (e == null)
                return;
            SetItems(e.Select(element => new ListViewItem {
                Content = element.ToFriendlyString(),
                Tag = element
            }));
        }

        public IEnumerable<MediumBase> GetContactMediums() {
            return GetItems(i => (MediumBase) i.Tag);
        }

        public void SetContactMediums(IEnumerable<MediumBase> m) {
            if (m == null)
                return;
            SetItems(m.Select(medium => new ListViewItem {
                Content = medium.ToFriendlyString(),
                Tag = medium
            }));
        }

        private void AddAndSelect(ListViewItem i) {
            list.Items.Add(i);
            list.SelectedItem = i;
        }

        private void Add(object sender, RoutedEventArgs e) {
            var result = _add();
            if (result != null)
                AddAndSelect(result);
            UpdateCount();
        }

        private void Edit(object sender, RoutedEventArgs e) {
            if (list.SelectedItem == null) {
                MessageBox.Show("Select an item to edit it.", "Editor", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK);
            } else {
                var result = _edit((ListViewItem) list.SelectedItem);
                if (result != null)
                    list.Items[list.SelectedIndex] = result;
            }
        }

        private void Remove(object sender, RoutedEventArgs e) {
            if (list.SelectedItem == null)
                MessageBox.Show("Select an item to remove it.", "Editor", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK);
            else if (_remove((ListViewItem) list.SelectedItem))
                list.Items.RemoveAt(list.SelectedIndex);
            UpdateCount();
        }

        private void MoveBottom(object sender, RoutedEventArgs e) {
            if (list.SelectedItem == null) {
                MessageBox.Show("Select an item to move it.", "Editor", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK);
            } else {
                var item = list.SelectedItem;
                list.Items.Remove(item);
                list.Items.Add(item);
            }
        }

        private void MoveTop(object sender, RoutedEventArgs e) {
            if (list.SelectedItem == null) {
                MessageBox.Show("Select an item to move it.", "Editor", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK);
            } else {
                var item = list.SelectedItem;
                list.Items.Remove(item);
                list.Items.Insert(0, item);
            }
        }

        private void MoveDown(object sender, RoutedEventArgs e) {
            if (list.SelectedItem == null) {
                MessageBox.Show("Select an item to move it.", "Editor", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK);
            } else {
                if (list.SelectedIndex + 1 >= Count) {
                    MessageBox.Show("This item cannot be moved any futher.", "Editor", MessageBoxButton.OK,
                        MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }
                var index = list.SelectedIndex;
                var mov = list.Items[index + 1];
                list.Items.RemoveAt(index + 1);
                list.Items.Insert(index, mov);
            }
        }

        private void MoveUp(object sender, RoutedEventArgs e) {
            if (list.SelectedItem == null) {
                MessageBox.Show("Select an item to move it.", "Editor", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK);
            } else {
                if (list.SelectedIndex - 1 < 0) {
                    MessageBox.Show("This item cannot be moved any futher.", "Editor", MessageBoxButton.OK,
                        MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }
                var index = list.SelectedIndex;
                var mov = list.Items[index - 1];
                list.Items.RemoveAt(index - 1);
                list.Items.Insert(index, mov);
            }
        }
    }
}
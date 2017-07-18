#region LICENSE

// Project Merge Data Utility:  ObjectChooserWindow.xaml.cs (in Solution Merge Data Utility)
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Merge_Data_Utility.Tools;

#endregion

namespace Merge_Data_Utility.UI.Windows {
    /// <summary>
    ///     Interaction logic for ObjectChooserWindow.xaml
    /// </summary>
    public partial class ObjectChooserWindow : Window {
        private object _object;

        public ObjectChooserWindow() {
            InitializeComponent();
        }

        public ObjectChooserWindow(Func<Task<IEnumerable<ListViewItem>>> source) : this() {
            Loaded += async (s, e) => {
                var reference = new LoaderReference(cc);
                reference.StartLoading();
                (await source()).ForEach(i => list.Items.Add(i));
                reference.StopLoading();
            };
        }

        public bool ObjectSelected => _object != null;

        public T GetSelectedObject<T>() {
            return (T) _object;
        }

        private void Cancel(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Choose(object sender, RoutedEventArgs e) {
            if (list.SelectedIndex == -1) {
                MessageBox.Show(this, "You must select an item or click Cancel to close this window.", "No Item",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            _object = ((ListViewItem) list.SelectedItem).Tag;
            Close();
        }
    }
}
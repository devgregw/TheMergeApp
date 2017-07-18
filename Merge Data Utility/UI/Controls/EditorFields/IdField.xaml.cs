#region LICENSE

// Project Merge Data Utility:  IdField.xaml.cs (in Solution Merge Data Utility)
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
using System.Windows;
using System.Windows.Controls;
using MergeApi.Client;

#endregion

namespace Merge_Data_Utility.UI.Controls.EditorFields {
    public sealed class RegeneratedEventArgs : EventArgs {
        public RegeneratedEventArgs(string prev, string n) {
            PreviousId = prev;
            NewId = n;
        }

        public string PreviousId { get; }

        public string NewId { get; }
    }

    /// <summary>
    ///     Interaction logic for IdBox.xaml
    /// </summary>
    public partial class IdField : UserControl {
        public EventHandler<RegeneratedEventArgs> Regenerated;

        public IdField() {
            InitializeComponent();
        }

        public string Id => box.Text;

        public void SetId(string id, bool canRegen) {
            box.Text = string.IsNullOrWhiteSpace(id) ? MergeDatabase.NewId() : id;
            regenButton.IsEnabled = canRegen;
        }

        private void RegenerateRequested(object sender, RoutedEventArgs e) {
            var old = Id;
            SetId("", true);
            Regenerated?.Invoke(this, new RegeneratedEventArgs(old, Id));
        }
    }
}
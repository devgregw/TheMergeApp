﻿#region LICENSE

// Project Merge Data Utility:  AttentionListItem.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 03/20/2017 at 6:42 PM.
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace Merge_Data_Utility.UI.Controls.Other {
    /// <summary>
    ///     Interaction logic for AttentionListItem.xaml
    /// </summary>
    public partial class AttentionListItem : UserControl {
        private Func<Task<bool>> _fixer;

        private Action _onSuccess, _onError;

        public AttentionListItem() {
            InitializeComponent();
            fixButton.Click += FixButton_Click;
        }

        public AttentionListItem( /*AttentionInfo*/ dynamic info, Action onSuccess, Action onError) : this() {
            message.Text = info.Message;
            _fixer = info.Fixer;
            if (_fixer == null)
                fixButton.IsEnabled = false;
            _onSuccess = onSuccess;
            _onError = onError;
        }

        private async void FixButton_Click(object sender, RoutedEventArgs e) {
            if (await _fixer()) {
                ((ListView) Parent).Items.Remove(this);
                _onSuccess();
            } else {
                _onError();
            }
        }
    }
}
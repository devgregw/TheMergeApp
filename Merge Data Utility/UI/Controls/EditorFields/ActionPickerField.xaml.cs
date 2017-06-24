#region LICENSE

// Project Merge Data Utility:  ActionPickerField.xaml.cs (in Solution Merge Data Utility)
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
using System.Windows;
using System.Windows.Controls;
using MergeApi.Framework.Abstractions;
using Merge_Data_Utility.UI.Windows;

#endregion

namespace Merge_Data_Utility.UI.Controls.EditorFields {
    /// <summary>
    ///     Interaction logic for ActionPickerField.xaml
    /// </summary>
    public partial class ActionPickerField : UserControl {
        private ActionBase _action;

        public ActionPickerField() {
            InitializeComponent();
        }

        public ActionBase DefaultAction { get; set; }

        public event EventHandler ActionSelected;

        private bool _showView;

        public bool ShowViewCodeButton {
            get => _showView;
            set {
                _showView = value;
                viewCodeButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public ActionBase SelectedAction {
            get => _action;
            set {
                _action = value;
                ActionSelected?.Invoke(this, null);
                box.Text = value?.ToFriendlyString();
            }
        }

        public void Reset() {
            SelectedAction = DefaultAction;
        }

        private void Configure(object sender, RoutedEventArgs e) {
            var window = new ActionConfigurationWindow(_action);
            window.ShowDialog();
            if (window.Action != null)
                SelectedAction = window.Action;
        }

        private void Clear(object sender, RoutedEventArgs e) {
            SelectedAction = null;
        }

        private void Reset(object sender, RoutedEventArgs e) {
            Reset();
        }

        private void ViewCode(object sender, RoutedEventArgs e) {
            new ActionCodeViewerWindow(SelectedAction).ShowDialog();
        }
    }
}
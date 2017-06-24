#region LICENSE

// Project Merge Data Utility:  TextInputWindow.xaml.cs (in Solution Merge Data Utility)
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

using System.Windows;
using System.Windows.Input;
// ReSharper disable MemberCanBePrivate.Global

#endregion

namespace Merge_Data_Utility.UI.Windows {
    /// <summary>
    ///     Interaction logic for TextInputWindow.xaml
    /// </summary>
    public partial class TextInputWindow : Window {
        public TextInputWindow() {
            InitializeComponent();
            box.KeyUp += (s, e) => {
                if (e.Key != Key.Enter) return;
                e.Handled = true;
                Ok(box, e);
            };
        }

        public TextInputWindow(string title, string message, string existingValue = "",
            InputScopeNameValue scope = InputScopeNameValue.Default) : this() {
            Title = title;
            this.message.Text = message;
            box.Text = existingValue;
            box.InputScope = new InputScope {
                Names = {
                    new InputScopeName(scope)
                }
            };
            box.Focus();
        }

        public string Input { get; private set; }

        public static string GetInput(string title, string message, string existingValue = "",
            InputScopeNameValue scope = InputScopeNameValue.Default) {
            var w = new TextInputWindow(title, message, existingValue, scope);
            w.ShowDialog();
            return w.Input;
        }

        private void Cancel(object sender, RoutedEventArgs e) {
            Input = null;
            Close();
        }

        private void Ok(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(box.Text)) {
                if (MessageBox.Show(this, "Input is invalid.  Click OK to edit input or Cancel to close this window.",
                        "Input",
                        MessageBoxButton.OKCancel, MessageBoxImage.Error, MessageBoxResult.OK) ==
                    MessageBoxResult.Cancel)
                    Cancel(sender, e);
            } else {
                Input = box.Text;
                Close();
            }
        }
    }
}
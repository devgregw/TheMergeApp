#region LICENSE

// Project Merge Data Utility:  ActionConfigurationWindow.xaml.cs (in Solution Merge Data Utility)
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
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Framework.Abstractions;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages.Base;

#endregion

namespace Merge_Data_Utility.UI.Windows {
    /// <summary>
    ///     Interaction logic for ActionConfigWindow.xaml
    /// </summary>
    public partial class ActionConfigurationWindow : Window {
        public ActionConfigurationWindow() : this(null) { }

        public ActionConfigurationWindow(ActionBase source) {
            InitializeComponent();
            ActionConfigurationPage.Mappings.ForEach(p => {
                var words = Regex.Split(p.Key.Name.Replace("Action", ""), @"(?<!^)(?=[A-Z])");
                for (var i = 0; i < words.Length; i++)
                    if (i != 0)
                        words[i] = words[i].ToLower();
                var name = words.Aggregate("", (current, w) => current + $"{w}{(words.Last() == w ? "" : " ")}");
                typeBox.Items.Add(new ComboBoxItem {
                    Content = name,
                    Tag = p.Key
                });
            });
            var original = source == null ? null : ActionConfigurationPage.GetPage(source.GetType(), source);
            typeBox.SelectionChanged += (s, e) => {
                var item = (ComboBoxItem) typeBox.SelectedItem;
                contentFrame.Navigate(source != null && (Type) item.Tag == source.GetType()
                    ? original
                    : ActionConfigurationPage.GetPage((Type) item.Tag, null));
            };
            typeBox.SelectedIndex = source == null
                ? 0
                : ActionConfigurationPage.Mappings.Keys.ToList().IndexOf(source.GetType());
        }

        public ActionBase Action { get; private set; }

        private void Cancel(object sender, RoutedEventArgs e) {
            Action = null;
            Close();
        }

        private void Done(object sender, RoutedEventArgs e) {
            var a = ((ActionConfigurationPage) contentFrame.Content).GetAction();
            if (a == null) return;
            Action = a;
            Close();
        }
    }
}
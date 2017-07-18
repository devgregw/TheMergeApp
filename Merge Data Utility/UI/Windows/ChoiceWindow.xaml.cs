#region LICENSE

// Project Merge Data Utility:  ChoiceWindow.xaml.cs (in Solution Merge Data Utility)
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

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Merge_Data_Utility.UI.Windows {
    /// <summary>
    ///     Interaction logic for ChoiceWindow.xaml
    /// </summary>
    public partial class ChoiceWindow : Window {
        public ChoiceWindow() {
            InitializeComponent();
        }

        private ChoiceWindow(string title, string msg, string[] choices) : this() {
            Title = title;
            message.Text = msg;
            for (var i = 0; i < choices.Length; i++) {
                var b = new Button {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 0, 5),
                    Content = choices[i],
                    Tag = i
                };
                b.Click += (s, e) => {
                    Choice = (int) b.Tag;
                    Close();
                };
                list.Children.Add(b);
            }
        }

        public int Choice { get; private set; } = -1;

        public static int GetChoice(string title, string msg, string[] choices) {
            var w = new ChoiceWindow(title, msg, choices);
            w.ShowDialog();
            return w.Choice;
        }
    }
}
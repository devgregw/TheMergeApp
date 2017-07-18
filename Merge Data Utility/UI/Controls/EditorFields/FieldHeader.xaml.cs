﻿#region LICENSE

// Project Merge Data Utility:  FieldHeader.xaml.cs (in Solution Merge Data Utility)
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

namespace Merge_Data_Utility.UI.Controls.EditorFields {
    /// <summary>
    ///     Interaction logic for FieldHeader.xaml
    /// </summary>
    public partial class FieldHeader : UserControl {
        public static DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string),
            typeof(FieldHeader), new PropertyMetadata("", PropertyChanged));

        public static DependencyProperty DescriptionProperty = DependencyProperty.Register("Description",
            typeof(string),
            typeof(FieldHeader), new PropertyMetadata("", PropertyChanged));

        public FieldHeader() {
            InitializeComponent();
        }

        public string Title {
            get => (string) GetValue(TitleProperty);
            set {
                SetValue(TitleProperty, value);
                Update();
            }
        }

        public string Description {
            get => (string) GetValue(DescriptionProperty);
            set {
                SetValue(DescriptionProperty, value);
                Update();
            }
        }

        private static void PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            ((FieldHeader) o).Update();
        }

        private void Update() {
            title.Text = Title;
            desc.Text = Description;
        }
    }
}
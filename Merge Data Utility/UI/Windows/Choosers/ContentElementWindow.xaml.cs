﻿#region LICENSE

// Project Merge Data Utility:  ContentElementWindow.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 03/22/2017 at 8:14 AM.
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

using System.Windows.Navigation;
using MergeApi.Framework.Abstractions;
using Merge_Data_Utility.UI.Pages;

#endregion

namespace Merge_Data_Utility.UI.Windows.Choosers {
    /// <summary>
    ///     Interaction logic for ContentElementWindow.xaml
    /// </summary>
    public partial class ContentElementWindow : NavigationWindow {
        public ContentElementWindow() : this(null) { }

        public ContentElementWindow(ElementBase existingValue) {
            InitializeComponent();
            if (existingValue == null)
                Navigate(new ContentElementTypePage(this));
            else
                Navigate(new ContentElementPage(this, existingValue));
        }

        public ElementBase Element { get; set; }
    }
}
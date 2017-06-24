#region LICENSE

// Project Merge Data Utility:  ImageInputWindow.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 03/25/2017 at 9:08 AM.
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
using System.Globalization;
using System.Windows;
using Merge_Data_Utility.Tools;

#endregion

namespace Merge_Data_Utility.UI.Windows {
    /// <summary>
    ///     Interaction logic for ImageInputWindow.xaml
    /// </summary>
    public partial class ImageInputWindow : Window {
        private bool _dt;

        private string _n;

        public ImageInputWindow() {
            InitializeComponent();
        }

        private ImageInputWindow(string title, string m, string fileNameBase, bool appendDateTime)
            : this() {
            Title = title;
            message.Text = m;
            _n = fileNameBase;
            _dt = appendDateTime;
        }

        public string ImageUrl { get; set; }

        public static string GetUrl(string title, string m, string fileNameBase, bool appendDateTime) {
            var w = new ImageInputWindow(title, m, fileNameBase, appendDateTime);
            w.ShowDialog();
            return w.ImageUrl;
        }

        private void Cancel(object sender, RoutedEventArgs e) {
            Close();
        }

        private async void Ok(object sender, RoutedEventArgs e) {
            var reference = new LoaderReference(content);
            reference.StartLoading("Processing...");
            ImageUrl = await field.PerformChangesAsync(
                (_dt ? $"{_n}-{DateTime.Now.ToString("MMddyyyy\"-\"hhmmss", CultureInfo.CurrentUICulture)}" : _n).ToLower());
            Close();
        }
    }
}
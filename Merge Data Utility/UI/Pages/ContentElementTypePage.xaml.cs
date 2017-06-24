#region LICENSE

// Project Merge Data Utility:  ContentElementTypePage.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 03/20/2017 at 6:45 PM.
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
using Merge_Data_Utility.UI.Windows.Choosers;

#endregion

namespace Merge_Data_Utility.UI.Pages {
    /// <summary>
    ///     Interaction logic for ContentElementTypePage.xaml
    /// </summary>
    public partial class ContentElementTypePage : Page {
        private ContentElementWindow _window;

        public ContentElementTypePage() {
            InitializeComponent();
        }

        public ContentElementTypePage(ContentElementWindow window) : this() {
            _window = window;
        }

        private void TypeVideo(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new ContentElementPage(_window, 3));
        }

        private void TypeImage(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new ContentElementPage(_window, 2));
        }

        private void TypeLabel(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new ContentElementPage(_window, 1));
        }

        private void TypeButton(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new ContentElementPage(_window, 0));
        }
    }
}
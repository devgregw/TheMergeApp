#region LICENSE

// Project Merge Data Utility:  UploadedFileChooser.xaml.cs (in Solution Merge Data Utility)
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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Merge_Data_Utility.Tools;

#endregion

namespace Merge_Data_Utility.UI.Windows.Choosers {
    /// <summary>
    ///     Interaction logic for UploadedFileChooser.xaml
    /// </summary>
    public partial class UploadedFileChooser : Window {
        public static readonly string[] ImageExtensions = {
            "png", "jpe", "jpeg", "jpg", "gif", "bmp", "tiff"
        };

        private List<string> _extensions;

        public UploadInfo SelectedFile;

        public UploadedFileChooser() {
            InitializeComponent();
        }

        public UploadedFileChooser(IEnumerable<string> exts) : this() {
            /*_extensions = exts.Select(s => s.ToLower()).ToList();
            extBlock.Text = string.Join(", ", _extensions.Select(s => s.ToUpper()));
            Loaded += async (s, e) => {
                var reference = new LoaderReference(cc);
                reference.StartLoading("Looking for files...");
                var files = await MergeDatabase.ListStorageReferencesAsync(StaticAuth.AuthLink);
                var allowedFiles =
                    files.Where(p => _extensions.Contains(ImageConverter.GetExtension(p.Key).ToLower()));
                foreach (var f in allowedFiles) {
                    var frame = new Image {
                        Source = new BitmapImage(new Uri(f.Value)),
                        Width = 320,
                        Height = 180,
                        Stretch = Stretch.UniformToFill
                    };
                    var item = new ListViewItem {
                        Content = f.Key,
                        Tag = f
                    };
                    ToolTipService.SetToolTip(item, frame);
                    ToolTipService.SetInitialShowDelay(item, 0);
                    list.Items.Add(item);
                }
                reference.StopLoading();
            };*/
        }

        private void Cancel(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Open(object sender, RoutedEventArgs e) {
            if (list.SelectedItems.Count == 0) {
                MessageBox.Show(this, "Choose a file to continue or click Cancel.", "Server Browser",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            } else {
                SelectedFile = (UploadInfo) ((ListViewItem) list.SelectedItems[0]).Tag;
                Close();
            }
        }
    }
}
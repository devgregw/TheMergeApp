﻿#region LICENSE

// Project Merge Data Utility:  ImageUploader.xaml.cs (in Solution Merge Data Utility)
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

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Merge_Data_Utility.Tools;
using Microsoft.Win32;

#endregion

namespace Merge_Data_Utility.UI.Controls.EditorFields {
    /// <summary>
    ///     Interaction logic for ImageUploader.xaml
    /// </summary>
    public partial class ImageUploader : UserControl {
        private string _original = "";

        private State _state = State.Empty;

        public ImageUploader() {
            InitializeComponent();
        }

        public string Value {
            get => coverFileBox.Text;
            private set {
                coverFileBox.Text = value;
                coverImage.Source = value == "" ? null : new BitmapImage(new Uri(value));
            }
        }

        public async Task<string> PerformChangesAsync(string name) {
            if (_state == State.NotModified)
                return Value;
            // assuming _state == State.FileSelected
            var localPath = Value;
            //if (_shouldConvert) localPath = ImageConverter.ConvertToPng(localPath);
            //File.Copy(localPath, "Cache\\upload.png", true);
            return (await FileUploader.PutStorageReferenceAsync( /*"Cache\\upload.png"*/localPath, name + ".png")).Url;
        }

        private void coverBrowse_Click(object sender, RoutedEventArgs e) {
            var dlg = new OpenFileDialog {
                Filter = "Image Files|*.tiff;*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.jpe",
                Title = "Choose an Image"
            };
            if (dlg.ShowDialog().GetValueOrDefault(false)) {
                Value = dlg.FileName;
                _state = State.FileSelected;
            }
        }

        public void SetOriginalValue(string v) {
            Value = v;
            if (v.Contains(@"\")) {
                _state = State.FileSelected;
            } else {
                _original = v;
                _state = State.NotModified;
            }
        }

        /*private void uploadBrowse_Click(object sender, RoutedEventArgs e) {
            var chooser = new UploadedFileChooser(UploadedFileChooser.ImageExtensions);
            chooser.ShowDialog();
            if (chooser.SelectedFile != null) SetServerValue(chooser.SelectedFile.Url);
        }*/

        private void clearButton_Click(object sender, RoutedEventArgs e) {
            if (_original == "") {
                Value = "";
                _state = State.Empty;
            } else {
                Value = _original;
                _state = State.NotModified;
            }
        }

        private enum State {
            Empty,
            FileSelected,
            NotModified
        }
    }
}
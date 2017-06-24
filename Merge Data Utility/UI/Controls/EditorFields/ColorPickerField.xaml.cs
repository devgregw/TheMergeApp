#region LICENSE

// Project Merge Data Utility:  ColorPickerField.xaml.cs (in Solution Merge Data Utility)
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
using System.Windows.Controls;
using System.Windows.Media;
using Merge_Data_Utility.UI.Windows;

#endregion

namespace Merge_Data_Utility.UI.Controls.EditorFields {
    /// <summary>
    ///     Interaction logic for ColorPickerField.xaml
    /// </summary>
    public partial class ColorPickerField : UserControl {
        private ThemePreviewField _preview;

        private ImageUploader _upload;

        public ColorPickerField() {
            InitializeComponent();
        }

        public Color? SelectedColor {
            get => picker.SelectedColor;
            set => picker.SelectedColor = value;
        }

        public void Prepare(ThemePreviewField preview, ImageUploader upload) {
            _preview = preview;
            _upload = upload;
            if (_upload == null)
                btnSuggestions.Visibility = Visibility.Collapsed;
        }

        private void ColorPicked(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            _preview?.Update(SelectedColor);
        }

        private void SuggestionsRequested(object sender, RoutedEventArgs e) {
            var window = new PaletteWindow(_upload.Value);
            window.ShowDialog();
            if (window.ChosenColor.HasValue)
                SelectedColor = window.ChosenColor.Value;
            // safe to use Value here because we already checked that it HasValue
        }
    }
}
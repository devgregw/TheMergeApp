#region LICENSE

// Project Merge Data Utility:  ThemePreviewField.xaml.cs (in Solution Merge Data Utility)
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

using System.Windows.Controls;
using System.Windows.Media;
using MergeApi.Framework.Enumerations;

#endregion

namespace Merge_Data_Utility.UI.Controls.EditorFields {
    /// <summary>
    ///     Interaction logic for ThemePreviewField.xaml
    /// </summary>
    public partial class ThemePreviewField : UserControl {
        private Color? _color = Colors.White;

        private Theme _theme = Theme.Auto;

        public ThemePreviewField() {
            InitializeComponent();
        }

        public void Update(Color? color) {
            _color = color;
            UpdateInternal();
        }

        public void Update(Theme t) {
            _theme = t;
            UpdateInternal();
        }

        private Color Contrast(Color c) {
            var a = 1 - (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255;
            var d = a < 0.5 ? 0 : 255;
            return Color.FromArgb(255, (byte) d, (byte) d, (byte) d);
        }

        private Color ColorForTheme(Theme t) {
            return t == Theme.Auto
                ? Contrast(_color.GetValueOrDefault(Colors.White))
                : t == Theme.Light
                    ? Colors.White
                    : Colors.Black;
        }

        private void UpdateInternal() {
            bkg.Background = new SolidColorBrush(_color.GetValueOrDefault(Colors.White));
            block.Foreground = new SolidColorBrush(ColorForTheme(_theme));
        }
    }
}
#region LICENSE

// Project Merge Data Utility:  ContentElementPage.xaml.cs (in Solution Merge Data Utility)
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Elements;
using MergeApi.Tools;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Windows.Choosers;

#endregion

namespace Merge_Data_Utility.UI.Pages {
    /// <summary>
    ///     Interaction logic for ContentElementPage.xaml
    /// </summary>
    public partial class ContentElementPage : Page {
        private bool _imageOk, _videoOk;

        private bool _ready;
        private int _type;

        private ContentElementWindow _window;

        public ContentElementPage() {
            InitializeComponent();
            labelStyleBox.SelectedIndex = 0;
            /*scaleTypeBox.SelectedIndex = 0;
            scaleTypeBox.SelectionChanged +=
                (s, e) =>
                    imagePreview.Stretch =
                        ((ComboBoxItem) scaleTypeBox.SelectedItem).Tag.ToString().ToEnum<Stretch>(true);*/
            videoVendorBox.SelectedIndex = 0;
        }

        public ContentElementPage(ContentElementWindow window, int type) : this() {
            _type = type;
            _window = window;
            switch (type) {
                case 0:
                    button.Visibility = Visibility.Visible;
                    label.Visibility = Visibility.Collapsed;
                    image.Visibility = Visibility.Collapsed;
                    video.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    button.Visibility = Visibility.Collapsed;
                    label.Visibility = Visibility.Visible;
                    image.Visibility = Visibility.Collapsed;
                    video.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    button.Visibility = Visibility.Collapsed;
                    label.Visibility = Visibility.Collapsed;
                    image.Visibility = Visibility.Visible;
                    video.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    button.Visibility = Visibility.Collapsed;
                    label.Visibility = Visibility.Collapsed;
                    image.Visibility = Visibility.Collapsed;
                    video.Visibility = Visibility.Visible;
                    break;
            }
            Loaded += (s, e) => back.IsEnabled = NavigationService.CanGoBack;
            HideScriptErrors();
            _ready = true;
        }

        public ContentElementPage(ContentElementWindow window, ElementBase existingValue)
            : this(window, GetTypeInt(existingValue)) {
            LoadExistingValue(existingValue);
        }

        private static int GetTypeInt(ElementBase e) {
            if (e is ButtonElement)
                return 0;
            if (e is LabelElement)
                return 1;
            if (e is ImageElement)
                return 2;
            if (e is VideoElement)
                return 3;
            throw new InvalidOperationException("Something went wrong!");
        }

        private void LoadExistingValue(ElementBase e) {
            if (e is ButtonElement) {
                var b = (ButtonElement) e;
                buttonLabelBox.Text = b.Label;
                buttonAction.DefaultAction = b.Action;
                buttonAction.Reset();
            } else if (e is LabelElement) {
                var l = (LabelElement) e;
                labelTextBox.Text = l.Label;
                labelSizeBox.Value = l.Size;
                labelStyleBox.SelectedItem =
                    labelStyleBox.Items.Cast<ComboBoxItem>().First(i => i.Tag.ToString() == l.Style.ToString());
                UpdateLabelPreview(null, null);
            } else if (e is ImageElement) {
                var i = (ImageElement) e;
                imageUploader.SetOriginalValue(i.Url);
            } else {
                var v = (VideoElement) e;
                videoIdBox.Text = v.VideoId;
                videoVendorBox.SelectedItem =
                    videoVendorBox.Items.Cast<ComboBoxItem>().First(i => i.Content.ToString() == v.Vendor.ToString());
                UpdateVideoPreview(null, null);
            }
        }

        private void HideScriptErrors() {
            var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            var objComWebBrowser = fiComWebBrowser.GetValue(videoPreview);
            if (objComWebBrowser == null) {
                videoPreview.Loaded += (o, s) => HideScriptErrors(); //In case we are to early
                return;
            }
            objComWebBrowser.GetType()
                .InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] {true});
        }

        private void UpdateLabelPreview(object sender, RoutedEventArgs e) {
            if (!_ready)
                return;
            labelPreview.Text = labelTextBox.Text;
            labelPreview.FontSize = labelSizeBox.Value.GetValueOrDefault(12);
            labelPreview.FontStyle = ((ComboBoxItem) labelStyleBox.SelectedItem).Tag.ToString().Contains("Italic")
                ? FontStyles.Italic
                : FontStyles.Normal;
            labelPreview.FontWeight = ((ComboBoxItem) labelStyleBox.SelectedItem).Tag.ToString().Contains("Bold")
                ? FontWeights.Bold
                : FontWeights.Normal;
        }

        /*private void UpdateImagePreview(object sender, RoutedEventArgs e) {
            if (!_ready)
                return;
            try {
                var req = (HttpWebRequest) WebRequest.Create(sourceBox.Text);
                req.Method = "HEAD";
                var res = (HttpWebResponse) req.GetResponse();
                if (res.StatusCode == HttpStatusCode.NotFound)
                    throw new Exception();
                res.Close();
                imagePreview.Source = new BitmapImage(new Uri(sourceBox.Text));
                _imageOk = true;
            } catch (Exception ex) {
                imagePreview.Source = new BitmapImage(new Uri("https://merge.devgregw.com/util/img_preview_error.png"));
                _imageOk = false;
            }
        }*/

        private void UpdateVideoPreview(object sender, RoutedEventArgs e) {
            if (!_ready)
                return;
            try {
                var url = (videoVendorBox.SelectedIndex == 0
                              ? "https://www.youtube.com/embed/"
                              : "https://player.vimeo.com/video/") + videoIdBox.Text;
                var req = (HttpWebRequest) WebRequest.Create(url);
                req.Method = "HEAD";
                var res = (HttpWebResponse) req.GetResponse();
                if (res.StatusCode == HttpStatusCode.NotFound)
                    throw new Exception();
                res.Close();
                videoPreview.Navigate(
                    new Uri(url));
                _videoOk = true;
            } catch (Exception ex) {
                videoPreview.Navigate(new Uri("https://merge.devgregw.com/util/img_preview_error.png"));
                _videoOk = false;
            }
        }

        private void OtherType(object sender, RoutedEventArgs e) {
            NavigationService.GoBack();
        }

        private bool Validate() {
            var errors = new List<string>();
            switch (_type) {
                case 0:
                    errors.Add(string.IsNullOrWhiteSpace(buttonLabelBox.Text) ? "No button label specified." : "");
                    errors.Add(buttonAction.SelectedAction == null ? "No action specified." : "");
                    break;
                case 1:
                    errors.Add(string.IsNullOrWhiteSpace(labelTextBox.Text) ? "No label text specified." : "");
                    break;
                case 2:
                    /*UpdateImagePreview(null, null);
                    errors.Add(!_imageOk ? "The specified image is invalid." : "");*/
                    errors.Add(string.IsNullOrWhiteSpace(imageUploader.Value) ? "No image specified." : "");
                    break;
                case 3:
                    UpdateVideoPreview(null, null);
                    errors.Add(!_videoOk ? "The specified video is invalid." : "");
                    break;
            }
            errors.RemoveAll(string.IsNullOrWhiteSpace);
            if (!errors.Any()) return true;
            var text = errors.Aggregate("", (current, s) => current + $"{s}\n");
            MessageBox.Show(_window, "Please resolve the following errors:\n\n" + text, "Input Invalid",
                MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            return false;
        }

        private async void Done(object sender, RoutedEventArgs e) {
            if (!Validate())
                return;
            switch (_type) {
                case 0:
                    _window.Element = new ButtonElement {
                        Label = buttonLabelBox.Text,
                        Action = buttonAction.SelectedAction
                    };
                    break;
                case 1:
                    _window.Element = new LabelElement {
                        Label = labelTextBox.Text,
                        Size = labelSizeBox.Value.GetValueOrDefault(12),
                        Style = ((ComboBoxItem) labelStyleBox.SelectedItem).Tag.ToString().ToEnum<LabelStyle>(true)
                    };
                    break;
                case 2:
                    /*_window.Element = new ImageElement {
                        ScaleType = ((ComboBoxItem) scaleTypeBox.SelectedItem).Tag.ToString().ToEnum<ScaleType>(true),
                        Url = sourceBox.Text
                    };*/
                    var reference = new LoaderReference(image);
                    reference.StartLoading("Processing...");
                    _window.IsEnabled = false;
                    var file =
                        await imageUploader.PerformChangesAsync(
                            $"ImageElement-{DateTime.Now.ToString("MMddyyyy\"-\"hhmmss", CultureInfo.CurrentUICulture)}"
                                .ToLower());
                    _window.Element = new ImageElement {
                        ScaleType = ScaleType.Uniform,
                        Url = file
                    };
                    break;
                case 3:
                    _window.Element = new VideoElement {
                        VideoId = videoIdBox.Text,
                        Vendor =
                            ((ComboBoxItem) videoVendorBox.SelectedItem).Content.ToString().ToEnum<VideoVendor>(true)
                    };
                    break;
            }
            _window.Close();
        }
    }
}
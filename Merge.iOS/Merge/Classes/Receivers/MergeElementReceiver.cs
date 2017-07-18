#region LICENSE

// Project Merge.iOS:  MergeElementReceiver.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 06/28/2017 at 7:33 AM.
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
using AVKit;
using Merge.Classes.Helpers;
using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Interfaces.Receivers;
using MergeApi.Models.Elements;
using UIKit;
using Xamarin.Forms;

#endregion

namespace Merge.Classes.Receivers {
    public sealed class MergeElementReceiver : IElementCreationReceiver {
        public T CreateButtonElement<T>(ButtonElement element) {
            var b = new Button {
                Text = element.Label,
                TextColor = Color.Black,
                BackgroundColor = ColorConsts.AccentColor.ToFormsColor(),
                BorderWidth = 1d,
                BorderColor = Color.Black
            };
            if (element.Action != null)
                b.Clicked += (s, e) => element.Action.Invoke();
            return (dynamic) b;
        }

        public T CreateLabelElement<T>(LabelElement element) {
            return (dynamic) new Label {
                Text = element.Label,
                FontAttributes = element.Style.ToFontAttributes(),
                FontSize = Convert.ToDouble(element.Size),
                TextColor = Color.Black
            };
        }

        public T CreateImageElement<T>(ImageElement element) {
            return (dynamic) new Image {
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Source = ImageSource.FromUri(new Uri(element.Url)),
                Aspect = element.ScaleType.ToAspect()
            };
        }

        public T CreateVideoElement<T>(VideoElement element) {
            return (dynamic) new WebView {
                Source = element.Vendor == VideoVendor.YouTube
                             ? $"https://www.youtube.com/embed/{element.VideoId}"
                             : $"https://player.vimeo.com/video/{element.VideoId}"
            };
        }
    }
}
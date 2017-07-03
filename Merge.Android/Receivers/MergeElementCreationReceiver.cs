#region LICENSE

// Project Merge.Android:  MergeElementCreationReceiver.cs (in Solution Merge.Android)
// Created by Greg Whatley on 06/30/2017 at 9:27 AM.
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

using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Merge.Android.Helpers;
using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Interfaces.Receivers;
using MergeApi.Models.Elements;
using MergeApi.Tools;
using Utilities = Merge.Android.Helpers.Utilities;

#endregion

namespace Merge.Android.Receivers {
    public sealed class MergeElementCreationReceiver : IElementCreationReceiver {
        private Color _color;
        private Context _context;

        private Theme _theme;

        public MergeElementCreationReceiver(Context c) {
            _context = c;
        }

        public T CreateButtonElement<T>(ButtonElement element) {
            var button = new Button(_context) {Text = element.Label};
            if (SdkChecker.Lollipop) {
                button.SetBackgroundColor(_color);
                button.SetTextColor(_theme == Theme.Dark
                    ? Color.Black
                    : _theme == Theme.Light
                        ? Color.White
                        : _color.ContrastColor());
            }
            button.Click += (s, e) => element.Action.Invoke();
            return (dynamic) button;
        }

        public T CreateLabelElement<T>(LabelElement element) {
            var text = new TextView(_context) {
                Text = element.Label,
                TextSize = element.Size
            };
            text.SetTypeface(Typeface.SansSerif, element.Style.Manipulate(e => {
                switch (e) {
                    case LabelStyle.Normal:
                        return TypefaceStyle.Normal;
                    case LabelStyle.Bold:
                        return TypefaceStyle.Bold;
                    case LabelStyle.Italic:
                        return TypefaceStyle.Italic;
                    case LabelStyle.BoldItalic:
                        return TypefaceStyle.BoldItalic;
                    default:
                        return TypefaceStyle.Normal;
                }
            }));
            text.SetTextColor(Color.Black);
            return (dynamic) text;
        }

        public T CreateImageElement<T>(ImageElement element) {
            var image = new ImageView(_context);
            image.SetScaleType(element.ScaleType.Manipulate(t => {
                switch (t) {
                    case ScaleType.None:
                        return ImageView.ScaleType.Center;
                    case ScaleType.Fill:
                        return ImageView.ScaleType.FitXy;
                    case ScaleType.Uniform:
                        return ImageView.ScaleType.CenterInside;
                    case ScaleType.UniformToFill:
                        return ImageView.ScaleType.CenterCrop;
                    default:
                        return ImageView.ScaleType.CenterInside;
                }
            }));
            Utilities.LoadImageForDisplay(element.Url, image);
            image.SetAdjustViewBounds(true);
            image.LayoutParameters =
                new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            return (dynamic) image;
        }

        public T CreateVideoElement<T>(VideoElement element) {
            var wv = new WebView(_context) {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                    _context.Resources.DisplayMetrics.WidthPixels / 2)
            };
            wv.Settings.JavaScriptEnabled = true;
            wv.LoadUrl((element.Vendor.ToString().ToLower() == "youtube"
                           ? "https://www.youtube.com/embed/"
                           : "https://player.vimeo.com/video/") + element.VideoId);
            return (dynamic) wv;
        }

        public void SetColorInfo(Color c, Theme t) {
            _color = c;
            _theme = t;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Merge.Android.Classes.Helpers;
using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Interfaces.Receivers;
using MergeApi.Models.Elements;
using MergeApi.Tools;

namespace Merge.Android {
    public sealed class MergeElementCreationReceiver : IElementCreationReceiver {
        private Context _context;

        private Color _color;

        private Theme _theme;

        public MergeElementCreationReceiver(Context c) {
            _context = c;
        }

        public void SetColorInfo(Color c, Theme t) {
            _color = c;
            _theme = t;
        }

        public T CreateButtonElement<T>(ButtonElement element) {
            var button = new Button(_context) { Text = element.Label };
            if ((int)Build.VERSION.SdkInt >= (int)BuildVersionCodes.Lollipop) {
                button.SetBackgroundColor(_color);
                button.SetTextColor(_theme == Theme.Dark ? Color.Black : _theme == Theme.Light ? Color.White : _color.ContrastColor());
            }
            button.Click += (s, e) => element.Action.Invoke();
            return (dynamic)button;
        }

        public T CreateLabelElement<T>(LabelElement element) {
            var text = new TextView(_context) {
                Text = element.Label,
                TextSize = element.Size
            };
            text.SetTypeface(Typeface.SansSerif, element.Style.Convert(e => {
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
            return (dynamic)text;
        }

        public T CreateImageElement<T>(ImageElement element) {
            var image = new ImageView(_context);
            image.SetScaleType(element.ScaleType.Convert(t => {
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
            ApiAccessor.LoadImageForDisplay(element.Url, image);
            image.SetAdjustViewBounds(true);
            image.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            return (dynamic)image;
        }

        public T CreateVideoElement<T>(VideoElement element) {
            var wv = new WebView(_context) {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, _context.Resources.DisplayMetrics.WidthPixels / 2)
            };
            wv.Settings.JavaScriptEnabled = true;
            wv.LoadUrl((element.Vendor.ToString().ToLower() == "youtube"
                           ? "https://www.youtube.com/embed/"
                           : "https://player.vimeo.com/video/") + element.VideoId);
            return (dynamic)wv;
        }
    }
}
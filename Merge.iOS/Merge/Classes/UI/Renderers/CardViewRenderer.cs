#region LICENSE

// Project Merge.iOS:  CardViewRenderer.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/08/2017 at 10:37 AM.
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
using System.ComponentModel;
using CoreGraphics;
using Merge.Classes.UI.Controls;
using Merge.Classes.UI.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

#endregion

[assembly: ExportRenderer(typeof(DataView), typeof(CardViewRenderer))]
[assembly: ExportRenderer(typeof(BlankCardView), typeof(CardViewRenderer))]
[assembly: ExportRenderer(typeof(TipView), typeof(CardViewRenderer))]

namespace Merge.Classes.UI.Renderers {
    public sealed class CardViewRenderer : ViewRenderer {
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.OnElementPropertyChanged(sender, e);
            AddShadow();
        }

        protected override void OnElementChanged(ElementChangedEventArgs<View> e) {
            base.OnElementChanged(e);
            AddShadow();
        }

        private void AddShadow() {
            nfloat radius = 10f;
            try {
                Subviews[0].Layer.CornerRadius = radius;
                Subviews[0].Layer.MasksToBounds = true;
            } catch {
                // ignored
            }
            Layer.CornerRadius = radius;
            Layer.MasksToBounds = false;
            //Layer.ShadowOffset = new CGSize(0d, 5d);
            Layer.BorderColor = UIColor.Black.CGColor;
            Layer.BorderWidth = 0.5f;
            //Layer.ShadowColor = UIColor.Black.CGColor;
            //Layer.ShadowOpacity = 0.5f;
            //Layer.ShadowRadius = radius;
            //Layer.ShadowPath = UIBezierPath.FromRoundedRect(Layer.Bounds, Layer.CornerRadius).CGPath;
            /*Layer.BackgroundColor = BackgroundColor.CGColor;
            BackgroundColor = null;*/
        }

        public override void Draw(CGRect rect) {
            base.Draw(rect);
            AddShadow();
        }
    }
}
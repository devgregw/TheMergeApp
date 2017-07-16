#region LICENSE

// Project Merge.iOS:  TipView.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/08/2017 at 4:50 PM.
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

using Merge.Classes.Helpers;
using Merge.iOS.Helpers;
using MergeApi.Models.Core.Tab;
using MergeApi.Tools;
using Xamarin.Forms;

#endregion

namespace Merge.Classes.UI.Controls {
    public class TipView : ContentView {
        public TipView(TabTip tip) {
            var messageLayout = new StackLayout {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.Center,
                Spacing = 0d,
                Children = {
                    new IconView(Images.Tip, new Label {
                        Text = tip.Message,
                        FontSize = 18d,
                        TextColor = Color.Black,
                        LineBreakMode = LineBreakMode.WordWrap
                    })
                }
            };
            if (tip.Action != null) {
                messageLayout.Children.Add(new IconView(Images.TipHasAction, new Label {
                    Text = "Tap to open",
                    FontSize = 14d,
                    TextColor = Color.Gray
                }));
                GestureRecognizers.Add(new TapGestureRecognizer {
                    Command = new Command(tip.Action.Invoke)
                });
            }
            var mainLayout = new StackLayout {
                Padding = new Thickness(5),
                BackgroundColor = ColorConsts.AccentColor.ToFormsColor(),
                Orientation = StackOrientation.Horizontal,
                Children = {
                    messageLayout
                }
            };
            if (!tip.Persistent)
                mainLayout.Children.Add(new Button {
                    Image = Images.Dismiss,
                    TextColor = Color.Black,
                    BackgroundColor = Color.Transparent,
                    WidthRequest = 30d,
                    VerticalOptions = LayoutOptions.Fill
                }.Manipulate(b => {
                    b.Clicked += (s, e) => {
                        ((StackLayout) Parent).Children.Remove(this);
                        PreferenceHelper.AddDismissedTip(tip.Id);
                    };
                    return b;
                }));
            BackgroundColor = ColorConsts.AccentColor.ToFormsColor();
            Content = mainLayout;
            Margin = new Thickness(8, 0, 8, 0);
        }
    }
}
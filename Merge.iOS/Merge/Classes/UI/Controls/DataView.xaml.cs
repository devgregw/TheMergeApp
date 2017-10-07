#region LICENSE

// Project Merge.iOS:  DataView.xaml.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 06/28/2017 at 9:11 PM.
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
using System.Linq;
using Merge.Classes.Helpers;
using Merge.Classes.UI.Pages;
using Merge.iOS.Helpers;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Core;
using MergeApi.Tools;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

#endregion

namespace Merge.Classes.UI.Controls {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DataView : ContentView {
        public DataView() {
            InitializeComponent();
        }

        public DataView(MergeEvent e, ValidationResult r) : this() {
            Initialize(e.Title, e.ShortDescription, e.CoverImage,
                () => Navigation.PushAsync(new DataDetailPage(e), true), r == null || r.ResultType == ValidationResultType.Success ? null : new IconView(Images.Error, new Label {
                    Text = "Validation Failure"
                }));
        }

        public DataView(MergePage p, ValidationResult r) : this() {
            Initialize(p.Title, p.ShortDescription, p.CoverImage,
                () => Navigation.PushAsync(new DataDetailPage(p), true), p.ButtonAction != null
                    ? new Button {
                        Text = p.ButtonLabel,
                        TextColor = p.Color.ToFormsColor().ContrastColor(p.Theme),
                        BackgroundColor = p.Color.ToFormsColor(),
                        BorderWidth = 0.5d,
                        BorderColor = p.Color.ToFormsColor().ContrastColor(p.Theme),
                        Command = new Command(p.ButtonAction.Invoke)
                    }
                    : null, p.LeadersOnly
                    ? new IconView(Images.PasswordProtected, new Label {
                        Text = "Leaders Only",
                        TextColor = Color.Black,
                        FontSize = 14d
                    })
                    : null,
                p.Hidden ? new IconView(Images.NoVisibleContent, new Label {
                    Text = "Hidden"
                }) : null, r == null || r.ResultType == ValidationResultType.Success ? null : new IconView(Images.Error, new Label {
                    Text = "Validation Failure"
                }));
        }

        public DataView(MergeGroup g) : this() {
            Initialize(g.Name, $"Lead by {g.LeadersFormatted}", g.CoverImage,
                () => Navigation.PushAsync(new DataDetailPage(g), true));
        }

        private void Initialize(string header, string sub, string imageUrl, Action onClick, params View[] extraViews) {
            title.Text = header;
            description.Text = sub;
            cover.Source = ImageSource.FromUri(new Uri(imageUrl));
            foreach (var v in extraViews.Where(o => o != null))
                content.Children.Add(v);
            GestureRecognizers.Add(new TapGestureRecognizer {
                Command = new Command(onClick)
            });
        }
    }
}
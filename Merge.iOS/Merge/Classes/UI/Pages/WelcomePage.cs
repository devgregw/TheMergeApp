#region LICENSE

// Project Merge.iOS:  WelcomePage.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/07/2017 at 10:26 AM.
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
using Merge.Classes.Helpers;
using MergeApi.Models.Actions;
using MergeApi.Tools;
using Xamarin.Forms;

#endregion

namespace Merge.Classes.UI.Pages {
    public class WelcomePage : ContentPage {
        private Action _callback;

        public WelcomePage(Action callback) {
            _callback = callback;
            Title = "Setup";
            Content = new StackLayout();
            ShowWelcomeView();
        }

        public WelcomePage() : this(() => { }) { }

        private async void SetView(string title, string message, string[] buttons, Action[] buttonClicks) {
            Button MakeButton(string text, Action click) {
                return new Button {
                    Text = text,
                    WidthRequest = 80,
                    BackgroundColor = ColorConsts.AccentColor.ToFormsColor(),
                    TextColor = Color.Black
                }.Manipulate(b => {
                    b.Clicked += delegate { click(); };
                    return b;
                });
            }

            await Content.FadeTo(0);
            var buttonPanel = new StackLayout {
                Spacing = 5,
                HorizontalOptions = LayoutOptions.Center,
                Orientation = StackOrientation.Horizontal
            };
            for (var i = 0; i < buttons.Length; i++)
                buttonPanel.Children.Add(MakeButton(buttons[i], buttonClicks[i]));
            var all = new ScrollView {
                Content = new StackLayout {
                    Margin = new Thickness(10d),
                    VerticalOptions = LayoutOptions.Center,
                    Spacing = 5,
                    Children = {
                        new Label {
                            Text = title,
                            TextColor = Color.Black,
                            FontSize = 34d,
                            HorizontalTextAlignment = TextAlignment.Center,
                            LineBreakMode = LineBreakMode.WordWrap
                        },
                        new Label {
                            Text = message,
                            TextColor = Color.Black,
                            FontSize = 14d,
                            HorizontalTextAlignment = TextAlignment.Center,
                            LineBreakMode = LineBreakMode.WordWrap
                        },
                        buttonPanel
                    }
                }
            };
            await all.FadeTo(0d, 0U);
            Content = all;
            await Content.FadeTo(1d);
        }

        private void ShowWelcomeView() {
            SetView("Welcome to Merge",
                "Welcome to the official Merge app!  The Merge app is designed to be your one-stop shop for all things Merge.  To use the Merge app, follow the steps on-screen to complete this brief setup.",
                new[] {"Start"},
                new Action[] {ShowLegalView});
        }

        private void ShowLegalView() {
            SetView("Here's the Legal Stuff",
                "By using the Merge app, you agree to the MIT license as well as all third-party licenses specified.  Tap 'Licenses' to learn more, or visit the 'About Merge' page after you complete the setup.",
                new[] {"Back", "Licenses", "I Agree"},
                new Action[] {
                    ShowWelcomeView, () => { LaunchUriAction.FromUri("https://merge.gregwhatley.dev/licenses").Invoke(); },
                    ShowLeaderView
                });
        }

        private void ShowLeaderView() {
            SetView("Are You a Leader?",
                "Leaders have special access to protected features.  If you are a leader and have been given a username and password, tap 'Yes'.  Otherwise, tap 'No'.",
                new[] {"Back", "No", "Yes"},
                new Action[] {ShowLegalView, ShowTargetingView, ShowLeaderAuthenticationPage});
        }

        private async void ShowLeaderAuthenticationPage() {
            await Navigation.PushModalAsync(new LeaderAuthenticationPage(result => {
                if (result)
                    ShowTargetingView();
            }).WrapInNavigationPage());
        }

        private void ShowTargetingView() {
            SetView("Tell Us About Yourself",
                "Merge can filter out content this is irrelevant to you.  If you're a leader or parent, you may choose to select the grade level(s) and gender(s) that apply to your student(s).  By default, this feature is disabled.  To enable it, tap 'Switch On' or visit the 'Settings' page after you complete the setup.",
                new[] {"Back", "Switch On", "Skip"},
                new Action[] {ShowLeaderView, ShowTargetingPage, ShowCompleteView});
        }

        private async void ShowTargetingPage() {
            await Navigation.PushModalAsync(new TargetingPage(ShowCompleteView).WrapInNavigationPage());
        }

        private void ShowCompleteView() {
            SetView("Setup Complete", "The Merge app is ready!  Tap 'Finish' to get started now.",
                new[] {"Back", "Finish"},
                new Action[] {
                    ShowTargetingView, async () => {
                        PreferenceHelper.HasLaunchedBefore = true;
                        await Navigation.PopModalAsync();
                        _callback();
                    }
                });
        }
    }
}
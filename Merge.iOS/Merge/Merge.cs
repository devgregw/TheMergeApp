#region LICENSE

// Project Merge.iOS:  Merge.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 10/28/2016 at 9:16 AM.
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
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Merge.Classes;
using Merge.Classes.Helpers;
using Merge.Classes.Receivers;
using Merge.Classes.UI;
using Merge.Classes.UI.Pages;
using Merge.Classes.UI.Pages.LeadersOnly;
using Merge.iOS;
using Merge.iOS.Helpers;
using MergeApi.Framework.Abstractions;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Application = Xamarin.Forms.Application;
using ui = UIKit;

#endregion

namespace Merge {
    public class App : Application {
        public static ActionBase NotificationAction;

        private LoadingOverlay _overlay;

        private TabbedPage _tabbedPage;

        public App() {
            MessagingCenter.Subscribe<IMessageSender>(this, ShortcutIndentifiers.HomeShortcut, async s => {
                await MainPage.Navigation.PopToRootAsync();
                ReInitTabs(0);
            });
            MessagingCenter.Subscribe<IMessageSender>(this, ShortcutIndentifiers.EventsShortcut, async s => {
                await MainPage.Navigation.PopToRootAsync();
                ReInitTabs(1);
            });
            MessagingCenter.Subscribe<IMessageSender>(this, ShortcutIndentifiers.GroupsShortcut, async s => {
                await MainPage.Navigation.PopToRootAsync();
                ReInitTabs(2);
            });
            InitMainPage();
            MergeLogReceiver.Log("appStarted", new Dictionary<string, string>());
        }

        public void ShowLoader(string message) {
            _overlay = new LoadingOverlay(message, ui.UIScreen.MainScreen.Bounds,
                ui.UIApplication.SharedApplication.KeyWindow.RootViewController);
            _overlay.Show();
        }

        public void HideLoader() {
            _overlay.Hide();
        }

        public void InitMainPage() {
            _tabbedPage = new TabbedPage {
                BackgroundColor = Color.FromHex(ColorConsts.PrimaryLightColor),
                Title = "Merge"
            };
            _tabbedPage.AddToolbarItem("Menu", Images.MoreVertical, (s, e) => {
                var menuItems = new List<string> {
                    "Refresh",
                    "About Merge",
                    "Settings",
                    "Contact Us"
                };
                if (PreferenceHelper.IsValidLeader)
                    menuItems.Add("Leader Resources");
                AlertHelper.ShowSheet("The Merge App",
                    b => {
                        if (b == "Close")
                            return;
                        var items = menuItems.ToArray();
                        if (b == items[0]) {
                            TabPageDelegates.Instance.Nullify();
                            ReInitTabs();
                        } else if (b == items[1]) {
                            ((NavigationPage) MainPage).Navigation.PushAsync(new AboutPage());
                        } else if (b == items[2]) {
                            ((NavigationPage) MainPage).Navigation.PushAsync(new SettingsPage());
                        } else if (b == items[3]) {
                            EmailAction.FromAddress("students@pantego.org").Invoke();
                        } else if (b == items[4]) {
                            ((NavigationPage) MainPage).Navigation.PushAsync(new LeaderResourcesPage());
                        }
                    }, "Close", null, ((ToolbarItem) s).ToUIBarButtonItem(), menuItems.ToArray());
            });
            _tabbedPage.Children.Add(new ContentPage {
                BackgroundColor = Color.FromHex(ColorConsts.PrimaryLightColor)
            });
            _tabbedPage.Appearing += (sender, e) => {
                ui.UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
                new Action(async () => {
                    if (!PreferenceHelper.HasLaunchedBefore)
                        await MainPage.Navigation.PushModalAsync(new WelcomePage(() => ReInitTabs())
                            .WrapInNavigationPage());
                    else
                        ReInitTabs();
                    /*if (NotificationAction != null)
						await ActionBase.Invoke(NotificationAction);
					NotificationAction = null;*/
                }).Invoke();
            };
            MainPage = _tabbedPage.WrapInNavigationPage();
        }

        public void ReInitTabs(int i = -1) {
            var selection = i == -1 ? _tabbedPage.Children.IndexOf(_tabbedPage.CurrentPage) : i;
            _tabbedPage.Children.Clear();
            _tabbedPage.Children.Add(new GenericTabPage<MergePage, int>("Home", Images.Home, TabPageDelegates.Instance,
                TabPageDelegates.Instance));
            _tabbedPage.Children.Add(new GenericTabPage<MergeEvent, DateTime>("Upcoming Events", Images.UpcomingEvents,
                TabPageDelegates.Instance, TabPageDelegates.Instance));
            _tabbedPage.Children.Add(new GenericTabPage<MergeGroup, double>("Merge Groups", Images.MergeGroups,
                TabPageDelegates.Instance, TabPageDelegates.Instance));
            //_tabbedPage.Children.Add(new AnnouncementsPage { Icon = "announcements.png" });
            try {
                _tabbedPage.SelectedItem = _tabbedPage.Children[selection];
            } catch {
                _tabbedPage.SelectedItem = 0;
            }
        }

        protected override void OnStart() { }

        protected override void OnSleep() {
            // Handle when your app sleeps
        }

        /*protected override void OnResume() {
            GenericTabPage page;
            if ((page = (GenericTabPage)_tabbedPage.CurrentPage) != null)
                page.Resume();
        }*/

        public sealed class LoadingOverlay : ui.UIView {
            private ui.UIButton _button;
            private ui.UIViewController _controller;
            private ui.UILabel _label;
            private bool _showing;
            private ui.UIActivityIndicatorView _spinner;

            public LoadingOverlay(string msg, CGRect frame, ui.UIViewController controller) : base(frame) {
                BackgroundColor = ui.UIColor.Black;
                Alpha = 0.75f;
                AutoresizingMask = ui.UIViewAutoresizing.All;
                nfloat labelHeight = 22,
                    labelWidth = Frame.Width - 20,
                    centerX = Frame.Width / 2,
                    centerY = Frame.Height / 2;
                _controller = controller;
                _spinner = new ui.UIActivityIndicatorView(ui.UIActivityIndicatorViewStyle.WhiteLarge);
                _spinner.Frame = new CGRect(centerX - _spinner.Frame.Width / 2, centerY - _spinner.Frame.Height - 20,
                    _spinner.Frame.Width, _spinner.Frame.Height);
                _spinner.AutoresizingMask = ui.UIViewAutoresizing.All;
                AddSubview(_spinner);
                _spinner.StartAnimating();
                _label = new ui.UILabel(new CGRect(centerX - labelWidth / 2, centerY + 20, labelWidth, labelHeight)) {
                    BackgroundColor = ui.UIColor.Clear,
                    TextColor = ui.UIColor.White,
                    Text = msg,
                    TextAlignment = ui.UITextAlignment.Center,
                    AutoresizingMask = ui.UIViewAutoresizing.All
                };
                _button =
                    new ui.UIButton(new CGRect(centerX - labelWidth / 2, centerY + 40, labelWidth, labelHeight)) {
                        BackgroundColor = ui.UIColor.Clear
                    };
                _button.SetTitleColor(ui.UIColor.Clear, ui.UIControlState.Normal);
                AddSubview(_label);
            }

            public async void Show() {
                _showing = true;
                Alpha = 0f;
                _controller.View.Add(this);
                Animate(0.25d, () => Alpha = 0.75f);
                await Task.Run(() => Thread.Sleep(1));
                if (_showing)
                    AddSubview(_button);
            }

            public void Hide() {
                _showing = false;
                Animate(0.25, () => Alpha = 0f, RemoveFromSuperview);
            }
        }
    }
}
#region LICENSE

// Project Merge.iOS:  DataDetailPage.xaml.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/03/2017 at 12:52 PM.
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
using CoreLocation;
using MapKit;
using Merge.Classes.Helpers;
using Merge.Classes.UI.Controls;
using Merge.iOS.Helpers;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using MergeApi.Tools;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

#endregion

namespace Merge.Classes.UI.Pages {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DataDetailPage : ContentPage {
        private Dictionary<string, Action> _menuItems;

        public DataDetailPage() {
            InitializeComponent();
            _menuItems = new Dictionary<string, Action>();
            scroller.PropertyChanged += (sender, e) => {
                cover.WidthRequest = Width;
                dataList.WidthRequest = Width;
                if (scroller.ScrollY < 0)
                    cover.Layout(new Rectangle(cover.Bounds.X, scroller.ScrollY, cover.Bounds.Width,
                        175 - scroller.ScrollY));
                else
                    cover.Layout(new Rectangle(cover.Bounds.X, 0, cover.Bounds.Width,
                        175));
            };
        }

        public DataDetailPage(MergeEvent e) : this() {
            Colorize(e.Color.ToFormsColor(), e.Theme);
            SetTitle(e.Title);
            cover.Source = ImageSource.FromUri(new Uri(e.CoverImage));
            AddLabel(Images.Info, e.Description, true, true);
            AddLabel(Images.Cost, e.Price.GetValueOrDefault(0d) == 0d ? "Free" : $"${e.Price}");

            string MakeDateString(DateTime start, DateTime end) {
                return
                    $"Starts on {start.ToLongDateString()} at {start.ToString("h:mm tt", CultureInfo.CurrentUICulture)}\nand ends on {end.ToLongDateString()} at {end.ToString("h:mm tt", CultureInfo.CurrentUICulture)}";
            }

            var dateString = MakeDateString(e.StartDate.Value, e.EndDate.Value);
            if (e.RecurrenceRule != null) {
                var begin = RecurrenceRule.GetNextOccurrence(e.StartDate.Value, e.RecurrenceRule);
                if (begin.HasValue) {
                    var diff = e.EndDate.Value - e.StartDate.Value;
                    var end = begin.Value.AddMilliseconds(diff.TotalMilliseconds);
                    dateString = MakeDateString(begin.Value, end) +
                                 $"\n{RecurrenceRule.GetRuleDescription(e.StartDate.Value, e.RecurrenceRule)}";
                }
            }
            AddLabel(Images.DateTime, dateString);
            AddLabel(Images.Location, $"{e.Location}{(string.IsNullOrWhiteSpace(e.Address) ? "" : $"\n{e.Address}")}");
            if (e.HasRegistration)
                AddLabel(Images.RegistrationRequired,
                    $"Registration is required\nRegistration closes on {e.RegistrationClosingDate.Value.ToString("dddd, d MMMM yyyy \"at\" h:mm tt", CultureInfo.CurrentUICulture)}");
            AddLabel(Images.Targeting, ApiAccessor.GetTargetingString(e));
            _menuItems.Add("Add To Calendar", AddToCalendarAction.FromEventId(e.Id).Invoke);
            if (!string.IsNullOrWhiteSpace(e.Address))
                _menuItems.Add("Get Directions", GetDirectionsAction.FromEventId(e.Id).Invoke);
            if (e.HasRegistration)
                _menuItems.Add("Register", () => {
                    void Register() =>
                        LaunchUriAction.FromUri(e.RegistrationUrl).Invoke();

                    if (e.RegistrationClosingDate.GetValueOrDefault(DateTime.MaxValue) < DateTime.Now)
                        AlertHelper.ShowAlert("Event Registration",
                            $"The registration for \"{e.Title}\" has closed.  Do you want to view the registration page anyway?",
                            (v, i) => {
                                if (i == 1)
                                    Register();
                            }, "No", "Yes");
                    else
                        Register();
                });
            InitializeMenu();
        }

        public DataDetailPage(MergePage p) : this() {
            if (p.LeadersOnly && !PreferenceHelper.IsValidLeader) {
                AlertHelper.ShowAlert("Unauthorized",
                    "You are not authorized to view this page.  If you believe this is an error, turn on leader-only features in Settings or contact us.",
                    async (a, i) => {
                        if (i == a.CancelButtonIndex) {
                            await Navigation.PopAsync(true);
                        } else if (i == a.FirstOtherButtonIndex) {
                            await Navigation.PopAsync(true);
                            EmailAction.FromAddress("students@pantego.org").Invoke();
                        } else {
                            await Navigation.PopAsync(true);
                            await Navigation.PushAsync(new SettingsPage(), true);
                        }
                    }, "Dismiss", "Contact Us", "Settings");
                return;
            }
            Colorize(p.Color.ToFormsColor(), p.Theme);
            SetTitle(p.Title);
            cover.Source = ImageSource.FromUri(new Uri(p.CoverImage));
            if (!string.IsNullOrWhiteSpace(p.Description))
                AddLabel(Images.Info, p.Description, true, true);
            foreach (var element in p.Content) {
                var v = element.CreateView<View>();
                if (!(v is Button || v is Label))
                    v.Margin = new Thickness(-6, 0, -6, 0);
                if (v is Button) {
                    ((Button) v).BackgroundColor = Color;
                    ((Button) v).TextColor = Color.ContrastColor(Theme);
                } else if (v is WebView) {
                    v.WidthRequest = UIApplication.SharedApplication.KeyWindow.Bounds.Width;
                    v.HeightRequest = UIApplication.SharedApplication.KeyWindow.Bounds.Width / 3 * 2;
                }
                dataList.Children.Add(v);
            }
            if (p.LeadersOnly)
                AddLabel(Images.PasswordProtected, "Leaders Only");
            AddLabel(Images.Targeting, ApiAccessor.GetTargetingString(p));
            _menuItems.Add("Reload", async () => {
                await Navigation.PushAsync(new DataDetailPage(p));
                Navigation.RemovePage(this);
            });
            InitializeMenu();
        }

        public DataDetailPage(MergeGroup g) : this() {
            Colorize(Color.FromHex(ColorConsts.AccentColor), Theme.Dark);
            SetTitle(g.Name);
            cover.Source = ImageSource.FromUri(new Uri(g.CoverImage));
            AddLabel(Images.Info, $"Lead by {g.LeadersFormatted}", true, true);
            AddLabel(Images.Location, g.Address);
            AddLabel(Images.Home, $"Hosted by {g.Host}");
            dataList.Children.Add(new MapView(new List<MKAnnotation> {
                new MapAnnotation(g.Coordinates.Manipulate(
                    p => new CLLocationCoordinate2D(Convert.ToDouble(p.Latitude), Convert.ToDouble(p.Longitude))))
            }, a => { }, false, false) {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = UIApplication.SharedApplication.KeyWindow.Bounds.Width,
                Margin = new Thickness(-6, 0, -6, 0)
            });
            _menuItems.Add("Get Directions", () => GetDirectionsAction.FromGroupId(g.Id).Invoke());
            _menuItems.Add("See All Groups", () => new OpenGroupMapPageAction().Invoke());
            _menuItems.Add("Contact Group Leaders", () => ShowContactInfoAction.FromGroupId(g.Id).Invoke());
            InitializeMenu();
        }

        public Color Color { get; set; }

        public Theme Theme { get; set; }

        private void InitializeMenu() {
            _menuItems.Add("Contact Us", EmailAction.FromAddress("students@pantego.org").Invoke);
            ToolbarItems.Add(new ToolbarItem("More", Images.MoreVertical, () => AlertHelper.ShowSheet(null, (s, i) => {
                if (i != s.CancelButtonIndex)
                    _menuItems.ElementAt((int) i).Value();
            }, "Close", null, _menuItems.Keys.ToArray())));
        }

        private void Colorize(Color c, Theme t) {
            Color = c;
            Theme = t;
        }

        private void SetTitle(string title) {
            Title = title;
            dataList.Children.Add(new Label {
                Text = title,
                TextColor = Color.ContrastColor(Theme),
                FontSize = 34d,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.WordWrap,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(-6d, -1d, -6d, 0d),
                BackgroundColor = Color
            });
        }

        private void AddLabel(string icon, string text, bool large = false, bool black = false) {
            dataList.Children.Add(new IconView(icon, new Label {
                Text = text,
                TextColor = black ? Color.Black : Color.Gray,
                FontSize = large ? 18d : 14d,
                LineBreakMode = LineBreakMode.WordWrap
            }));
        }
    }
}
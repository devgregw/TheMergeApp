﻿#region LICENSE

// Project Merge.iOS:  GenericTabPage.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/08/2017 at 11:09 AM.
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
using System.Linq;
using System.Threading.Tasks;
using Merge.Classes.Helpers;
using Merge.Classes.UI.Controls;
using Merge.iOS.Helpers;
using MergeApi;
using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Interfaces;
using MergeApi.Models.Actions;
using MergeApi.Models.Core.Tab;
using MergeApi.Tools;
using UIKit;
using Xamarin.Forms;

#endregion

namespace Merge.Classes.UI.Pages {
    public interface IGenericTabPageDelegate<T, out TSortBy> where T : IIdentifiable where TSortBy : IComparable {
        IEnumerable<T> GetItems();

        void SetItems(IEnumerable<T> value);

        TSortBy TransformForSorting(T input);

        View TransformIntoView(T input, ValidationResult r);

        bool DoesPassThroughFilter(T input);

        int GetTab();
    }

    public interface IGenericTabPageMetaDelegate {
        IEnumerable<TabTip> GetTips();

        void SetTips(IEnumerable<TabTip> value);

        bool TipDoesPassThroughFilter(TabTip tip);

        IEnumerable<TabHeader> GetHeaders();

        void SetHeaders(IEnumerable<TabHeader> value);
    }

    public abstract class GenericTabPage : ContentPage {
        private static Dictionary<int, double> _ys;

        public static Dictionary<int, double> ScrollYs => _ys ?? (_ys = new Dictionary<int, double>());

        public abstract void Resume();
    }

    public class GenericTabPage<T, TSortBy> : GenericTabPage
        where T : class, IIdentifiable where TSortBy : IComparable {
        private IGenericTabPageDelegate<T, TSortBy> _delegate;

        private bool _isLoading;

        private StackLayout _list;

        private IGenericTabPageMetaDelegate _metaDelegate;

        public GenericTabPage(string title, string icon, IGenericTabPageDelegate<T, TSortBy> @delegate,
            IGenericTabPageMetaDelegate metaDelegate) {
            _delegate = @delegate;
            _metaDelegate = metaDelegate;
            Title = title;
            Icon = icon;
            _list = new StackLayout {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Start,
                Orientation = StackOrientation.Vertical,
                Margin = new Thickness(0, 8, 0, 0)
            };
            Content = new ScrollView {
                Orientation = ScrollOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Content = _list,
                BackgroundColor = ColorConsts.PrimaryLightColor.ToFormsColor()
            };
            ((ScrollView) Content).Scrolled += (s, e) => ScrollYs[_delegate.GetTab()] = e.ScrollY;
        }

        private async Task ShowLoader() {
            if (!_isLoading)
                await SetViews(new[] {
                    new BlankCardView(new StackLayout {
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Orientation = StackOrientation.Horizontal,
                        Children = {
                            new ActivityIndicator {
                                Color = Color.Black,
                                WidthRequest = 25,
                                HeightRequest = 25,
                                Margin = new Thickness(0, 0, 5, 0),
                                IsRunning = true
                            },
                            new Label {
                                Text = "Loading...",
                                TextColor = Color.Black,
                                FontSize = 18d,
                                VerticalOptions = LayoutOptions.Center
                            }
                        }
                    })
                }, true, true);
        }

        private async Task ShowError(Exception ex) {
            await SetViews(new[] {
                new BlankCardView(new StackLayout {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Orientation = StackOrientation.Horizontal,
                    Children = {
                        new IconView(Images.NoContent,
                            new Label {
                                Text =
                                    $"An error occurred while loading content.\n{ex.Message} ({ex.GetType().FullName})",
                                TextColor = Color.Black,
                                FontSize = 14d
                            })
                    }
                })
            }, true);
        }

        private async Task SetViews(IReadOnlyCollection<View> views, bool fade, bool loading = false) {
            _isLoading = loading;
            if (fade)
                await _list.FadeTo(0d);
            _list.Children.Clear();
            foreach (var v in views) {
                if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                    if (!(v is Image))
                        v.Margin = new Thickness(UIApplication.SharedApplication.KeyWindow.Bounds.Width / 4, 0,
                            UIApplication.SharedApplication.KeyWindow.Bounds.Width / 4, 0);
                _list.Children.Add(v);
            }
            await ((ScrollView) Content).ScrollToAsync(0, ScrollYs.TryGetValue(_delegate.GetTab(), out var y) ? y : 0d,
                false);
            if (fade)
                await _list.FadeTo(1d);
        }

        public sealed override async void Resume() {
            var didLoadData = false;
            if (_delegate.GetItems() == null) {
                await ShowLoader();
                try {
                    await Task.Run(async () => _delegate.SetItems(await MergeDatabase.ListAsync<T>()));
                    didLoadData = true;
                } catch (Exception e) {
                    await ShowError(e);
                    return;
                }
            }
            if (_metaDelegate.GetTips() == null) {
                await ShowLoader();
                try {
                    await Task.Run(async () => _metaDelegate.SetTips(await MergeDatabase.ListAsync<TabTip>()));
                    didLoadData = true;
                } catch (Exception) {
                    _metaDelegate.SetTips(new List<TabTip>());
                }
            }
            if (_metaDelegate.GetHeaders() == null) {
                await ShowLoader();
                try {
                    await Task.Run(async () => _metaDelegate.SetHeaders(await MergeDatabase.ListAsync<TabHeader>()));
                    didLoadData = true;
                } catch (Exception) {
                    _metaDelegate.SetHeaders(new List<TabHeader>());
                }
            }
            Task<(T Object, ValidationResult Result)>[] validations = _delegate.GetItems()
                .Where(_delegate.DoesPassThroughFilter).OrderBy(_delegate.TransformForSorting).Select(async o =>
                    (o, o is IValidatable ? await ((IValidatable) o).ValidateAsync() : null)).ToArray();
            var all = (await Task.WhenAll(validations)).Where(o =>
                PreferenceHelper.ShowInvalidObjects || o.Result == null ||
                o.Result.ResultType == ValidationResultType.Success).ToList();
            //var all = _delegate.GetItems().OrderBy(_delegate.TransformForSorting).ToList();
            //var filteredMainContent = all.Where(_delegate.DoesPassThroughFilter).ToList();
            var tips = _metaDelegate.GetTips()
                .Where(t => (int) t.Tab == _delegate.GetTab() && _metaDelegate.TipDoesPassThroughFilter(t)).ToList();
            var content = tips.Select(t => new TipView(t))
                .Concat(all.Select(o => _delegate.TransformIntoView(o.Object, o.Result))).ToList();
            TabHeader header;
            if ((header = _metaDelegate.GetHeaders().FirstOrDefault(h => (int) h.Tab == _delegate.GetTab())) != null)
                if (!string.IsNullOrWhiteSpace(header.Image))
                    content.Insert(0, new Image {
                        HeightRequest = 200d,
                        Margin = new Thickness(0, -8, 0, 0),
                        Source = ImageSource.FromUri(new Uri(header.Image)),
                        Aspect = Aspect.AspectFill
                    });
            var nothingCard =
                new BlankCardView(new IconView(Images.NoContent, new Label {
                    Text = "No content available",
                    TextColor = Color.Black,
                    FontSize = 18d
                }));
            var mapButton = new Button {
                Text = "View a Map",
                BackgroundColor = ColorConsts.AccentColor.ToFormsColor(),
                TextColor = Color.Black,
                Margin = new Thickness(10, 0, 10, 0)
            }.Manipulate(b => {
                b.Clicked += (s, e) => new OpenGroupMapPageAction().Invoke();
                return b;
            });
            var views = content;
            if (!all.Any())
                views.Add(nothingCard);
            if (all.Any() && _delegate.GetTab() == 2)
                views.Add(mapButton);
            await SetViews(views, didLoadData);
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            Resume();
        }
    }
}
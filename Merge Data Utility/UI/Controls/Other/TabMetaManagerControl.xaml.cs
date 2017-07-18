#region LICENSE

// Project Merge Data Utility:  TabMetaManagerControl.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 06/23/2017 at 10:45 AM.
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MergeApi.Client;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Core.Tab;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages.Base;
using Merge_Data_Utility.UI.Windows;

#endregion

namespace Merge_Data_Utility.UI.Controls.Other {
    /// <summary>
    ///     Interaction logic for TabMetaManagerControl.xaml
    /// </summary>
    public partial class TabMetaManagerControl : UserControl {
        private Action _drafts;
        private TabHeader _header;
        private Tab _tab;

        private bool _visible = true, _inited;

        public TabMetaManagerControl() {
            InitializeComponent();
        }

        public void Init(Tab t, Action reloadDrafts) {
            _tab = t;
            _drafts = reloadDrafts;
            Init();
        }

        private async void InitHeader() {
            _header = await MergeDatabase.GetAsync<TabHeader>(_tab.ToString());
            if (string.IsNullOrWhiteSpace(_header?.Image)) {
                urlBox.Text = "<no header set>";
            } else {
                urlBox.Text = _header.Image;
                tabHeader.Source = new BitmapImage(new Uri(_header.Image));
            }
        }

        private async void InitTips(bool silent = false) {
            var reference = new LoaderReference(tipPanel);
            if (!silent)
                reference.StartLoading();
            try {
                var tips = (await MergeDatabase.ListAsync<TabTip>()).Where(t => t.Tab == _tab);
                tipsList.Children.Clear();
                tips.ForEach(tip => tipsList.Children.Add(ModelControl.Create(tip, false, t => {
                    new EditorWindow(t, false, r => {
                        if (r == EditorWindow.ResultType.Published)
                            InitTips();
                        else if (r == EditorWindow.ResultType.Saved)
                            _drafts();
                    }).Show();
                }, async t => {
                    if (MessageBox.Show("Are you sure you want to delete this tip?  This action cannot be undone.",
                            "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation,
                            MessageBoxResult.No) != MessageBoxResult.Yes) return;
                    await MergeDatabase.DeleteAsync((TabTip) t);
                    InitTips();
                })));
                //resp.Errors.ForEach(e => tipsList.Children.Add(new ErrorControl(e)));
                if (tipsList.Children.Count == 0)
                    tipsList.Children.Add(new TextBlock {
                        Text = "No tips found.",
                        Margin = new Thickness(5),
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    });
            } catch (Exception) {
                tipsList.Children.Clear();
                tipsList.Children.Add(new TextBlock {
                    Text = "No tips found.",
                    Margin = new Thickness(5),
                    FontStyle = FontStyles.Italic,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                });
            }
            if (!silent)
                reference.StopLoading();
        }

        private void Init() {
            InitHeader();
            InitTips();
            Loaded += (s, e) => {
                if (_inited)
                    InitTips(true);
            };
            _inited = true;
        }

        private async void HeaderSet_Click(object sender, RoutedEventArgs e) {
            var img = ImageInputWindow.GetUrl("Set Header: " + _tab, "Choose a new header image:", $"{_tab}Header",
                false);
            if (string.IsNullOrWhiteSpace(img)) return;
            await MergeDatabase.UpdateAsync(new TabHeader {
                Image = img,
                Tab = _tab
            });
            InitHeader();
        }

        private async void HeaderClear_Click(object sender, RoutedEventArgs e) {
            if (
                MessageBox.Show("Are you sure you want to remove this tab's header?", "Confirm", MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation, MessageBoxResult.No) == MessageBoxResult.Yes) {
                await MergeDatabase.UpdateAsync(new TabHeader {
                    Image = "",
                    Tab = _tab
                });
                InitHeader();
            }
        }

        private void TipNew_Click(object sender, RoutedEventArgs e) {
            new EditorWindow(EditorPage.GetPage(typeof(TabTip), null, false), r => {
                if (r == EditorWindow.ResultType.Published)
                    InitTips();
                else if (r == EditorWindow.ResultType.Saved)
                    _drafts();
            }).Show();
        }

        private async void TipClear_Click(object sender, RoutedEventArgs e) {
            if (tipsList.Children[0] is TextBlock) {
                MessageBox.Show("There are no tips to delete.", "Tips", MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    MessageBoxResult.OK);
                return;
            }
            if (
                MessageBox.Show("Are you sure you want to delete all tips from this tab?", "Confirm",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) == MessageBoxResult.Yes) {
                var reference = new LoaderReference(tipPanel);
                reference.StartLoading("Deleting tips...");
                var tips =
                    tipsList.Children.OfType<UIElement>()
                        .ToList()
                        .Select(c => (ModelControl) c)
                        .Select(c => c.GetSource<TabTip>())
                        .ToList();
                foreach (var tip in tips)
                    await MergeDatabase.DeleteAsync(tip);
                reference.StopLoading();
                InitTips();
            }
        }

        private void Toggle_Click(object sender, RoutedEventArgs e) {
            if (_visible) {
                //hide
                headerPanel.Visibility = Visibility.Collapsed;
                tipPanel.Visibility = Visibility.Collapsed;
                toggleButton.Content = "Show";
            } else {
                //show
                headerPanel.Visibility = Visibility.Visible;
                tipPanel.Visibility = Visibility.Visible;
                toggleButton.Content = "Hide";
            }
            _visible = !_visible;
        }
    }
}
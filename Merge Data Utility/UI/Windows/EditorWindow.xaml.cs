#region LICENSE

// Project Merge Data Utility:  EditorWindow.xaml.cs (in Solution Merge Data Utility)
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
using MergeApi.Client;
using MergeApi.Models.Core;
using MergeApi.Models.Elements;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages.Base;
using Merge_Data_Utility.UI.Pages.Editors;

#endregion

namespace Merge_Data_Utility.UI.Windows {
    /// <summary>
    ///     Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window {
        public enum ResultType {
            Published,
            Saved,
            Canceled
        }

        private Action<ResultType> _callback;

        private EditorPage _page;

        private bool _safeClosing;

        public EditorWindow() {
            InitializeComponent();
        }

        public EditorWindow(object source, bool draft, Action<ResultType> callback)
            : this(EditorPage.GetPage(source.GetType(), source, draft), callback) { }

        public EditorWindow(EditorPage page, Action<ResultType> callback) : this() {
            _page = page;
            _callback = callback;
            Closing += async (sender, args) => {
                if (_safeClosing) return;
                args.Cancel = true;
                if (MessageBox.Show(this,
                        "Are you sure you want to close this editor?  All unsaved changes will be lost.",
                        "Save Changes", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation,
                        MessageBoxResult.Cancel) != MessageBoxResult.OK) return;
                Result = ResultType.Canceled;
                if (_page is PageEditorPage) {
                    var cast = (PageEditorPage) _page;
                    var reference = GetLoaderReference();
                    reference.StartLoading("Cleaning up unused assets...");
                    var diff = cast.contentList.GetElements().OfType<ImageElement>()
                        .Where(e => !cast.HasSource || !cast.GetSource<MergePage>().Content.Contains(e)).ToList();
                    if (diff.Any()) {
                        foreach (var img in diff)
                            await MergeDatabase.DeleteStorageReferenceAsync(
                                img.Url.Replace("https://merge.devgregw.com/content/", ""));
                        args.Cancel = false;
                        _safeClosing = true;
                        Close();
                    }
                }
                args.Cancel = false;
            };
            _page.SetWindow(this);
            frame.Navigate(_page);
        }

        public ResultType Result { get; private set; }

        public bool MenuButtonsEnabled {
            get => menu.IsEnabled;
            set => menu.IsEnabled = value;
        }

        private void CloseSafe(ResultType r) {
            _safeClosing = true;
            Result = r;
            Close();
            if (r != ResultType.Canceled)
                _callback(Result);
        }

        public LoaderReference GetLoaderReference() {
            return new LoaderReference(content);
        }

        public void SetExtraTitle(string e) {
            Title = $"Editor - {e}";
        }

        private async void Publish_OnClick(object sender, RoutedEventArgs e) {
            MenuButtonsEnabled = false;
            if (await _page.Publish()) CloseSafe(ResultType.Published);
            MenuButtonsEnabled = true;
        }

        private async void Draft_OnClick(object sender, RoutedEventArgs e) {
            MenuButtonsEnabled = false;
            await _page.SaveAsDraft();
            CloseSafe(ResultType.Saved);
            MenuButtonsEnabled = true;
        }
    }
}
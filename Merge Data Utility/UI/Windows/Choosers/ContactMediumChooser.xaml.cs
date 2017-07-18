#region LICENSE

// Project Merge Data Utility:  ContactMediumChooser.xaml.cs (in Solution Merge Data Utility)
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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Client;
using MergeApi.Framework.Abstractions;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using MergeApi.Models.Elements;
using Merge_Data_Utility.Tools;

#endregion

namespace Merge_Data_Utility.UI.Windows.Choosers {
    /// <summary>
    ///     Interaction logic for ContactMediumChooser.xaml
    /// </summary>
    public partial class ContactMediumChooser : Window {
        private Predicate<MediumBase> _predicate;

        public ContactMediumChooser() {
            InitializeComponent();
        }

        public ContactMediumChooser(Predicate<MediumBase> predicate, bool allowCustom,
            string[] excludedIds) : this() {
            if (excludedIds == null)
                throw new ArgumentException("Never set excludedIds to null, use an empty array.", nameof(excludedIds));
            _predicate = predicate;
            custom.IsEnabled = allowCustom;
            Loaded += async (s, args) => {
                var lref = new LoaderReference(cc);
                lref.StartLoading("Looking for existing contact mediums...");
                var rawMediums = new Dictionary<MediumBase, string>();

                #region Finding mediums...

                var pages =
                    (await MergeDatabase.ListAsync<MergePage>()).Where(p => !excludedIds.Contains(p.Id));
                var groups =
                    (await MergeDatabase.ListAsync<MergeGroup>()).Where(g => !excludedIds.Contains(g.Id));
                //var leaders =
                //(await MergeDatabase.ListLeadersAsync()).Items.Where(l => !excludedIds.Contains(l.Id));
                foreach (var p in pages)
                foreach (var e in p.Content) {
                    if (!(e is ButtonElement)) continue;
                    if (((ButtonElement) e).Action is ShowContactInfoAction)
                        ((ShowContactInfoAction) ((ButtonElement) e).Action).ContactMediums2.ForEach(
                            m => rawMediums.Add(m, $"pages/{p.Id}"));
                    else if (((ButtonElement) e).Action is CallAction)
                        rawMediums.Add(((CallAction) ((ButtonElement) e).Action).ContactMedium1, $"pages/{p.Id}");
                    else if (((ButtonElement) e).Action is TextAction)
                        rawMediums.Add(((TextAction) ((ButtonElement) e).Action).ContactMedium1, $"pages/{p.Id}");
                    else if (((ButtonElement) e).Action is EmailAction)
                        rawMediums.Add(((EmailAction) ((ButtonElement) e).Action).ContactMedium1, $"pages/{p.Id}");
                }
                foreach (var g in groups)
                    g.ContactMediums.ForEach(m => rawMediums.Add(m, $"groups/{g.Id}"));
                //foreach (var l in leaders)
                //l.ContactMediums.ForEach(m => rawMediums.Add(m, $"leaders/{l.Id}"));

                #endregion

                var filtered = new Dictionary<MediumBase, IList<string>>();
                foreach (var pair in rawMediums) {
                    if (!predicate(pair.Key))
                        continue;
                    if (filtered.ContainsKey(pair.Key))
                        filtered[pair.Key].Add(pair.Value);
                    else
                        filtered.Add(pair.Key, new List<string> {
                            pair.Value
                        });
                }
                filtered.ForEach(pair => list.Items.Add(new ListViewItem {
                    Content =
                        $"{pair.Key.ToFriendlyString()} ({pair.Value.Sum(str => str + (pair.Value.Last() == str ? "" : ", "))})",
                    Tag = pair.Key
                }));
                lref.StopLoading();
            };
        }

        public MediumBase SelectedMedium { get; private set; }

        private void Custom(object sender, RoutedEventArgs e) {
            var window = new CustomContactMediumWindow(_predicate);
            window.ShowDialog();
            if (window.ContactMedium == null) return;
            SelectedMedium = window.ContactMedium;
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e) {
            SelectedMedium = null;
            Close();
        }

        private void Choose(object sender, RoutedEventArgs e) {
            if (list.SelectedIndex != -1) {
                SelectedMedium = (MediumBase) ((ListViewItem) list.SelectedItem).Tag;
                Close();
            } else if (
                MessageBox.Show(this,
                    "Select a contact medium, then click Choose.  Click OK to return, or Cancel to close this window.",
                    "Contact Medium Browser", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation,
                    MessageBoxResult.OK) == MessageBoxResult.Cancel) {
                Cancel(sender, e);
            }
        }
    }
}
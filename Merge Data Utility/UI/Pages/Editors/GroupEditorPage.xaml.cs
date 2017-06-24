#region LICENSE

// Project Merge Data Utility:  GroupEditorPage.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 03/20/2017 at 6:42 PM.
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
using System.Windows;
using System.Windows.Controls;
using MergeApi.Client;
using MergeApi.Models.Core;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages.Base;
using Merge_Data_Utility.UI.Windows;
using Merge_Data_Utility.UI.Windows.Choosers;

#endregion

namespace Merge_Data_Utility.UI.Pages.Editors {
    /// <summary>
    ///     Interaction logic for GroupEditorPage.xaml
    /// </summary>
    public partial class GroupEditorPage : EditorPage {
        public GroupEditorPage() {
            InitializeComponent();
        }

        public GroupEditorPage(MergeGroup g, bool isDraft) : this() {
            SetSource(g, isDraft);
        }

        protected override void Update() {
            idField.Regenerated += (s, e) => { UpdateTitle(); };
            leadersList.Prepare(() => {
                var input = TextInputWindow.GetInput("Add Leader", "Enter leader's name:");
                return input == null
                    ? null
                    : new ListViewItem {
                        Content = input
                    };
            }, i => true, i => {
                var input = TextInputWindow.GetInput("Edit Leader", "Enter leader's name:", i.Content.ToString());
                return input == null
                    ? i
                    : new ListViewItem {
                        Content = input
                    };
            });
            mediumsList.Prepare(() => {
                var window = new ContactMediumChooser(m => true, true,
                    new[] {HasSource ? GetSource<MergeGroup>().Id : ""});
                window.ShowDialog();
                return window.SelectedMedium == null
                    ? null
                    : new ListViewItem {
                        Content = window.SelectedMedium.ToFriendlyString(),
                        Tag = window.SelectedMedium
                    };
            }, i => {
                return
                    MessageBox.Show(Window, "Are you sure you want to delete this contact medium?", "Confirm",
                        MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) ==
                    MessageBoxResult.Yes;
            }, i => {
                var window = new ContactMediumChooser(m => true, true,
                    new[] {HasSource ? GetSource<MergeGroup>().Id : ""});
                window.ShowDialog();
                return window.SelectedMedium == null
                    ? null
                    : new ListViewItem {
                        Content = window.SelectedMedium.ToFriendlyString(),
                        Tag = window.SelectedMedium
                    };
            });
            if (HasSource) {
                var group = GetSource<MergeGroup>();
                idField.SetId(group.Id, false);
                nameBox.Text = group.Name;
                hostsBox.Text = group.Host;
                leadersList.SetItems(group.Leaders.Select(s => new ListViewItem {
                    Content = s
                }));
                addressBox.Text = group.Address;
                coverField.SetOriginalValue(group.CoverImage);
                mediumsList.SetContactMediums(group.ContactMediums);
            } else {
                idField.SetId("", true);
            }
        }

        protected override InputValidationResult ValidateInput() {
            var errors = new List<string> {
                string.IsNullOrWhiteSpace(nameBox.Text) ? "No name specified." : "",
                string.IsNullOrWhiteSpace(hostsBox.Text) ? "No host specified." : "",
                leadersList.Count == 0 ? "At least one leader must be added." : "",
                string.IsNullOrWhiteSpace(addressBox.Text) ? "No address specified." : "",
                string.IsNullOrWhiteSpace(coverField.Value) ? "No cover image specified." : "",
                mediumsList.Count == 0 ? "At least one contact medium must be added." : ""
            };
            errors.RemoveAll(string.IsNullOrWhiteSpace);
            return new InputValidationResult(errors);
        }

        protected override async Task<object> MakeObject() {
            var nsrc = new MergeGroup {
                Address = addressBox.Text,
                ContactMediums = mediumsList.GetContactMediums().ToList(),
                Host = hostsBox.Text,
                Id = idField.Id,
                Leaders = leadersList.GetItems(i => i.Content.ToString()).ToList(),
                Name = nameBox.Text
            };
            if (IsDraft) {
                nsrc.CoverImage = coverField.Value;
            } else {
                nsrc.Coordinates = (await GoogleMapsGeocoding.GetCoordinates(addressBox.Text)).Item2;
                nsrc.CoverImage = await coverField.PerformChangesAsync($"group_{idField.Id.ToLower()}");
            }
            return nsrc;
        }

        public override string GetIdentifier() {
            return $"groups/{idField.Id}{(IsDraft ? " (draft)" : "")}";
        }

        public override async Task<bool> Publish() {
            IsDraft = false;
            var res = ValidateInput();
            if (!res.IsInputValid) {
                res.Display(Window);
            } else {
                var reference = GetLoaderReference();
                reference.StartLoading("Processing...");
                var c = await GoogleMapsGeocoding.GetCoordinates(addressBox.Text);
                if (c == null) {
                    MessageBox.Show(Window,
                        "Crtitical Error:  The specified address is invalid.  No geographical coordinates could be found.",
                        "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return false;
                }
                var o = (MergeGroup) await MakeObject();
                DraftManager.AutoDelete(o);
                try {
                    await MergeDatabase.UpdateAsync(o);
                    return true;
                } catch (Exception ex) {
                    MessageBox.Show(Window,
                        $"Could not update groups/{o.Id} ({o.GetType().FullName}):\n{ex.Message}\n{ex.GetType().FullName}\n\n{ex.StackTrace}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return false;
        }

        public override async Task SaveAsDraft() {
            IsDraft = true;
            var o = await MakeObject();
            DraftManager.AddDraftedGroup((MergeGroup) o);
        }
    }
}
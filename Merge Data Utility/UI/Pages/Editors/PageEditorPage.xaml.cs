#region LICENSE

// Project Merge Data Utility:  PageEditorPage.xaml.cs (in Solution Merge Data Utility)
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Client;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using MergeApi.Tools;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages.Base;
using Merge_Data_Utility.UI.Windows.Choosers;

#endregion

namespace Merge_Data_Utility.UI.Pages.Editors {
    /// <summary>
    ///     Interaction logic for PageEditorPage.xaml
    /// </summary>
    public partial class PageEditorPage : EditorPage {
        public PageEditorPage() {
            InitializeComponent();
        }

        public PageEditorPage(MergePage source, bool draft) : this() {
            SetSource(source, draft);
        }

        protected override void Update() {
            baseCtrls.IdRegenerated += (s, e) => { UpdateTitle(); };
            baseCtrls.Prepare(HasSource ? GetSource<ModelBase>() : null, true);
            contentList.Prepare(() => {
                var window = new ContentElementWindow();
                window.ShowDialog();
                return window.Element == null
                    ? null
                    : new ListViewItem {
                        Content = window.Element.ToFriendlyString(),
                        Tag = window.Element
                    };
            }, i => {
                return
                    MessageBox.Show(Window, "Are you sure you want to delete this element?", "Confirm",
                        MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) ==
                    MessageBoxResult.Yes;
            }, i => {
                var window = new ContentElementWindow((ElementBase) i.Tag);
                window.ShowDialog();
                return window.Element == null
                    ? null
                    : new ListViewItem {
                        Content = window.Element.ToFriendlyString(),
                        Tag = window.Element
                    };
            });
            if (HasSource) {
                var p = GetSource<MergePage>();
                leadersOnly.IsChecked = p.LeadersOnly;
                contentList.SetElements(p.Content);
                hiddenBox.IsChecked = p.Hidden;
                buttonLabel.Text = p.ButtonLabel;
                buttonAction.DefaultAction = p.ButtonAction;
                buttonAction.Reset();
                importance.SelectedIndex = p.Importance == Importance.Normal
                    ? 0
                    : p.Importance == Importance.Medium
                        ? 1
                        : 2;
            } else {
                importance.SelectedIndex = 0;
            }
        }

        protected override InputValidationResult ValidateInput() {
            var errors = new List<string>();
            bool hasDescription = !string.IsNullOrWhiteSpace(baseCtrls.descBox.Text),
                hasElements = contentList.Count > 0;

            errors.AddRange(baseCtrls.GetValidationErrors(hasElements, true));
            errors.Add(string.IsNullOrWhiteSpace(buttonLabel.Text) && buttonAction.SelectedAction != null
                ? "If a button action is specified, a label must be specified."
                : "");
            errors.Add(!string.IsNullOrWhiteSpace(buttonLabel.Text) && buttonAction.SelectedAction == null
                ? "If a button label is specified, an action must be specified."
                : "");
            if (!hasDescription)
                errors.Add(!hasElements ? "Content elements are required if no description is provided." : "");
            errors.RemoveAll(string.IsNullOrWhiteSpace);
            return new InputValidationResult(errors);
        }

        protected override async Task<object> MakeObject() {
            var nsrc = new MergePage {
                LeadersOnly = leadersOnly.IsChecked.GetValueOrDefault(false),
                Content = contentList.GetElements().ToList(),
                Hidden = hiddenBox.IsChecked.GetValueOrDefault(false),
                Importance = ((ComboBoxItem) importance.SelectedItem).Content.ToString().ToEnum<Importance>(),
                ButtonLabel = string.IsNullOrWhiteSpace(buttonLabel.Text)
                    ? $"Launch {baseCtrls.Title}"
                    : buttonLabel.Text,
                ButtonAction = buttonAction.SelectedAction ?? OpenPageAction.FromPageId(baseCtrls.Id)
            };
            await baseCtrls.ApplyToAsync(nsrc, IsDraft);
            return nsrc;
        }

        public override string GetIdentifier() {
            return $"pages/{baseCtrls.Id}{(IsDraft ? " (draft)" : "")}";
        }

        public override async Task<bool> Publish() {
            IsDraft = false;
            var res = ValidateInput();
            if (!res.IsInputValid) {
                res.Display(Window);
            } else {
                var reference = GetLoaderReference();
                reference.StartLoading("Processing...");
                var o = (MergePage) await MakeObject();
                DraftManager.AutoDelete(o);
                try {
                    await MergeDatabase.UpdateAsync(o);
                    return true;
                } catch (Exception ex) {
                    MessageBox.Show(Window,
                        $"Could not update pages/{o.Id} ({o.GetType().FullName}):\n{ex.Message}\n{ex.GetType().FullName}\n\n{ex.StackTrace}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return false;
        }

        public override async Task SaveAsDraft() {
            IsDraft = true;
            var o = await MakeObject();
            DraftManager.AddDraftedPage((MergePage) o);
        }
    }
}
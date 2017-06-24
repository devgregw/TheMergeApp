#region LICENSE

// Project Merge Data Utility:  TipEditorPage.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 03/28/2017 at 7:31 PM.
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
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Core.Tab;
using MergeApi.Tools;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages.Base;

#endregion

namespace Merge_Data_Utility.UI.Pages.Editors {
    /// <summary>
    ///     Interaction logic for TipEditorPage.xaml
    /// </summary>
    public partial class TipEditorPage : EditorPage {
        public TipEditorPage() {
            InitializeComponent();
        }

        public TipEditorPage(TabTip source, bool draft) : this() {
            SetSource(source, false);
        }

        protected override void Update() {
            idField.Regenerated += (s, e) => { UpdateTitle(); };
            tab.SelectionChanged += (s, e) => {
                if (tab.SelectedIndex > -1)
                    UpdateTitle();
            };
            if (!HasSource) {
                idField.SetId(MergeDatabase.NewId(), true);
                return;
            }
            var tip = GetSource<TabTip>();
            idField.SetId(tip.Id, false);
            tab.SelectedItem = tab.Items.Cast<ComboBoxItem>().First(i => i.Tag.ToString() == tip.Tab.ToString());
            message.Text = tip.Message;
            action.DefaultAction = tip.Action;
            action.Reset();
            persistent.IsChecked = tip.Persistent;
            gradesField.Value = tip.GradeLevels;
            gendersField.Value = tip.Genders;
        }

        protected override InputValidationResult ValidateInput() {
            var errors = new List<string>();
            errors.Add(tab.SelectedIndex == -1 ? "No tab selected." : "");
            errors.Add(string.IsNullOrWhiteSpace(message.Text) ? "No message specified." : "");
            errors.Add(gradesField.Value.Count == 0 ? "At least one grade level must be selected." : "");
            errors.Add(gendersField.Value.Count == 0 ? "At least one gender must be specified." : "");
            errors.RemoveAll(string.IsNullOrWhiteSpace);
            return new InputValidationResult(errors);
        }

        protected override Task<object> MakeObject() {
            return Task.FromResult((object) new TabTip {
                Id = idField.Id,
                Tab = ((ComboBoxItem) tab.SelectedItem).Tag.ToString().ToEnum<Tab>(true),
                Persistent = persistent.IsChecked.GetValueOrDefault(false),
                Action = action.SelectedAction,
                Message = message.Text,
                GradeLevels = gradesField.Value,
                Genders = gendersField.Value
            });
        }

        public override string GetIdentifier() {
            return
                $"tips/{(tab.SelectedIndex == -1 ? "<tab>" : ((ComboBoxItem) tab.SelectedItem).Tag.ToString().ToLower())}/{idField.Id}";
        }

        public override async Task<bool> Publish() {
            var res = ValidateInput();
            if (!res.IsInputValid) {
                res.Display(Window);
            } else {
                var reference = GetLoaderReference();
                reference.StartLoading("Processing...");
                var o = (TabTip) await MakeObject();
                try {
                    await MergeDatabase.UpdateAsync(o);
                    return true;
                } catch (Exception ex) {
                    MessageBox.Show(Window,
                        $"Could not update tips/{o.Tab.ToString().ToLower()}/{o.Id} ({o.GetType().FullName}):\n{ex.Message}\n{ex.GetType().FullName}");
                }
            }
            return false;
        }

        public override async Task SaveAsDraft() {
            IsDraft = true;
            var o = await MakeObject();
            DraftManager.AddDraftedTip((TabTip) o);
        }
    }
}
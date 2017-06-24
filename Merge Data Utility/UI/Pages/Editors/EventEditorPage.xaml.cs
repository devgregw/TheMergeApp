#region LICENSE

// Project Merge Data Utility:  EventEditorPage.xaml.cs (in Solution Merge Data Utility)
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
using MergeApi.Framework.Abstractions;
using MergeApi.Models.Core;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages.Base;

#endregion

namespace Merge_Data_Utility.UI.Pages.Editors {
    /// <summary>
    ///     Interaction logic for EventEditorPage.xaml
    /// </summary>
    public partial class EventEditorPage : EditorPage {
        public EventEditorPage() {
            InitializeComponent();
        }

        public EventEditorPage(MergeEvent source, bool isDraft) : this() {
            SetSource(source, isDraft);
        }

        protected override InputValidationResult ValidateInput() {
            var errors = new List<string>();
            errors.AddRange(baseCtrls.GetValidationErrors(false));
            errors.Add(string.IsNullOrWhiteSpace(locationBox.Text) ? "The location is invalid." : "");
            errors.Add(!startDateBox.Value.HasValue ? "No start date selected." : "");
            if (endDateBox.Value.HasValue)
                errors.Add(endDateBox.Value.Value <=
                           (startDateBox.Value.HasValue ? startDateBox.Value : DateTime.MaxValue)
                    ? "The end date must come after the start date."
                    : "");
            else
                errors.Add("No end date selected.");
            errors.Add(!priceBox.Value.HasValue ? "The price is invalid." : "");
            if (!string.IsNullOrWhiteSpace(urlBox.Text))
                errors.Add(regCloseBox.Value.HasValue ? "" : "No registration closing date specified.");
            errors.RemoveAll(string.IsNullOrWhiteSpace);
            return new InputValidationResult(errors);
        }

        protected override async Task<object> MakeObject() {
            var nsrc = new MergeEvent {
                Location = locationBox.Text,
                Address = addressBox.Text,
                StartDate = startDateBox.Value,
                EndDate = endDateBox.Value,
                Price = Convert.ToDouble(priceBox.Value.GetValueOrDefault(0)),
                RegistrationUrl = urlBox.Text,
                RegistrationClosingDate = string.IsNullOrWhiteSpace(urlBox.Text) ? null : regCloseBox.Value
            };
            await baseCtrls.ApplyToAsync(nsrc, IsDraft);
            return nsrc;
        }

        public override string GetIdentifier() {
            return $"events/{baseCtrls.Id}{(IsDraft ? " (draft)" : "")}";
        }

        public override async Task<bool> Publish() {
            IsDraft = false;
            var res = ValidateInput();
            if (!res.IsInputValid) {
                res.Display(Window);
            } else {
                var reference = GetLoaderReference();
                reference.StartLoading("Processing...");
                var o = (MergeEvent) await MakeObject();
                DraftManager.AutoDelete(o);
                try {
                    await MergeDatabase.UpdateAsync(o);
                    return true;
                } catch (Exception ex) {
                    MessageBox.Show(Window,
                        $"Could not update events/{o.Id} ({o.GetType().FullName}):\n{ex.Message}\n{ex.GetType().FullName}\n\n{ex.StackTrace}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return false;
        }

        public override async Task SaveAsDraft() {
            IsDraft = true;
            var o = await MakeObject();
            DraftManager.AddDraftedEvent((MergeEvent) o);
        }

        protected override void Update() {
            baseCtrls.IdRegenerated += (s, e) => { UpdateTitle(); };
            baseCtrls.Prepare(HasSource ? GetSource<ModelBase>() : null);
            var presets = addressPresets.Children.Cast<Button>();
            foreach (var b in presets) b.Click += (s, e) => addressBox.Text = b.Tag.ToString();
            if (!HasSource) return;
            var src = GetSource<MergeEvent>();
            locationBox.Text = src.Location;
            addressBox.Text = src.Address;
            startDateBox.Value = src.StartDate;
            endDateBox.Value = src.EndDate;
            priceBox.Value = Convert.ToDecimal(src.Price);
            urlBox.Text = src.RegistrationUrl;
            regCloseBox.Value = src.RegistrationClosingDate;
        }
    }
}
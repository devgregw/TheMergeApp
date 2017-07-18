#region LICENSE

// Project Merge Data Utility:  CallActionPage.xaml.cs (in Solution Merge Data Utility)
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

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using MergeApi.Framework.Abstractions;
using MergeApi.Models.Actions;
using MergeApi.Models.Mediums;
using Merge_Data_Utility.UI.Pages.Base;
using Merge_Data_Utility.UI.Windows.Choosers;

#endregion

namespace Merge_Data_Utility.UI.Pages.ActionConfiguration {
    /// <summary>
    ///     Interaction logic for CallActionPage.xaml
    /// </summary>
    public partial class CallActionPage : ActionConfigurationPage {
        public CallActionPage() : this(null) { }

        public CallActionPage(CallAction source) {
            InitializeComponent();
            Initialize(source);
        }

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        public override void Update() {
            if (!HasCurrentValue) return;
            var cv = GetCurrentAction<CallAction>();
            switch (cv.ParamGroup) {
                case "1":
                    r1.IsChecked = true;
                    mediumBox.Text = cv.ContactMedium1.ToFriendlyString();
                    mediumBox.Tag = cv.ContactMedium1;
                    break;
                case "2":
                    r2.IsChecked = true;
                    numberBox.Text = cv.PhoneNumber2;
                    break;
            }
        }

        public override ActionBase GetAction() {
            if (r1.IsChecked.GetValueOrDefault(false)) {
                if (!string.IsNullOrWhiteSpace(mediumBox.Text))
                    return CallAction.FromContactMedium((PhoneNumberMedium) mediumBox.Tag);
                DisplayErrorMessage(new[] {"No contact medium selected."});
                return null;
            }
            if (r2.IsChecked.GetValueOrDefault(false)) {
                if (PhoneNumberMedium.IsValidPhoneNumber(numberBox.Text))
                    return CallAction.FromPhoneNumber(numberBox.Text);
                DisplayErrorMessage(new[] {"The specified phone number is invalid."});
                return null;
            }
            DisplayErrorMessage(new[] {"No parameter group selected."});
            return null;
        }

        private void ChooseMedium(object sender, RoutedEventArgs args) {
            var window = new ContactMediumChooser(m => m is PhoneNumberMedium, true, new[] {""});
            window.ShowDialog();
            if (window.SelectedMedium != null) {
                mediumBox.Text = window.SelectedMedium.ToFriendlyString();
                mediumBox.Tag = window.SelectedMedium;
            }
        }
    }
}
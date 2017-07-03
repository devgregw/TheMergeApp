#region LICENSE

// Project Merge Data Utility:  CustomContactMediumPage.xaml.cs (in Solution Merge Data Utility)
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Mediums;
using MergeApi.Tools;
using Merge_Data_Utility.UI.Windows.Choosers;

#endregion

namespace Merge_Data_Utility.UI.Pages {
    /// <summary>
    ///     Interaction logic for CustomContactMediumPage.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
    public partial class CustomContactMediumPage : Page {
        private int _type;

        private CustomContactMediumWindow _window;

        public CustomContactMediumPage() {
            InitializeComponent();
        }

        public CustomContactMediumPage(CustomContactMediumWindow window, int type) : this() {
            /* TYPES
             * 0: Phone numbers (with or without SMS)
             * 1: Phone numbers with SMS
             * 2: Phone numbers without SMS
             * 3: Email addresses
             */
            _window = window;
            _type = type;
            switch (type) {
                case 0:
                    phone.Visibility = Visibility.Visible;
                    email.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    phone.Visibility = Visibility.Visible;
                    email.Visibility = Visibility.Collapsed;
                    sms.IsChecked = true;
                    sms.IsEnabled = false;
                    break;
                case 2:
                    phone.Visibility = Visibility.Visible;
                    email.Visibility = Visibility.Collapsed;
                    sms.IsChecked = false;
                    sms.IsEnabled = false;
                    break;
                case 3:
                    phone.Visibility = Visibility.Collapsed;
                    email.Visibility = Visibility.Visible;
                    break;
            }
            Loaded += (s, e) => back.IsEnabled = NavigationService.CanGoBack;
        }

        private void OtherType(object sender, RoutedEventArgs e) {
            NavigationService.GoBack();
        }

        private bool Validate() {
            var errors = new List<string>();
            errors.Add(string.IsNullOrWhiteSpace(nameBox.Text) ? "No name specified." : "");
            switch (_type) {
                case 0:
                case 1:
                case 2:
                    errors.Add(PhoneNumberMedium.IsValidPhoneNumber(phoneNumberBox.Text)
                        ? ""
                        : "The specified phone number is invalid.");
                    errors.Add(phoneType.SelectedIndex == -1 ? "No type specified." : "");
                    break;
                case 3:
                    errors.Add(EmailAddressMedium.IsValidEmailAddress(emailBox.Text)
                        ? ""
                        : "The specified email address is invalid.");
                    errors.Add(emailType.SelectedIndex == -1 ? "No type specified." : "");
                    break;
            }
            errors.RemoveAll(string.IsNullOrWhiteSpace);
            if (!errors.Any()) return true;
            var text = errors.Aggregate("", (current, s) => current + $"{s}\n");
            MessageBox.Show(_window, "Please resolve the following errors:\n\n" + text, "Input Invalid",
                MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            return false;
        }

        private void Done(object sender, RoutedEventArgs e) {
            if (!Validate())
                return;
            switch (_type) {
                case 0:
                case 1:
                case 2:
                    _window.ContactMedium = PhoneNumberMedium.Create(nameBox.Text, phoneNumberBox.Text,
                        sms.IsChecked.GetValueOrDefault(false),
                        phoneType.SelectedItem.Manipulate(
                            o => ((ComboBoxItem)o).Content.ToString().ToEnum<PhoneNumberKind>()));
                    break;
                case 3:
                    _window.ContactMedium = EmailAddressMedium.Create(nameBox.Text, emailBox.Text,
                        emailType.SelectedItem.Manipulate(
                            o => ((ComboBoxItem)o).Content.ToString().ToEnum<EmailAddressKind>()));
                    break;
            }
            _window.Close();
        }
    }
}
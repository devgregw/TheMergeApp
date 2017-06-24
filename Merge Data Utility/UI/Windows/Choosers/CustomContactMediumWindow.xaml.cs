#region LICENSE

// Project Merge Data Utility:  CustomContactMediumWindow.xaml.cs (in Solution Merge Data Utility)
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
using System.Windows.Navigation;
using MergeApi.Framework.Abstractions;
using MergeApi.Models.Mediums;
using Merge_Data_Utility.UI.Pages;

#endregion

namespace Merge_Data_Utility.UI.Windows.Choosers {
    /// <summary>
    ///     Interaction logic for CustomContactMediumWindow.xaml
    /// </summary>
    public partial class CustomContactMediumWindow : NavigationWindow {
        public CustomContactMediumWindow() : this(m => true) { }

        public CustomContactMediumWindow(Predicate<MediumBase> predicate) {
            InitializeComponent();
            if (predicate(SmsTestMedium) && predicate(NonSmsTestMedium) && predicate(EmailTestMedium))
                // All types allowed
                Navigate(new ContactMediumTypePage());
            else if (predicate(SmsTestMedium) && predicate(NonSmsTestMedium)) // Only phone numbers
                Navigate(new CustomContactMediumPage(this, 0));
            else if (predicate(SmsTestMedium) && !predicate(NonSmsTestMedium)) // Only SMS-capable phones
                Navigate(new CustomContactMediumPage(this, 1));
            else if (predicate(NonSmsTestMedium) && !predicate(SmsTestMedium)) // Only non-SMS-capable phones
                Navigate(new CustomContactMediumPage(this, 2));
            else if (predicate(EmailTestMedium)) // Only email addresses
                Navigate(new CustomContactMediumPage(this, 3));
        }

        private PhoneNumberMedium SmsTestMedium => new PhoneNumberMedium {
            PhoneNumber = "8178230245",
            CanReceiveSMS = true
        };

        private PhoneNumberMedium NonSmsTestMedium => new PhoneNumberMedium {
            PhoneNumber = "8178230245",
            CanReceiveSMS = false
        };

        private EmailAddressMedium EmailTestMedium => new EmailAddressMedium {
            Address = "devgregw@outlook.com"
        };

        public MediumBase ContactMedium { get; set; }
    }
}
#region LICENSE

// Project Merge Data Utility:  AuthenticationPage.xaml.cs (in Solution Merge Data Utility)
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

using System.Windows;
using System.Windows.Controls;
using MergeApi;
using Merge_Data_Utility.Properties;
using Merge_Data_Utility.Tools;

#endregion

namespace Merge_Data_Utility.UI.Pages {
    /// <summary>
    ///     Interaction logic for AuthenticationPage.xaml
    /// </summary>
    public partial class AuthenticationPage : Page {
        public AuthenticationPage() {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(Settings.Default.Username) &&
                !string.IsNullOrWhiteSpace(Settings.Default.Password)) {
                usr.Text = Settings.Default.Username;
                pwd.Password = Settings.Default.Password;
                SignIn(null, null);
            }
        }

        private async void SignIn(object sender, RoutedEventArgs e) {
            var reference = new LoaderReference(content);
            reference.StartLoading("Signing in...");
            try {
                StaticAuth.AuthLink = await MergeDatabase.AuthenticateAsync(usr.Text, pwd.Password);
                var settings = Settings.Default;
                settings.Username = usr.Text;
                settings.Password = pwd.Password;
                settings.Save();
                NavigationService.Navigate(new MainPage(0));
                return;
            } catch {
                MessageBox.Show("The email address or password is incorrect.", "Sign-in Error", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK);
            }
            reference.StopLoading();
            usr.Text = "";
            pwd.Password = "";
        }
    }
}
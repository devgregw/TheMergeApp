#region LICENSE

// Project Merge Data Utility:  UpdatePromptPage.xaml.cs (in Solution Merge Data Utility)
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

using System.Diagnostics;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace Merge_Data_Utility.UI.Pages {
    /// <summary>
    ///     Interaction logic for UpdatePromptPage.xaml
    /// </summary>
    public partial class UpdatePromptPage : Page {
        public UpdatePromptPage() {
            InitializeComponent();
        }

        public UpdatePromptPage(UpdateCheckPage.UtilityVersion info, int tab = 0, bool skip = false) : this() {
            update.Click += (s, e) => {
                header.Text = "Please wait.";
                while (main.Children.Count > 1)
                    main.Children.RemoveAt(1);
                ZipFile.ExtractToDirectory("updater.zip", "UpdaterTemp");
                Process.Start("UpdaterTemp\\mdu-updater.exe");
                Application.Current.MainWindow.Close();
            };
            note.Text = $"Version {info.Version}:  {info.Note}";
            if (info.IsUpdateRequired) {
                var block = new TextBlock {
                    Text = "This update is required.",
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetColumn(block, 2);
                grid.Children.Add(block);
            } else {
                var button = new Button {
                    Content = "Later"
                };
                button.Click +=
                    (s, e) => NavigationService.Navigate(skip ? (Page) new MainPage(tab) : new AuthenticationPage());
                Grid.SetColumn(button, 2);
                grid.Children.Add(button);
            }
        }
    }
}
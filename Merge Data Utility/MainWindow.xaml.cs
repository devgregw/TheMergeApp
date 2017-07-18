#region LICENSE

// Project Merge Data Utility:  MainWindow.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 06/23/2017 at 10:43 AM.
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
using System.IO;
using System.Windows.Controls;
using System.Windows.Navigation;
using MergeApi.Client;
using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Interfaces.Receivers;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages;

#endregion

namespace Merge_Data_Utility {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NavigationWindow {
        public MainWindow() {
            InitializeComponent();
            if (!Directory.Exists("Cache"))
                Directory.CreateDirectory("Cache");
            if (Directory.Exists("UpdaterTemp"))
                Directory.Delete("UpdaterTemp", true);
            Closing += (s, e) => {
                if (Directory.Exists("Cache"))
                    Directory.Delete("Cache", true);
            };
            MergeDatabase.Initialize(async () => {
                await StaticAuth.AuthLink.GetFreshAuthAsync();
                return StaticAuth.AuthLink.FirebaseToken;
            }, null, null, new Logger());
            Navigated += (s, e) => Title = ((Page) e.Content).Title + " - Merge Data Utility";
            Navigate(new UpdateCheckPage());
        }

        public class Logger : ILogReceiver {
            public bool Initialize() {
                return true;
            }

            public void Log(LogLevel level, string sender, string message) {
                Console.WriteLine($"[{DateTime.Now}] [{level}] {message} ({sender})");
            }

            public void Log(LogLevel level, string sender, Exception e) {
                Console.WriteLine(
                    $"[{DateTime.Now}] [{level}] [{e.GetType().FullName}] {e.Message} \n {e.StackTrace} ({sender})");
            }
        }
    }
}
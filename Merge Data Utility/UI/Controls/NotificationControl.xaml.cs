#region LICENSE

// Project Merge Data Utility:  NotificationControl.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 05/13/2017 at 10:29 AM.
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Merge_Data_Utility.UI.Windows;
using OneSignal.CSharp.SDK;
using OneSignal.CSharp.SDK.Resources.Notifications;

#endregion

namespace Merge_Data_Utility.UI.Controls {
    /// <summary>
    ///     Interaction logic for NotificationControl.xaml
    /// </summary>
    public partial class NotificationControl : UserControl {
        public NotificationControl() {
            InitializeComponent();
        }

        public NotificationControl(NotificationManagerWindow.NotificationInfo n, bool active) : this() {
            header.Text = n.Headings["en"];
            message.Text = n.Contents["en"];
            var notifs = n.Remaining + n.Successful + n.Failed;
            total.Text = $"Total: {notifs} (100%)";
            remaining.Text = $"Remaining: {n.Remaining} ({GetIntAverage(n.Remaining, notifs)}%)";
            success.Text = $"Successful: {n.Successful} ({GetIntAverage(n.Successful, notifs)}%)";
            failed.Text = $"Failed: {n.Failed} ({GetIntAverage(n.Failed, notifs)}%)";
            clicked.Text = $"Clicked: {n.Converted} ({GetIntAverage(n.Converted, notifs)}%)";
            id.Text = n.Id;
            if (active) {
                cancel.Click += async (s, e) => await Task.Run(() => {
                    var client = new OneSignalClient("MGNlYmMzYjAtZmMxMS00MjQxLTgxOGUtOGI4ZWU0YTQ0YzUz");
                    if (MessageBox.Show("Are you sure you want to cancel this notification?", "Confirm",
                            MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) ==
                        MessageBoxResult.Yes)
                        client.Notifications.Cancel(new NotificationCancelOptions {
                            AppId = "b52deecc-3f20-4904-a3f0-fd8e9aabb2b3",
                            Id = n.Id
                        });
                });
            } else {
                cancel.Visibility = Visibility.Collapsed;
                canceled.Visibility = n.Canceled ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private int GetIntAverage(int top, int bottom) {
            return Convert.ToInt32(Convert.ToDouble(top) / Convert.ToDouble(bottom) * 100);
        }
    }
}
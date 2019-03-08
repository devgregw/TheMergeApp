#region LICENSE

// Project Merge Data Utility:  FileBrowserWindow.xaml.cs (in Solution Merge Data Utility)
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MergeApi;
using MergeApi.Tools;
using Merge_Data_Utility.Tools;
using Microsoft.Win32;

#endregion

namespace Merge_Data_Utility.UI.Windows.Choosers {
    /// <summary>
    ///     Interaction logic for UploadedFileChooser.xaml
    /// </summary>
    public partial class FileBrowserWindow : Window {
        public FileBrowserWindow() {
            InitializeComponent();
            Loaded += async (s, e) => await LoadFiles();
        }

        private Button MakeButton(string content, RoutedEventHandler handler) {
            var b = new Button {
                Content = content,
                Margin = new Thickness(2.5)
            };
            b.Click += handler;
            return b;
        }

        private ListViewItem MakeItem(StorageReference reference) {
            var layout = new StackPanel {
                Children = {
                    new TextBlock {
                        Text = reference.Name
                    },
                    new StackPanel {
                        Orientation = Orientation.Horizontal,
                        Children = {
                            MakeButton("Copy URL", async (s, e) => {
                                SystemSounds.Beep.Play();
                                Clipboard.SetText(reference.Url, TextDataFormat.Text);
                                ((Button) s).Content = "Copied!";
                                await Task.Run(() => Thread.Sleep(2000));
                                ((Button) s).Content = "Copy URL";
                            }),
                            MakeButton("Download", (s, e) => {
                                var ext = ImageConverter.GetExtension(reference.Url);
                                var dialog = new SaveFileDialog {
                                    Filter = $"{ext.ToUpper()} Files|*.{ext}",
                                    Title = "Download File",
                                    OverwritePrompt = true,
                                    CreatePrompt = true
                                };
                                if (dialog.ShowDialog().GetValueOrDefault(false))
                                    new AsyncLoadingWindow("Downloading File", "Please wait...",
                                        new Func<object, Task<object>>(async o => {
                                            using (var client = new WebClient()) {
                                                File.WriteAllBytes(dialog.FileName,
                                                    await client.DownloadDataTaskAsync(reference.Url));
                                            }
                                            MessageBox.Show("The file was saved successfully.", "Success",
                                                MessageBoxButton.OK, MessageBoxImage.Information);
                                            return null;
                                        }), null).ShowDialog();
                            }),
                            MakeButton("Open", (s, e) => Process.Start(reference.Url)),
                            MakeButton("Delete", async (s, e) => {
                                if (MessageBox.Show(this, "Are you sure you want to delete this file?", "Confirm",
                                        MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) !=
                                    MessageBoxResult.Yes) return;
                                list.Items.Remove(list.Items.OfType<ListViewItem>()
                                    .First(i => i.Tag.ToString() == reference.Url));
                                await MergeDatabase.DeleteStorageReferenceAsync(reference.Name, "uploads");
                            })
                        }
                    }
                }
            };
            return new ListViewItem {
                Tag = reference.Url,
                Content = layout
            };
        }

        private async Task LoadFiles() {
            var reference = new LoaderReference(cc);
            reference.StartLoading("Loading files...");
            var files = await MergeDatabase.ListStorageReferencesAsync("uploads");
            list.Items.Clear();
            files.ForEach(r => list.Items.Add(MakeItem(r)));
            if (list.Items.Count == 0)
                list.Items.Add(new ListViewItem {
                    Content = "No files found"
                });
            reference.StopLoading();
        }

        private async void Upload_Click(object sender, RoutedEventArgs e) {
            new FileInputWindow().ShowDialog();
            await LoadFiles();
        }
    }
}
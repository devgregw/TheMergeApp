#region LICENSE

// Project Merge Data Utility:  LoaderReference.cs (in Solution Merge Data Utility)
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

using System.Windows;
using System.Windows.Controls;
using local = Merge_Data_Utility.UI.Controls.Other;

#endregion

namespace Merge_Data_Utility.Tools {
    public sealed class LoaderReference {
        public LoaderReference(ContentControl host) {
            Host = host;
            Content = host.Content;
        }

        public ContentControl Host { get; }

        public object Content { get; }

        public T GetContent<T>() {
            return (T) Content;
        }

        public void StartLoading(string message = "Loading...") {
            Host.Content = new Grid {
                Children = {
                    new StackPanel {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Children = {
                            new local.LoadingImage {
                                Height = 50
                            },
                            new TextBlock {
                                Margin = new Thickness(0, 5, 0, 0),
                                Text = message
                            }
                        }
                    }
                }
            };
        }

        public void SetMessage(string m = "Loading...") {
            StartLoading(m);
        }

        public void StopLoading() {
            StopLoading(Content);
        }

        public void StopLoading(object content) {
            Host.Content = content;
        }
    }
}
#region LICENSE

// Project Merge Data Utility:  ErrorControl.xaml.cs (in Solution Merge Data Utility)
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

#endregion

namespace Merge_Data_Utility.UI.Controls {
    /// <summary>
    ///     Interaction logic for ErrorControl.xaml
    /// </summary>
    /*public partial class ErrorControl : UserControl {
        private ClientError _err;

        private string _prefix, _id;

        public ErrorControl() {
            InitializeComponent();
        }

        public ErrorControl(ClientError err) : this() {
            _err = err;
            // ReSharper disable once ConstantNullCoalescingCondition
            _prefix = err.ObjectType == null ? "unknown" : err.ObjectType.Name.ToLower().Replace("merge", "") + "s";
            _id = string.IsNullOrWhiteSpace(err.ObjectId) ? "unknown" : err.ObjectId;
            title.Text = $"An error occured while processing {_prefix}/{_id}.";
            message.Text = err.Cause.Message + $"({err.Cause.GetType().FullName})";
            id.Text = $"{_prefix}/{_id}";
            resp.IsEnabled = err.Cause is ApiResponseException;
        }

        private void ResponseDetails(object sender, RoutedEventArgs e) {
            var api = ((ApiResponseException) _err.Cause).Response;
            MessageBox.Show(
                api.Summary, "Response Details", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);
        }

        private void ExceptionDetails(object sender, RoutedEventArgs e) {
            MessageBox.Show($"{_err.Cause.Message} ({_err.Cause.GetType().FullName})\n\n{_err.Cause.StackTrace}",
                "Exception Details", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);
        }
    }*/
}
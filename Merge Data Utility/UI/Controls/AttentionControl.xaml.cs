#region LICENSE

// Project Merge Data Utility:  AttentionControl.xaml.cs (in Solution Merge Data Utility)
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Framework.Abstractions;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Windows;
using api = MergeApi.Tools;

#endregion

namespace Merge_Data_Utility.UI.Controls {
    /// <summary>
    ///     Interaction logic for AttentionControl.xaml
    /// </summary>
    public partial class AttentionControl : UserControl {
        private Action _callback;

        private api.ValidationResult _result;

        public AttentionControl() {
            InitializeComponent();
        }

        public AttentionControl(api.ValidationResult r, Action callback) : this() {
            _result = r;
            _callback = callback;
            var m = _result.GetSubject<ModelBase>();
            header.Text = m.Title;
            subheader.Text = DataValidation.GetResultDescription(r.ResultType);
            id.Text = $"{m.GetType().Name.Replace("Merge", "").ToLower()}s/{m.Id}";
        }

        private void Repair_Requested(object sender, RoutedEventArgs e) {
            var window = new AsyncLoadingWindow("Data Validation", "Attempting repairs...",
                new Func<api.ValidationResult, Task<FixResult>>(async r => await DataValidation.ApplyFix(r)),
                _result);
            window.ShowDialog();
            var result = (FixResult) window.Result;
            switch (result) {
                case FixResult.Success:
                    MessageBox.Show("The operation completed successfully.", "Data Validation", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    break;
                case FixResult.Error:
                    MessageBox.Show("Data Validation was unable to repair the item.", "Data Validation",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case FixResult.Cancel:
                    MessageBox.Show("The repair was canceled.  No changes were made.", "Data Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    break;
            }
            if (result != FixResult.Cancel)
                _callback();
        }
    }
}
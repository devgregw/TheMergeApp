#region LICENSE

// Project Merge Data Utility:  ActionConfigurationPage.cs (in Solution Merge Data Utility)
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Framework.Abstractions;
using MergeApi.Models.Actions;
using Merge_Data_Utility.UI.Pages.ActionConfiguration;

#endregion

namespace Merge_Data_Utility.UI.Pages.Base {
    public abstract class ActionConfigurationPage : Page {
        public static Dictionary<Type, Type> Mappings = new Dictionary<Type, Type> {
            {typeof(AddToCalendarAction), typeof(AddToCalendarActionPage)},
            {typeof(CallAction), typeof(CallActionPage)},
            {typeof(EmailAction), typeof(EmailActionPage)},
            {typeof(GetDirectionsAction), typeof(GetDirectionsActionPage)},
            {typeof(LaunchUriAction), typeof(LaunchUriActionPage)},
            {typeof(OpenEventDetailsAction), typeof(OpenEventDetailsActionPage)},
            {typeof(OpenGroupMapPageAction), typeof(OpenGroupMapPageActionPage)},
            {typeof(OpenGroupDetailsAction), typeof(OpenGroupDetailsActionPage)},
            {typeof(OpenPageAction), typeof(OpenPageActionPage)},
            {typeof(ShowContactInfoAction), typeof(ShowContactInfoActionPage)},
            {typeof(TextAction), typeof(TextActionPage)}
        };

        private ActionBase _current;

        public bool HasCurrentValue => _current != null;

        public static ActionConfigurationPage GetPage(Type actionType, ActionBase value) {
            return
                (ActionConfigurationPage)
                Mappings[actionType].GetConstructors()
                    .First(c => c.GetParameters().Length == 1)
                    .Invoke(new object[] {value});
        }

        protected T GetCurrentAction<T>() where T : ActionBase {
            return (T) _current;
        }

        protected void Initialize(ActionBase value) {
            _current = value;
            Update();
        }

        protected void DisplayErrorMessage(IEnumerable<string> errors) {
            var l = errors.ToList();
            l.RemoveAll(string.IsNullOrWhiteSpace);
            var list = l.Aggregate("", (current, e) => current + $"{e}\n");
            MessageBox.Show($"Please resolve the following errors:\n\n{list}", "Input Validation",
                MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        public abstract void Update();

        public abstract ActionBase GetAction();
    }
}
#region LICENSE

// Project Merge Data Utility:  EditorPage.cs (in Solution Merge Data Utility)
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Models.Core;
using MergeApi.Models.Core.Attendance;
using MergeApi.Models.Core.Tab;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages.Editors;
using Merge_Data_Utility.UI.Windows;

#endregion

namespace Merge_Data_Utility.UI.Pages.Base {
    public abstract class EditorPage : Page {
        public static Dictionary<Type, Type> Mappings = new Dictionary<Type, Type> {
            {typeof(MergeEvent), typeof(EventEditorPage)},
            {typeof(MergeGroup), typeof(GroupEditorPage)},
            {typeof(MergePage), typeof(PageEditorPage)},
            {typeof(AttendanceGroup), typeof(AttendanceGroupEditorPage)},
            {typeof(AttendanceRecord), typeof(AttendanceRecordEditorPage)},
            {typeof(TabTip), typeof(TipEditorPage)}
        };

        private bool _drafting = true;

        private object _src;

        protected EditorWindow Window { get; set; }

        public bool HasSource => _src != null;

        public bool IsDraft { get; protected set; }

        protected void DisableDrafting() {
            _drafting = false;
        }

        public static EditorPage GetPage(Type objectType, object source, bool draft) {
            return
                (EditorPage)
                Mappings[objectType].GetConstructors()
                    .First(c => c.GetParameters().Count() == 2)
                    .Invoke(new[] {source, draft});
        }

        public LoaderReference GetLoaderReference() {
            return Window.GetLoaderReference();
        }

        public void SetWindow(EditorWindow window) {
            Window = window;
            Window.draftButton.IsEnabled = _drafting;
            UpdateTitle();
        }

        public void UpdateTitle() {
            Window?.SetExtraTitle(GetIdentifier());
        }

        public T GetSource<T>() {
            return (T) _src;
        }

        public void SetSource(object src, bool draft) {
            _src = src;
            IsDraft = draft;
            Update();
        }

        protected abstract void Update();

        protected abstract InputValidationResult ValidateInput();

        protected abstract Task<object> MakeObject();

        public abstract string GetIdentifier();

        public abstract Task<bool> Publish();

        public abstract Task SaveAsDraft();

        public sealed class InputValidationResult {
            public InputValidationResult() {
                Errors = new List<string>();
            }

            public InputValidationResult(List<string> errors) {
                Errors = errors;
            }

            public List<string> Errors { get; }

            public bool IsInputValid => Errors.Count == 0;

            public void Display(Window owner) {
                if (!IsInputValid)
                    MessageBox.Show(owner, $"Please resolve the following errors:\n{Errors.Sum(e => e + "\n")}",
                        "Input Validation",
                        MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }
    }
}
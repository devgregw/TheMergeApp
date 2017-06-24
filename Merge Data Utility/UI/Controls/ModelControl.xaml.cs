#region LICENSE

// Project Merge Data Utility:  ModelControl.xaml.cs (in Solution Merge Data Utility)
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Framework.Interfaces;
using MergeApi.Models.Core;
using MergeApi.Models.Core.Tab;
using Merge_Data_Utility.Tools;

#endregion

namespace Merge_Data_Utility.UI.Controls {
    /// <summary>
    ///     Interaction logic for ModelControl.xaml
    /// </summary>
    public partial class ModelControl : UserControl {
        private Action<object> _edit, _delete;

        private string _header;

        private string _id;

        private object _source;

        private string _subheader;

        public ModelControl() {
            InitializeComponent();
        }

        public ModelControl(object src, string i, string h, string sh, Action<object> e, Action<object> del) : this() {
            _source = src;
            _id = i;
            _header = h;
            _subheader = sh;
            _edit = e;
            _delete = del;
            id.Text = _id;
            header.Text = _header;
            subheader.Text = _subheader;
        }

        public ModelControl(object src, string i, string h, Action<object> e, Action<object> del, int fontSize = 12)
            : this(src, i, h, "", e, del) {
            Minify(fontSize);
        }

        public T GetSource<T>() where T : IIdentifiable {
            return (T) _source;
        }

        private static string MakeIdString(bool draft, params string[] pieces) {
            return pieces.Aggregate("", (current, s) => current + $"{s}{(pieces.Last() == s ? "" : "/")}") +
                   (draft ? " (draft)" : "");
        }

        private static T Condition<T>(bool condition, T @true, T @false) {
            return condition ? @true : @false;
        }

        public static ModelControl Create(object o, bool draft, Action<object> edit, Action<object> delete) {
            dynamic casted;
            if (o.TryCast<MergeEvent>(out casted))
                return new ModelControl(o, MakeIdString(draft, "events", casted.Id),
                    Condition(string.IsNullOrWhiteSpace(casted.Title), "(untitled)", casted.Title),
                    casted.ShortDescription, edit, delete);
            if (o.TryCast<MergeGroup>(out casted))
                return new ModelControl(o, MakeIdString(draft, "groups", casted.Id),
                    Condition(string.IsNullOrWhiteSpace(casted.Name), "(no name)", casted.Name),
                    $"{casted.Host} at {casted.Address}", edit, delete);
            if (o.TryCast<MergePage>(out casted))
                return new ModelControl(o, MakeIdString(draft, "pages", casted.Id),
                    Condition(string.IsNullOrWhiteSpace(casted.Title), "(untitled)", casted.Title),
                    casted.ShortDescription, edit, delete);
            if (o.TryCast<MergeLeader>(out casted))
                return new ModelControl(o, MakeIdString(draft, "leaders", casted.Id),
                    Condition(string.IsNullOrWhiteSpace(casted.Name), "(no name)", casted.Name), casted.Role, edit,
                    delete);
            if (o.TryCast<TabTip>(out casted))
                return new ModelControl(o, MakeIdString(draft, "tips/" + casted.Tab.ToString().ToLower(), casted.Id),
                    casted.Message, edit, delete, 12);
            throw new InvalidOperationException(
                $"Cannot create ModelControl with source of type {o.GetType().FullName}");
        }

        public void Minify(int fontSize = 12) {
            header.FontSize = fontSize;
            subheader.Visibility = Visibility.Collapsed;
        }

        private void Edit_Requested(object sender, RoutedEventArgs e) {
            _edit(_source);
        }

        private void Delete_Requested(object sender, RoutedEventArgs e) {
            _delete(_source);
        }
    }
}
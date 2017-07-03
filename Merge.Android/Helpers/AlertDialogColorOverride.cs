#region LICENSE

// Project Merge.Android:  AlertDialogColorOverride.cs (in Solution Merge.Android)
// Created by Greg Whatley on 06/30/2017 at 9:26 AM.
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

using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.App;
using Java.Lang;

#endregion

namespace Merge.Android.Helpers {
    public class AlertDialogColorOverride : Object, IDialogInterfaceOnShowListener {
        private AlertDialogColorOverride() { }
        public static AlertDialogColorOverride Instance => new AlertDialogColorOverride();

        public void OnShow(IDialogInterface dialog) {
            var map = new Dictionary<DialogButtonType, Color> {
                {DialogButtonType.Positive, Color.Argb(255, 33, 150, 243)},
                {DialogButtonType.Negative, Color.Argb(255, 77, 77, 77)},
                {DialogButtonType.Neutral, Color.Argb(255, 77, 77, 77)}
            };
            foreach (var type in map) {
                var button = ((AlertDialog) dialog).GetButton((int) type.Key);
                button?.SetTextColor(type.Value);
            }
        }
    }
}
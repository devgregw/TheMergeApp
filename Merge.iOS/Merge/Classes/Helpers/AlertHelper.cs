#region LICENSE

// Project Merge.iOS:  AlertHelper.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 10/28/2016 at 8:28 AM.
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
using System.Threading;
using Foundation;
using UIKit;

#endregion

namespace Merge.Classes.Helpers {
    public static class AlertHelper {
        public static void ShowTextInputAlert(string title, string message, bool password,
            Action<UITextField> fieldInitializer, Action<UIAlertView, nint> handler, string cancelButton,
            params string[] otherButtons) {
            var av = new UIAlertView(title, message, (IUIAlertViewDelegate) new AlertViewDelegate(handler),
                cancelButton, otherButtons);
            av.AlertViewStyle = password ? UIAlertViewStyle.SecureTextInput : UIAlertViewStyle.PlainTextInput;
            fieldInitializer(av.GetTextField(0));
            av.Show();
        }

        public static void ShowAlert(string title, string message, Action<UIAlertView, nint> handler,
            string cancelButton, params string[] otherButtons) {
#if DEBUG
            new Thread(() => {
                var obj = new NSObject();
                obj.InvokeOnMainThread(() => new UIAlertView(title, message,
                    (IUIAlertViewDelegate) new AlertViewDelegate(handler), cancelButton, otherButtons).Show());
            }).Start();
#else
			new UIAlertView(title, message, new AlertViewDelegate(handler), cancelButton, otherButtons).Show();
#endif
        }

        public static void ShowSheet(string title, Action<UIActionSheet, nint> del, string cancel, string destroy,
            params string[] other) {
            new UIActionSheet(title, (IUIActionSheetDelegate) new SheetDelegate(del), cancel, destroy, other).ShowFrom(
                new UIBarButtonItem(), true);
        }

        public class AlertViewDelegate : UIAlertViewDelegate, IUIAlertViewDelegate {
            public AlertViewDelegate(Action<UIAlertView, nint> handler) {
                Handler = handler;
            }

            public Action<UIAlertView, nint> Handler { get; }

            public override void Clicked(UIAlertView alertview, nint buttonIndex) {
                Handler?.Invoke(alertview, buttonIndex);
            }
        }

        public class SheetDelegate : UIActionSheetDelegate {
            public SheetDelegate(Action<UIActionSheet, nint> handler) {
                Handler = handler;
            }

            public Action<UIActionSheet, nint> Handler { get; }

            public override void Clicked(UIActionSheet actionSheet, nint buttonIndex) {
                Handler?.Invoke(actionSheet, buttonIndex);
            }
        }
    }
}
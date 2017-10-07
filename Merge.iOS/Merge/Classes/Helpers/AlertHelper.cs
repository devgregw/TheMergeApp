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
using CoreGraphics;
using UIKit;

#endregion

namespace Merge.Classes.Helpers {
    public static class AlertHelper {
        public static void ShowTextInputAlert(string title, string message, bool password,
            Action<UITextField> fieldInitializer, Action<string, string> handler, string cancelButton,
            params string[] otherButtons) {
            var av = new UIAlertView(title, message,
                (IUIAlertViewDelegate) new AlertViewDelegate2((a, i) =>
                    handler(i == a.CancelButtonIndex ? cancelButton : otherButtons[(int) i - 1],
                        a.GetTextField(0).Text)),
                cancelButton, otherButtons);
            av.AlertViewStyle = password ? UIAlertViewStyle.SecureTextInput : UIAlertViewStyle.PlainTextInput;
            fieldInitializer(av.GetTextField(0));
            av.Show();
        }

        public static void ShowAlert(string title, string message, Action<string> handler,
            string cancelButton, params string[] otherButtons) {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0)) {
                var controller = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
                if (!string.IsNullOrWhiteSpace(cancelButton))
                    controller.AddAction(UIAlertAction.Create(cancelButton, UIAlertActionStyle.Cancel,
                        a => handler?.Invoke(cancelButton)));
                foreach (var b in otherButtons ?? new string[] { })
                    controller.AddAction(UIAlertAction.Create(b, UIAlertActionStyle.Default, a => handler?.Invoke(b)));
                UIApplication.SharedApplication.KeyWindow.GetTopmostViewController()
                    .PresentViewController(controller, true, () => { });
            } else {
                new UIAlertView(title, message, (IUIAlertViewDelegate) new AlertViewDelegate(handler), cancelButton,
                    otherButtons).Show();
            }
        }

        public static void ShowSheet(string title, Action<string> handler, string cancel, string destroy,
            UIBarButtonItem source, params string[] otherButtons) {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0)) {
                var controller = UIAlertController.Create(title, null, UIAlertControllerStyle.ActionSheet);
                if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {
                    var top = UIApplication.SharedApplication.KeyWindow.GetTopmostViewController();
                    controller.PopoverPresentationController.SourceView = top.View;
                    var rect = CGRect.Empty;
                    rect.Location = new CGPoint(top.View.Bounds.GetMidX() - top.View.Frame.Location.X / 2,
                        top.View.Bounds.GetMidY() - top.View.Frame.Location.Y / 2);
                    controller.PopoverPresentationController.SourceRect = rect;
                    controller.PopoverPresentationController.PermittedArrowDirections = 0;
                }
                if (!string.IsNullOrWhiteSpace(cancel))
                    controller.AddAction(UIAlertAction.Create(cancel, UIAlertActionStyle.Cancel,
                        a => handler?.Invoke(cancel)));
                if (!string.IsNullOrWhiteSpace(destroy))
                    controller.AddAction(UIAlertAction.Create(destroy, UIAlertActionStyle.Destructive,
                        a => handler?.Invoke(destroy)));
                foreach (var b in otherButtons ?? new string[] { })
                    controller.AddAction(UIAlertAction.Create(b, UIAlertActionStyle.Default, a => handler?.Invoke(b)));
                UIApplication.SharedApplication.KeyWindow.GetTopmostViewController()
                    .PresentViewController(controller, true, () => { });
            } else {
                new UIActionSheet(title, (IUIActionSheetDelegate) new SheetDelegate(handler), cancel, destroy,
                    otherButtons).ShowFrom(new UIBarButtonItem(), true);
            }
        }

        public class AlertViewDelegate : UIAlertViewDelegate, IUIAlertViewDelegate {
            public AlertViewDelegate(Action<string> handler) {
                Handler = handler;
            }

            public Action<string> Handler { get; }

            public override void Clicked(UIAlertView alertview, nint buttonIndex) {
                Handler?.Invoke(alertview.ButtonTitle(buttonIndex));
            }
        }

        public class AlertViewDelegate2 : UIAlertViewDelegate, IUIAlertViewDelegate {
            public AlertViewDelegate2(Action<UIAlertView, nint> handler) {
                Handler = handler;
            }

            public Action<UIAlertView, nint> Handler { get; }

            public override void Clicked(UIAlertView alertview, nint buttonIndex) {
                Handler?.Invoke(alertview, buttonIndex);
            }
        }

        public class SheetDelegate : UIActionSheetDelegate {
            public SheetDelegate(Action<string> handler) {
                Handler = handler;
            }

            public Action<string> Handler { get; }

            public override void Clicked(UIActionSheet actionSheet, nint buttonIndex) {
                Handler?.Invoke(actionSheet.ButtonTitle(buttonIndex));
            }
        }
    }
}
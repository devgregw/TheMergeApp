#region LICENSE

// Project Merge.iOS:  AppDelegate.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 10/29/2016 at 10:59 AM.
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
using CoreLocation;
using Firebase.Auth;
using Firebase.CloudMessaging;
using Firebase.InstanceID;
using Foundation;
using Merge.Classes;
using Merge.Classes.Helpers;
using Merge.Classes.Receivers;
using MergeApi.Client;
using MergeApi.Framework.Abstractions;
using UIKit;
using UserNotifications;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

#endregion

namespace Merge.iOS {
    public interface IMessageSender { }

    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate, IMessageSender, IUNUserNotificationCenterDelegate {
        private bool _finishedLaunching;
        public static FirebaseAuthLink AuthLink { get; set; }
        public static CLLocationManager LocationManager { get; set; }

        public UIApplicationShortcutItem LaunchedShortcutItem { get; set; }

        public static void SetTintColors(UIColor main, UIColor contrast) {
            UINavigationBar.Appearance.BarTintColor = main;
            UINavigationBar.Appearance.TintColor = contrast;
            UIBarButtonItem.Appearance.TintColor = contrast;
            UITabBar.Appearance.BarTintColor = main;
            UITabBar.Appearance.TintColor = contrast;
            UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes {
                TextColor = contrast
            });
        }

        public static void RestoreTintColors() {
            SetTintColors(ColorConsts.AccentUiColor, UIColor.Black);
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken) {
#if DEBUG
            InstanceId.SharedInstance.SetApnsToken(deviceToken, ApnsTokenType.Sandbox);
#endif
#if RELEASE
			InstanceId.SharedInstance.SetApnsToken(deviceToken, ApnsTokenType.Prod);
#endif
        }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options) {
            if (options != null)
                LaunchedShortcutItem = options[UIApplication.LaunchOptionsShortcutItemKey] as UIApplicationShortcutItem;
            var userAgent = NSDictionary.FromObjectsAndKeys(
                new[] {
                    FromObject("Mozilla/5.0 (" + (UIDevice.CurrentDevice.Model.Contains("iPad") ? "iPad" : "iPhone") +
                               "; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A403 Safari/8536.25")
                }, new[] {FromObject("UserAgent")});
            NSUserDefaults.StandardUserDefaults.RegisterDefaults(userAgent);
            UITabBar.Appearance.BarTintColor = ColorConsts.PrimaryUiColor;
            Forms.Init();
            LoadApplication(new App());

            Firebase.Analytics.App.Configure();
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0)) {
                var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge |
                                  UNAuthorizationOptions.Sound;
                UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) => { });
                UNUserNotificationCenter.Current.Delegate = this;
            } else {
                var types = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                var settings = UIUserNotificationSettings.GetSettingsForTypes(types, null);
                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }
            UIApplication.SharedApplication.RegisterForRemoteNotifications();
            if (CLLocationManager.LocationServicesEnabled) {
                LocationManager = new CLLocationManager();
                LocationManager.Delegate = new MergeLocationDelegate();
                LocationManager.RequestWhenInUseAuthorization();
            }
            InstanceId.Notifications.ObserveTokenRefresh((s, e) => {
                Console.WriteLine($"[FIREBASE] instance ID refreshed: {InstanceId.SharedInstance.Token}");
            });
            MergeDatabase.Initialize(async () => {
                if (!string.IsNullOrWhiteSpace(PreferenceHelper.Token) &&
                    PreferenceHelper.TokenExpiration > DateTime.Now && PreferenceHelper.AuthenticationState !=
                    PreferenceHelper.LeaderAuthenticationState.Failed)
                    return PreferenceHelper.Token;
                if (PreferenceHelper.AuthenticationState == PreferenceHelper.LeaderAuthenticationState.NoAttempt) {
                    AuthLink = await MergeDatabase.AuthenticateAsync();
                    PreferenceHelper.AuthenticationState = PreferenceHelper.LeaderAuthenticationState.NoAttempt;
                } else if (PreferenceHelper.AuthenticationState ==
                           PreferenceHelper.LeaderAuthenticationState.Successful) {
                    try {
                        AuthLink = await MergeDatabase.AuthenticateAsync(PreferenceHelper.LeaderUsername,
                            PreferenceHelper.LeaderPassword);
                        PreferenceHelper.AuthenticationState =
                            PreferenceHelper.LeaderAuthenticationState.Successful;
                    } catch {
                        AuthLink = await MergeDatabase.AuthenticateAsync();
                        PreferenceHelper.AuthenticationState =
                            PreferenceHelper.LeaderAuthenticationState.Failed;
                    }
                }
                if (AuthLink != null) {
                    PreferenceHelper.Token = AuthLink.FirebaseToken;
                    PreferenceHelper.TokenExpiration = AuthLink.Created.AddSeconds(AuthLink.ExpiresIn);
                    return AuthLink.FirebaseToken;
                }
                return "";
            }, new MergeActionReceiver(), new MergeElementReceiver(), new MergeLogReceiver());

            RestoreTintColors();
            _finishedLaunching = true;
            return base.FinishedLaunching(app, options);
        }

        private void ShowNotificationAlert(string title, string message, ActionBase action) {
            if (action == null)
                AlertHelper.ShowAlert(title, message, (v, i) => { }, "Dismiss");
            else
                AlertHelper.ShowAlert(title, message, (v, i) => {
                    if (i == v.CancelButtonIndex)
                        action.Invoke();
                }, "Open", "Dismiss");
        }

        // Foreground: iOS 9 or earlier
        // Background: all
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo,
            Action<UIBackgroundFetchResult> completionHandler) {
            Messaging.SharedInstance.AppDidReceiveMessage(userInfo);
            Console.WriteLine($"[FIREBASE] (9)Message received: {userInfo}");
            var action = userInfo.ContainsKey(FromObject("action"))
                ? ActionBase.FromJson(userInfo.ObjectForKey(FromObject("action")).ToString())
                : null;
            if (application.ApplicationState == UIApplicationState.Active) {
                var alertInfo =
                    (NSDictionary) ((NSDictionary) userInfo.ObjectForKey(FromObject("aps")))
                    .ObjectForKey(FromObject("alert"));
                ShowNotificationAlert(alertInfo.ObjectForKey(new NSString("title")).ToString(),
                    alertInfo.ObjectForKey(new NSString("body")).ToString(), action);
            } else {
                action?.Invoke();
            }
        }

        // Foreground: iOS 10 or later
        // Background: none
        [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        // ReSharper disable once UnusedMember.Global
        public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification,
            Action<UNNotificationPresentationOptions> completionHandler) {
            Console.WriteLine($"[FIREBASE] (10)Message received: {notification.Request.Content.UserInfo}");
            var action = notification.Request.Content.UserInfo.ContainsKey(FromObject("action"))
                ? ActionBase.FromJson(notification.Request.Content.UserInfo.ObjectForKey(FromObject("action"))
                    .ToString())
                : null;
            ShowNotificationAlert(notification.Request.Content.Title, notification.Request.Content.Body, action);
        }

        public void ConnectFcm() {
            Messaging.SharedInstance.Connect(e => {
                if (e == null) {
                    //TODO: SUBSCRIBE OR UNSUBSCRIBE FROM LEADERS TOPIC
                }
                Console.WriteLine(
                    $"[FIREBASE] {(e == null ? "Connected successfully!" : $"Could not connect: {e.LocalizedDescription}")}");
            });
        }

        public override void OnActivated(UIApplication uiApplication) {
            ConnectFcm();
            base.OnActivated(uiApplication);
        }

        public override void DidEnterBackground(UIApplication uiApplication) {
            Messaging.SharedInstance.Disconnect();
        }

        public override void PerformActionForShortcutItem(UIApplication application,
            UIApplicationShortcutItem shortcutItem,
            UIOperationHandler completionHandler) {
            completionHandler(ShortcutItemSelected(shortcutItem));
        }

        public bool ShortcutItemSelected(UIApplicationShortcutItem shortcutItem) {
            if (shortcutItem == null)
                return false;
            MessagingCenter.Send<IMessageSender>(this, shortcutItem.Type);
            return true;
        }
    }
}
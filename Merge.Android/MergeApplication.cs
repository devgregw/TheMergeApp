﻿#region LICENSE

// Project Merge.Android:  MergeApplication.cs (in Solution Merge.Android)
// Created by Greg Whatley on 06/23/2017 at 10:33 AM.
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
using Android.App;
using Android.Runtime;
using Android.Util;
using Firebase.Auth;
using Firebase.Messaging;
using Merge.Android.Helpers;
using Merge.Android.Receivers;
using MergeApi.Client;
using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Interfaces.Receivers;

#endregion

namespace Merge.Android {
    [Application(Label = "Merge", AllowBackup = true, Icon = "@mipmap/ic_launcher", Theme = "@style/AppTheme",
        Logo = "@mipmap/ic_launcher", Debuggable = false)]
    /*[MetaData("com.google.android.gms.version", Value = "@integer/google_play_services_version")]
    [MetaData("com.google.android.geo.API_KEY", Value = "AIzaSyCShFAZLYGX2iGpBoWRuP6q0QfAaqAg-sA")]
    [MetaData("com.google.firebase.messaging.default_notification_icon", Resource = "@drawable/ic_notification")]*/
    public class MergeApplication : Application {
        public MergeApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) { }
        internal static FirebaseAuthLink AuthLink { get; set; }

        public override void OnCreate() {
            base.OnCreate();
            Log.Debug("MergeApplication", $"For debugging purposes: {GetString(Resource.String.google_app_id)}");
            PreferenceHelper.Initialize(this);
            LogHelper.Initialize(this);
            MergeDatabase.Initialize(async () => {
                    if (!string.IsNullOrWhiteSpace(PreferenceHelper.Token) &&
                        PreferenceHelper.TokenExpiration > DateTime.Now && PreferenceHelper.AuthenticationState !=
                        PreferenceHelper.LeaderAuthenticationState.Failed)
                        return PreferenceHelper.Token;
                    if (PreferenceHelper.AuthenticationState == PreferenceHelper.LeaderAuthenticationState.NoAttempt) {
                        AuthLink = await MergeDatabase.AuthenticateAsync();
                        PreferenceHelper.AuthenticationState =
                            PreferenceHelper.LeaderAuthenticationState.NoAttempt;
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
                }, new MergeActionInvocationReceiver(), new MergeElementCreationReceiver(this),
                new MergeLogger());
        }

        public class MergeLogger : ILogReceiver {
            public bool Initialize() {
                return true;
            }

            public void Log(LogLevel level, string sender, string message) {
                LogHelper.WriteMessage(level.ToString().ToUpper(), $"{sender}: {message}");
            }

            public void Log(LogLevel level, string sender, Exception e) {
                LogHelper.WriteException(e, false, null);
            }
        }
    }
}
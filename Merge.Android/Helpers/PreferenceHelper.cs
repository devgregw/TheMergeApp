#region LICENSE

// Project Merge.Android:  PreferenceHelper.cs (in Solution Merge.Android)
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Preferences;
using Android.Provider;
using Android.Widget;
using Firebase.Messaging;
using Java.Lang;
using Merge.Android.UI.Activities;
using MergeApi.Framework.Enumerations;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Object = Java.Lang.Object;

#endregion

namespace Merge.Android.Helpers {
    /// <summary>
    ///     A helper class that saves and loads preferences
    /// </summary>
    public static class PreferenceHelper {
        public enum LeaderAuthenticationState {
            NoAttempt = -1,
            Successful = 0,
            Failed = 1
        }

        /// <summary>
        ///     The context
        /// </summary>
        private static Context _context;

        private static ISharedPreferences _preferences;

        public static bool FirstRun {
            get => _preferences
                .GetBoolean("firstRun", true);
            set => _preferences
                .Edit()
                .PutBoolean("firstRun", value)
                .Commit();
        }

        public static string Token {
            get => _preferences
                .GetString("token", "");
            set => _preferences.Edit()
                .PutString("token", value).Commit();
        }

        public static DateTime TokenExpiration {
            get => DateTime.Parse(_preferences
                .GetString("tokenExpiration", ""));
            set => _preferences.Edit()
                .PutString("tokenExpiration", value.ToString(CultureInfo.CurrentUICulture)).Commit();
        }

        public static bool IsValidLeader => AuthenticationState == LeaderAuthenticationState.Successful &&
                                            !string.IsNullOrWhiteSpace(LeaderUsername) &&
                                            !string.IsNullOrWhiteSpace(LeaderPassword);

        public static string LeaderUsername {
            get => _preferences
                .GetString("leaderUsername", "");
            set => _preferences.Edit()
                .PutString("leaderUsername", value).Commit();
        }

        public static string LeaderPassword {
            get => _preferences
                .GetString("leaderPassword", "");
            set => _preferences.Edit()
                .PutString("leaderPassword", value).Commit();
        }

        public static LeaderAuthenticationState AuthenticationState {
            get => (LeaderAuthenticationState) int.Parse(_preferences
                .GetString("leaderAuthenticationState", "-1"));
            set => _preferences.Edit()
                .PutString("leaderAuthenticationState", ((int) value).ToString()).Commit();
        }

        public static string[] DismissedTips {
            get => _preferences.GetString("dismissedTips", "")
                .Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            set => _preferences.Edit().PutString("dismissedTips", string.Join(";", value)).Commit();
        }

        /// <summary>
        ///     Gets the grade levels
        /// </summary>
        /// <value>The grade levels</value>
        public static ICollection<GradeLevel> GradeLevels {
            get {
                return
                    _preferences
                        .GetStringSet("gradeLevels", new List<string>())
                        .Select(s => s.ToEnum<GradeLevel>())
                        .ToList();
            }
            set => _preferences
                .Edit()
                .PutStringSet("gradeLevels", (from e in value select e.ToString()).ToList())
                .Commit();
        }

        /// <summary>
        ///     Gets the genders
        /// </summary>
        /// <value>The genders</value>
        public static ICollection<Gender> Genders {
            get =>
                _preferences
                    .GetStringSet("genders", new List<string>())
                    .Select(s => s.ToEnum<Gender>())
                    .ToList();
            set => _preferences
                .Edit()
                .PutStringSet("genders", (from e in value select e.ToString()).ToList())
                .Commit();
        }

        public static bool IsLeader {
            get => _preferences.GetBoolean("isLeader", false);
            set => _preferences.Edit().PutBoolean("isLeader", value).Commit();
        }

        public static bool ShowInvalidObjects {
            get => _preferences.GetBoolean("showInvalidObjects", false);
            set => _preferences.Edit().PutBoolean("showInvalidObjects", value).Commit();
        }

        /// <summary>
        ///     Gets a value indicating whether the app will cache data.
        /// </summary>
        /// <value><c>true</c> if caching is enabled; otherwise, <c>false</c></value>
        public static bool Caching => _preferences
            .GetBoolean("caching", true);

        //public static bool Telemetry => _preferences
            //.GetBoolean("telemetry", true);

        public static bool Logging {
            /*get => _preferences.GetBoolean("logging", false);
            set => _preferences.Edit().PutBoolean("logging", value).Commit();*/
            get => false;
        }

        public static void AddDismissedTip(string id) => DismissedTips = DismissedTips.Concat(new[] { id }).ToArray();

        public static void Initialize(Context context) {
            _context = context;
            PreferenceManager.SetDefaultValues(_context, Resource.Xml.Preferences, false);
            _preferences = PreferenceManager.GetDefaultSharedPreferences(_context);
        }

        public class PreferenceChangeListener : Object, ISharedPreferencesOnSharedPreferenceChangeListener {
            private readonly Context _c;

            public PreferenceChangeListener(Context c) {
                _c = c;
            }

            public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key) {
                if (key == "isLeader") {
                    if (IsLeader) {
                        Toast.MakeText(_c, "Credentials are required.", ToastLength.Long).Show();
                        var intent = new Intent(_context, typeof(WelcomeActivity));
                        intent.PutExtra("leaderRoutine", true);
                        _c.StartActivity(intent);
                    } else {
                        Token = "";
                        TokenExpiration = DateTime.MinValue;
                        LeaderUsername = "";
                        LeaderPassword = "";
                        AuthenticationState = LeaderAuthenticationState.NoAttempt;
                        FirebaseMessaging.Instance.UnsubscribeFromTopic("verified_leader");
                    }
                }
                else if (key == "caching") {
                    var dialog = new AlertDialog.Builder(_c).SetTitle("Restart Required")
                        .SetMessage("To apply your changes, the Merge app must be restarted.").SetPositiveButton(
                            "Restart",
                            (s, e) => {
                                var intent =
                                    PendingIntent.GetActivity(_c, 0,
                                        _c.PackageManager.GetLaunchIntentForPackage(_c.PackageName), PendingIntentFlags.CancelCurrent);
                                ((AlarmManager)_c.GetSystemService(Context.AlarmService)).Set(AlarmType.Rtc, JavaSystem.CurrentTimeMillis() + 1, intent);
                                JavaSystem.Exit(2);
                            }).SetNegativeButton("Later", (s, e) => { }).SetCancelable(false).Create();
                    dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
                    dialog.Show();
                }
            }
        }
    }
}
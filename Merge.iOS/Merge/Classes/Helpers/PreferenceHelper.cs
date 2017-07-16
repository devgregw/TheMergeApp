#region LICENSE

// Project Merge.iOS:  PreferenceHelper.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 10/27/2016 at 12:51 PM.
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
using Foundation;
using MergeApi.Framework.Enumerations;

#endregion

namespace Merge.Classes.Helpers {
    public static class PreferenceHelper {
        public enum LeaderAuthenticationState {
            NoAttempt = -1,
            Successful = 0,
            Failed = 1
        }

        public static NSUserDefaults Preferences => NSUserDefaults.StandardUserDefaults;

        public static bool HasLaunchedBefore {
            get => Preferences.BoolForKey("hasLaunchedBefore");
            set {
                Preferences.SetBool(value, "hasLaunchedBefore");
                Preferences.Synchronize();
            }
        }

        public static bool IsValidLeader => LeaderFeaturesEnabled &&
                                            AuthenticationState == LeaderAuthenticationState.Successful &&
                                            !string.IsNullOrWhiteSpace(LeaderUsername) &&
                                            !string.IsNullOrWhiteSpace(LeaderPassword);

        public static string Token {
            get => Preferences.StringForKey("token") ?? "";
            set {
                Preferences.SetString(value, "token");
                Preferences.Synchronize();
            }
        }

        public static DateTime TokenExpiration {
            get => DateTime.Parse(Preferences.StringForKey("tokenExpiration") ?? "");
            set {
                Preferences.SetString(value.ToString(CultureInfo.CurrentUICulture), "tokenExpiration");
                Preferences.Synchronize();
            }
        }

        public static string LeaderUsername {
            get => Preferences.StringForKey("leaderUsername") ?? "";
            set {
                Preferences.SetString(value, "leaderUsername");
                Preferences.Synchronize();
            }
        }

        public static string LeaderPassword {
            get => Preferences.StringForKey("leaderPassword") ?? "";
            set {
                Preferences.SetString(value, "leaderPassword");
                Preferences.Synchronize();
            }
        }

        public static LeaderAuthenticationState AuthenticationState {
            get => (LeaderAuthenticationState) int.Parse(Preferences.StringForKey("leaderAuthenticationState") ?? "-1");
            set {
                Preferences.SetString(((int) value).ToString(), "leaderAuthenticationState");
                Preferences.Synchronize();
            }
        }

        public static string[] DismissedTips {
            get => (Preferences.StringForKey("dismissedTips") ?? "").Split(new[] {';'},
                StringSplitOptions.RemoveEmptyEntries);
            set {
                Preferences.SetString(string.Join(";", value), "dismissedTips");
                Preferences.Synchronize();
            }
        }

        public static bool LeaderFeaturesEnabled {
            get => Preferences.BoolForKey("leaderOnlyFeatures");
            set {
                Preferences.SetBool(value, "leaderOnlyFeatures");
                Preferences.Synchronize();
            }
        }

        public static List<GradeLevel> GradeLevels {
            get {
                var strings = new[] {"seventh", "eighth", "ninth", "tenth", "eleventh", "twelfth"};
                var values = new List<bool> {
                    Preferences.BoolForKey("seventh"),
                    Preferences.BoolForKey("eighth"),
                    Preferences.BoolForKey("ninth"),
                    Preferences.BoolForKey("tenth"),
                    Preferences.BoolForKey("eleventh"),
                    Preferences.BoolForKey("twelfth")
                };
                var results = new List<GradeLevel>();
                for (var i = 0; i < values.Count; i++) {
                    var b = values[i];
                    if (b)
                        results.Add((GradeLevel) Enum.Parse(typeof(GradeLevel), strings[i], true));
                }
                return results;
            }
            set {
                var all = new[] {"seventh", "eighth", "ninth", "tenth", "eleventh", "twelfth"};
                var set = (from grade in value select grade.ToString().ToLower()).ToList();
                foreach (var grade in all)
                    Preferences.SetBool(set.Contains(grade), grade);
                Preferences.Synchronize();
            }
        }

        public static List<Gender> Genders {
            get {
                var strings = new[] {"male", "female"};
                var values = new List<bool> {
                    Preferences.BoolForKey("male"),
                    Preferences.BoolForKey("female")
                };
                var results = new List<Gender>();
                for (var i = 0; i < values.Count; i++) {
                    var b = values[i];
                    if (b)
                        results.Add((Gender) Enum.Parse(typeof(Gender), strings[i], true));
                }
                return results;
            }
            set {
                var all = new[] {"male", "female"};
                var set = (from gender in value select gender.ToString().ToLower()).ToList();
                foreach (var gender in all)
                    Preferences.SetBool(set.Contains(gender), gender);
                Preferences.Synchronize();
            }
        }

        public static bool Caching {
            get => Preferences.BoolForKey("caching");
            set {
                Preferences.SetBool(value, "caching");
                Preferences.Synchronize();
            }
        }

        public static void AddDismissedTip(string id) {
            DismissedTips = DismissedTips.Concat(new[] {id}).ToArray();
        }
    }
}
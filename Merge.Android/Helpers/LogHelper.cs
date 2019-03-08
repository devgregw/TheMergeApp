#region LICENSE

// Project Merge.Android:  LogHelper.cs (in Solution Merge.Android)
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
using Android.App;
using Android.Content;
using Android.Util;
using Firebase.Analytics;
using Firebase.Crash;
using Java.Lang;
using Enum = System.Enum;
using Exception = System.Exception;
using Process = Android.OS.Process;

#endregion

namespace Merge.Android.Helpers {
    public static class LogHelper {
        private static Context _context;

        public static void FirebaseLog(Context c, string name, Dictionary<string, string> values) => FirebaseAnalytics
            .GetInstance(c).LogEvent(name, values.Concat(new Dictionary<string, string> {
                {"instanceId", MergeApplication.InstanceId},
                {"when", DateTime.Now.ToLongTimeString()}
#if DEBUG
                ,
                {"debug", "true"}
#endif
            }).ToBundle());

        public static void Initialize(Context c) {
            _context = c;
            WriteMessage("INFO",
                $"*** WELCOME TO MERGE ***\n*** VERSION {VersionConsts.Version} ({VersionConsts.Classification}, UPDATE {VersionConsts.Update}) BY GREG WHATLEY ***\n*** LOGGING INITIALIZED ***");
            WriteMessage("DEBUG", $"Instance Id: {MergeApplication.InstanceId}");
#if DEBUG
            WriteMessage("INFO", "*** DEBUGGING ***");
#endif
        }

        private static void ShowErrorMessage(Type exType, string msg, string stacktrace, Action retryAction) {
            var d = new AlertDialog.Builder(_context).Create();
            var builder = new AlertDialog.Builder(_context)
                .SetTitle("Something Went Wrong")
                .SetNeutralButton("Details",
                    (s, args) => new AlertDialog.Builder(_context).SetTitle(exType.Name).SetMessage(
                            $"{msg}\n\nSTACK TRACE:\n\n{stacktrace}")
                        .SetPositiveButton("Close", (s2, args2) => { d.Show(); })
                        .Show())
                .SetNegativeButton("Exit", (s, args) => Process.KillProcess(Process.MyPid()))
                .SetCancelable(false);
            if (retryAction != null) {
                builder.SetMessage(
                    "An unexpected error occurred and Merge cannot continue.  Click 'Retry' to try again, 'Exit' to close Merge, or 'Details' to view the error's details.");
                builder.SetPositiveButton("Retry", (s, e) => retryAction());
            } else {
                builder.SetMessage(
                    "An unexpected error occurred and Merge cannot continue.  Click 'Exit' to close Merge or 'Details' to view the error's details.");
            }
            d = builder.Create();
            d.Show();
        }

        public static void WriteException(Exception ex, bool showMessage, Action retryAction) {
            if (showMessage)
                ShowErrorMessage(ex.GetType(), ex.Message, ex.StackTrace, retryAction);
            FirebaseCrash.Report(ex);
            WriteMessage("ERROR",
                $"*** EXCEPTION ***\n*** {ex.GetType().FullName}: {ex.Message} ***\n*** BEGIN STACKTRACE ***\n{ex.StackTrace}\n*** END STACKTRACE ***");
        }

        public static void WriteMessage(string level, string message) {
            if (message.Contains("\n")) {
                var msgs = message.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var msg in msgs)
                    WriteMessage(level, msg);
            } else {
                if (!Enum.TryParse(level, true, out LogPriority p))
                    p = LogPriority.Info;
                FirebaseCrash.Log(message);
                Log.WriteLine(p, "MergeApp", message);
            }
        }
    }
}
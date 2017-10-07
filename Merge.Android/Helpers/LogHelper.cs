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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Support.V4.Content;
using Android.Util;
using Firebase.Analytics;
using Firebase.Crash;
using Java.Lang;
using Enum = System.Enum;
using Exception = System.Exception;
using File = Java.IO.File;
using Process = Android.OS.Process;
using Uri = Android.Net.Uri;

#endregion

namespace Merge.Android.Helpers {
    public static class LogHelper {
        //private static File _file;
        //private static File _folder;
        private static Context _context;
        private static bool _enable;

        public static void FirebaseLog(Context c, string name, Dictionary<string, string> values) => FirebaseAnalytics
            .GetInstance(c).LogEvent(name, values.Concat(new Dictionary<string, string> {
                {"instanceId", MergeApplication.InstanceId},
                {"when", DateTime.Now.ToLongTimeString()}
#if DEBUG
                ,
                {"debug", "true"}
#endif
            }).ToBundle());

        /*public static async Task<string[]> GetAllLogs() {
            WriteMessage("INFO", "Listing logs");
            var logs =
                (await _folder.ListFilesAsync()).Select(f => f.Name)
                .Where(s => _file == null || s != _file.Name)
                .ToArray();
            foreach (var l in logs)
                WriteMessage("DEBUG", $"Found log: {l}");
            return logs;
        }

        public static async Task DeleteAllLogs() {
            WriteMessage("WARN", "Deleting all logs");
            var files = await _folder.ListFilesAsync();
            foreach (
                var file in files.Where(f => f.Name.Contains("merge")).Where(f => _file == null || f.Name != _file.Name)
            ) {
                WriteMessage("WARN", $"Deleting log: {file.Name}");
                file.Delete();
            }
        }*/

        public static void Initialize(Context c) {
            _context = c;
            /*_folder = _context.GetExternalFilesDir(null);
            if (!_folder.Exists())
                _folder.Mkdir();
            _enable = PreferenceHelper.Logging;
            if (!_enable)
                return;
            _file = new File(_context.GetExternalFilesDir(null),
                $"mergeandroid-{VersionConsts.Version}-{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}.txt");
            if (!_file.Exists())
                _file.CreateNewFile();*/
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

        public static void WriteException(Throwable tr, bool showMessage, Action retryAction) {
            if (showMessage)
                ShowErrorMessage(tr.GetType(), tr.Message, tr.StackTrace, retryAction);
            FirebaseCrash.Report(tr);
            WriteMessage("ERROR",
                $"*** EXCEPTION ***\n*** {tr.GetType().FullName}: {tr.Message} ***\n*** BEGIN STACKTRACE ***\n{tr.StackTrace}\n*** END STACKTRACE ***");
        }

        public static void WriteException(Exception ex, bool showMessage, Action retryAction) {
            if (showMessage)
                ShowErrorMessage(ex.GetType(), ex.Message, ex.StackTrace, retryAction);
            FirebaseCrash.Report(ex);
            WriteMessage("ERROR",
                $"*** EXCEPTION ***\n*** {ex.GetType().FullName}: {ex.Message} ***\n*** BEGIN STACKTRACE ***\n{ex.StackTrace}\n*** END STACKTRACE ***");
        }

        /*public static void SendLog(string filename) {
            WriteMessage("INFO", $"Sending log: {filename}");
            var file = new File(_context.GetExternalFilesDir(null), filename);
            var path = SdkChecker.Nougat
                ? FileProvider.GetUriForFile(_context, GenericFileProvider.GetAuthority(_context),
                    file)
                : Uri.FromFile(file);
            var intent = new Intent(Intent.ActionSend);
            intent.SetType("vnd.android.cursor.dir/email");
            intent.PutExtra(Intent.ExtraEmail, new[] {"devgregw@outlook.com"});
            intent.PutExtra(Intent.ExtraStream, path);
            intent.PutExtra(Intent.ExtraSubject, "Merge Android Log Submission");
            intent.AddFlags(ActivityFlags.NewTask);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission);
            _context.StartActivity(intent);
        }*/

        public static void WriteMessage(string level, string message) {
            if (message.Contains("\n")) {
                var msgs = message.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var msg in msgs)
                    WriteMessage(level, msg);
            } else {
                /*if (_enable && ContextCompat.CheckSelfPermission(_context, Manifest.Permission.WriteExternalStorage) ==
                    Permission.Granted)
                    using (var stream = System.IO.File.Open(_file.AbsolutePath, FileMode.Append)) {
                        using (var writer = new StreamWriter(stream)) {
                            writer.Write($"[{DateTime.Now.ToString("HH:mm:ss")}] [{level}] {message}\n");
                        }
                    }*/
                LogPriority p;
                if (!Enum.TryParse(level, true, out p))
                    p = LogPriority.Info;
                FirebaseCrash.Log(message);
                Log.WriteLine(p, "MergeApp", message);
                //Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] [{level}] {message}\n");
            }
        }
    }
}
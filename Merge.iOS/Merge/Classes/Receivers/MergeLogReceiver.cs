#region LICENSE

// Project Merge.iOS:  MergeLogReceiver.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 06/28/2017 at 7:50 AM.
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
using System.Diagnostics;
using System.Linq;
using Firebase.Analytics;
using Firebase.CrashReporting;
using Foundation;
using MergeApi.Framework.Interfaces.Receivers;
using LogLevel = MergeApi.Framework.Enumerations.LogLevel;

#endregion

namespace Merge.Classes.Receivers {
    public sealed class MergeLogReceiver : ILogReceiver {
        public static string SessionId { get; private set; }

        public bool Initialize() {
            SessionId = Guid.NewGuid().ToString();
            Log(LogLevel.Info, "Merge.Classes.Receivers.MergeLogReceiver", "Session ID: " + SessionId);
            return true;
        }

        public void Log(LogLevel level, string sender, string message) {
            Debug.WriteLine($"[{level}]: {message} (from \"{sender}\")");
            CrashReporting.Log($"[{level}]: {message} (from \"{sender}\")");
        }

        public void Log(LogLevel level, string sender, Exception e) {
            Debug.WriteLine(
                $"[{level}]: {e.Message} ({e.GetType().FullName}) (from \"{sender}\")\n{e.StackTrace}\n-----");
            CrashReporting.Log(
                $"[{level}]: {e.Message} ({e.GetType().FullName}) (from \"{sender}\")\n{e.StackTrace}\n-----");
        }

        public static void Log(string name, Dictionary<string, string> items) {
            var allItems = items.Concat(new Dictionary<string, string> {
                {"instanceId", SessionId},
                {"when", DateTime.Now.ToLongTimeString()}
#if DEBUG
                ,
                {"debug", "true"}
#endif
            });
            var newDict = allItems
                .Select(p => new KeyValuePair<NSString, NSObject>(new NSString(p.Key), NSObject.FromObject(p.Value)))
                .ToDictionary(p => p.Key, p => p.Value);
            Analytics.LogEvent(name,
                new NSDictionary<NSString, NSObject>(newDict.Keys.ToArray(), newDict.Values.ToArray()));
        }
    }
}
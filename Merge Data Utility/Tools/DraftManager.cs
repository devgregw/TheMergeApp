#region LICENSE

// Project Merge Data Utility:  DraftManager.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 03/20/2017 at 6:42 PM.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using MergeApi.Framework.Interfaces;
using MergeApi.Models.Core;
using MergeApi.Models.Core.Tab;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Merge_Data_Utility.Tools {
    public static class DraftManager {
        private static JObject _content {
            get {
                if (!File.Exists("drafts.json")) {
                    File.Open("drafts.json", FileMode.OpenOrCreate).Close();
                    File.WriteAllText("drafts.json",
                        new JObject(new JProperty("events", new JArray()), new JProperty("groups", new JArray()),
                            /*new JProperty("announcements", new JArray()), */new JProperty("pages", new JArray()),
                            new JProperty("tips", new JArray())
                            /*new JProperty("leaders", new JArray())*/).ToString());
                }
                return JObject.Parse(File.ReadAllText("drafts.json"));
            }
            set => File.WriteAllText("drafts.json", value.ToString());
        }

        public static List<T> GetAllOfType<T>(string key) where T : IIdentifiable {
            try {
                return ((JArray) _content[key]).Select(token => token.ToObject<T>()).ToList();
            } catch {
                return new List<T>();
            }
        }

        public static IEnumerable<IIdentifiable> GetAllDrafts() {
            return
                GetAllOfType<MergeEvent>("events")
                    .Cast<IIdentifiable>()
                    .Concat(GetAllOfType<MergeGroup>("groups"))
                    .Concat(GetAllOfType<MergePage>("pages"))
                    .Concat(GetAllOfType<TabTip>("tips"))
                    //.Concat(GetAllOfType<MergeLeader>("leaders"))
                    .ToList();
        }

        private static void Add(string key, IIdentifiable thing) {
            var copy = _content;
            var arr = (JArray) copy[key];
            if (arr.Any(t => t.Value<string>("id") == thing.Id))
                arr[arr.IndexOf(arr.First(t => t.Value<string>("id") == thing.Id))] =
                    JObject.Parse(JsonConvert.SerializeObject(thing));
            else
                arr.Add(JObject.Parse(JsonConvert.SerializeObject(thing)));
            copy[key] = arr;
            _content = copy;
        }

        private static void Delete(string key, IIdentifiable thing) {
            var copy = _content;
            var arr = (JArray) copy[key];
            if (arr.Any(t => t.Value<string>("id") == thing.Id))
                arr.RemoveAt(arr.IndexOf(arr.First(t => t.Value<string>("id") == thing.Id)));
            copy[key] = arr;
            _content = copy;
        }

        [SuppressMessage("ReSharper", "CanBeReplacedWithTryCastAndCheckForNull")]
        public static void AutoDelete(IIdentifiable item) {
            if (item is MergeEvent)
                DeleteDraftedEvent((MergeEvent) item);
            else if (item is MergeGroup)
                DeleteDraftedGroup((MergeGroup) item);
            else if (item is MergePage)
                DeleteDraftedPage((MergePage) item);
            /*else if (item is MergeLeader)
                DeleteDraftedLeader((MergeLeader) item);*/
            else if (item is TabTip)
                DeleteDraftedTip((TabTip) item);
            else
                throw new InvalidOperationException(
                    $"Cannot delete object of type {item.GetType().FullName} via DraftManager");
        }

        public static List<MergeEvent> GetDraftedEvents() {
            return GetAllOfType<MergeEvent>("events");
        }

        public static void AddDraftedEvent(MergeEvent e) {
            Add("events", e);
        }

        public static void DeleteDraftedEvent(MergeEvent e) {
            Delete("events", e);
        }

        public static List<MergeGroup> GetDraftedGroups() {
            return GetAllOfType<MergeGroup>("groups");
        }

        public static void AddDraftedGroup(MergeGroup g) {
            Add("groups", g);
        }

        public static void DeleteDraftedGroup(MergeGroup g) {
            Delete("groups", g);
        }

        public static List<MergePage> GetDraftedPages() {
            return GetAllOfType<MergePage>("pages");
        }

        public static void AddDraftedPage(MergePage p) {
            Add("pages", p);
        }

        public static void DeleteDraftedPage(MergePage p) {
            Delete("pages", p);
        }

        /*public static List<MergeLeader> GetDraftedLeaders() {
            return GetAllOfType<MergeLeader>("leaders");
        }

        public static void AddDraftedLeader(MergeLeader l) {
            Add("leaders", l);
        }

        public static void DeleteDraftedLeader(MergeLeader l) {
            Delete("leaders", l);
        }*/

        public static List<TabTip> GetDraftedTips() {
            return GetAllOfType<TabTip>("tips");
        }

        public static void AddDraftedTip(TabTip t) {
            Add("tips", t);
        }

        public static void DeleteDraftedTip(TabTip t) {
            Delete("tips", t);
        }
    }
}
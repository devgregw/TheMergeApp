#region LICENSE

// Project Merge.Android:  SessionRepo.cs (in Solution Merge.Android)
// Created by Greg Whatley on 05/22/2017 at 9:50 PM.
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
using MergeApi.Models.Core;

#endregion

namespace Merge.Android.Classes.Helpers {
    /// <summary>
    ///     A helper that allows the transfer of data
    /// </summary>
    public static class SessionRepo {
        /// <summary>
        ///     The data repository
        /// </summary>
        private static Dictionary<string, object> data;

        /// <summary>
        ///     A getter/setter for events
        /// </summary>
        /// <value>The events</value>
        public static List<MergeEvent> Events {
            get => (List<MergeEvent>) Get("events");
            set => Add("events", value);
        }

        /// <summary>
        ///     A getter/setter for groups
        /// </summary>
        /// <value>The groups</value>
        public static List<MergeGroup> Groups {
            get => (List<MergeGroup>) Get("groups");
            set => Add("groups", value);
        }

        /// <summary>
        ///     A getter/setter for announcements
        /// </summary>
        /// <value>The announcements</value>
        /*public static List<MergeAnnouncement> Announcements {
            get { return (List<MergeAnnouncement>) Get("announcements"); }
            set { Add("announcements", value); }
        }*/
        /// <summary>
        ///     A getter/setter for pages
        /// </summary>
        /// <value>The pages</value>
        public static List<MergePage> Pages {
            get => (List<MergePage>) Get("pages");
            set => Add("pages", value);
        }

        /// <summary>
        ///     Instantiates data if it hasn't already been instantiated
        /// </summary>
        private static void TryCreateData() {
            if (data == null) data = new Dictionary<string, object>();
        }

        /// <summary>
        ///     Adds the specified object to the repository, giving it the id
        /// </summary>
        /// <param name="id">The object's id</param>
        /// <param name="value">The object to add</param>
        public static void Add(string id, object value) {
            TryCreateData();
            if (data.ContainsKey(id))
                Remove(id);
            data.Add(id, value);
        }

        /// <summary>
        ///     Add the specified object to the repository, giving it a random GUID
        /// </summary>
        /// <param name="value">The object to add</param>
        /// <returns>The object's id</returns>
        public static string Add(object value) {
            TryCreateData();
            var id = Guid.NewGuid().ToString();
            Add(id, value);
            return id;
        }

        /// <summary>
        ///     Gets an object from the repository
        /// </summary>
        /// <param name="id">The object's id</param>
        public static object Get(string id) {
            TryCreateData();
            return data.ContainsKey(id) ? data.GetItem<object>(id, null) : null;
        }

        /// <summary>
        ///     Removes an object from the repository
        /// </summary>
        /// <param name="id">The object's id</param>
        public static void Remove(string id) {
            var v = Get(id);
            if (v is IDisposable && v != null)
                ((IDisposable) v).Dispose();
            if (data.ContainsKey(id))
                data.Remove(id);
        }
    }
}
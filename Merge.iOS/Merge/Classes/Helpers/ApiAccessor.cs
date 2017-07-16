#region LICENSE

// Project Merge.iOS:  ApiAccessor.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 10/27/2016 at 8:20 AM.
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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;

#endregion

namespace Merge.Classes.Helpers {
    public static class ApiAccessor {
        #region Utilities

        public static string GetTargetingString<T>(T obj) where T : TargetableBase {
            var genders = (from g in obj.Genders select g.ToString().ToLower() == "male" ? "guys" : "girls").Format();
            var sets = new List<List<GradeLevel>>();
            var inSet = false;
            var currentSet = new List<GradeLevel>();
            for (var i = 0; i < obj.GradeLevels.Count; i++)
                if (!inSet) {
                    inSet = true;
                    currentSet = new List<GradeLevel> {obj.GradeLevels[i]};
                    if (i + 1 >= obj.GradeLevels.Count) {
                        sets.Add(currentSet);
                        break;
                    }
                    if ((int) obj.GradeLevels[i] + 1 == (int) obj.GradeLevels[i + 1]) continue;
                    sets.Add(currentSet);
                    inSet = false;
                } else {
                    if (i + 1 >= obj.GradeLevels.Count) {
                        currentSet.Add(obj.GradeLevels[i]);
                        sets.Add(currentSet);
                        break;
                    }
                    if ((int) obj.GradeLevels[i] + 1 == (int) obj.GradeLevels[i + 1]) {
                        currentSet.Add(obj.GradeLevels[i]);
                    } else {
                        currentSet.Add(obj.GradeLevels[i]);
                        sets.Add(currentSet);
                        inSet = false;
                    }
                }
            var grades = "";
            foreach (var set in sets)
                if (set.Count == 1)
                    grades += (int) set[0] + "th,";
                else
                    grades += $"{(int) set.First()}th thru {(int) set.Last()}th,";
            try {
                if (grades.Contains(",")) {
                    var s = grades.Remove(grades.LastIndexOf(","));
                    var builder = new StringBuilder(s);
                    builder.Replace(",", "{and}", s.LastIndexOf(",") - 1, 2);
                    builder.Append(" grade");
                    grades = builder.ToString().Replace(",", ", ").Replace("{and}", ", and ");
                }
            } catch {
                grades = grades.Remove(grades.LastIndexOf(",")) + " grade";
            }
            return $"For {genders} in {grades}.";
        }

        #endregion

        /*#region Getters

        /// <summary>
        /// Gets an event
        /// </summary>
        /// <returns>The event</returns>
        /// <param name="id">The event's id</param>
        public static async Task<MergeEvent> GetEvent(string id) {
            var data = await ContactServer(ApiDataType.Events, id, null);
            var final = MergeEvent.FromJson(JArray.Parse(data)).First();
            return final;
        }

        /// <summary>
        /// Gets all events
        /// </summary>
        /// <returns>The events</returns>
        public static async Task<List<MergeEvent>> GetEvents() {
            var data = await ContactServer(ApiDataType.Events, string.Empty, null);
            var final = MergeEvent.FromJson(JArray.Parse(data));
            return final;
        }

        /// <summary>
        /// Gets a group
        /// </summary>
        /// <returns>The group</returns>
        /// <param name="id">The group's id</param>
        public static async Task<MergeGroup> GetGroup(string id) {
            var data = await ContactServer(ApiDataType.Groups, id, null);
            var final = MergeGroup.FromJson(JArray.Parse(data)).First();
            return final;
        }

        /// <summary>
        /// Gets all groups
        /// </summary>
        /// <returns>The groups</returns>
        public static async Task<List<MergeGroup>> GetGroups() {
            var data = await ContactServer(ApiDataType.Groups, string.Empty, null);
            var final = MergeGroup.FromJson(JArray.Parse(data));
            return final;
        }

        /// <summary>
        /// Gets an announcement
        /// </summary>
        /// <returns>The announcement</returns>
        /// <param name="id">The announcement's id</param>
        public static async Task<MergeAnnouncement> GetAnnouncement(string id) {
            var data = await ContactServer(ApiDataType.Announcements, id, null);
            var final = MergeAnnouncement.FromJson(JArray.Parse(data)).First();
            return final;
        }

        /// <summary>
        /// Gets all announcements
        /// </summary>
        /// <returns>The announcements</returns>
        public static async Task<List<MergeAnnouncement>> GetAnnouncements() {
            var data = await ContactServer(ApiDataType.Announcements, string.Empty, null);
            var final = MergeAnnouncement.FromJson(JArray.Parse(data));
            return final;
        }

        /// <summary>
        /// Gets a page
        /// </summary>
        /// <returns>The page</returns>
        /// <param name="id">The page's id</param>
        public static async Task<MergePage> GetPage(string id) {
            var data = await ContactServer(ApiDataType.Pages, id, null);
            var final = MergePage.FromJson(JArray.Parse(data)).First();
            return final;
        }

        /// <summary>
        /// Gets all pages
        /// </summary>
        /// <returns>The pages</returns>
        public static async Task<List<MergePage>> GetPages() {
            var data = await ContactServer(ApiDataType.Pages, string.Empty, null);
            var final = MergePage.FromJson(JArray.Parse(data));
            return final;
        }

        /// <summary>
        /// Gets all leaders
        /// </summary>
        /// <returns>The leaders</returns>
        public static async Task<List<MergeLeader>> GetLeaders() {
            var data = await ContactServer(ApiDataType.Leaders, string.Empty, null);
            var final = MergeLeader.FromJson(JArray.Parse(data));
            return final;
        }

        /// <summary>
        /// Gets a leader
        /// </summary>
        /// <returns>The leader</returns>
        /// <param name="id">The leader's id</param>
        public static async Task<MergeLeader> GetLeader(string id) {
            var data = await ContactServer(ApiDataType.Leaders, id, null);
            var final = MergeLeader.FromJson(JArray.Parse(data)).First();
            return final;
        }

        #endregion*/
    }
}
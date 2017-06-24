﻿#region LICENSE

// Project MergeApi:  AttendanceGroup.cs (in Solution MergeApi)
// Created by Greg Whatley on 03/20/2017 at 6:44 PM.
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
using System.Threading.Tasks;
using MergeApi.Client;
using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Enumerations.Converters;
using MergeApi.Framework.Interfaces;
using MergeApi.Tools;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Models.Core.Attendance {
    public sealed class AttendanceGroup : IIdentifiable {
        [JsonProperty("leaderNames")]
        public List<string> LeaderNames { get; set; }

        [JsonProperty("studentNames")]
        public List<string> StudentNames { get; set; }

        [JsonProperty("gradeLevel")]
        public GradeLevel GradeLevel { get; set; }

        [JsonProperty("gender")]
        public Gender Gender { get; set; }

        public string Summary {
            get {
                var formatted = LeaderNames.Format();
                return
                    $"{formatted}{(formatted.ToCharArray().Last() == 's' ? "'" : "'s")} {GradeLevelConverter.ToInt32(GradeLevel)}th grade {GenderConverter.ToHumanString(Gender, true)}";
            }
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        public async Task<List<AttendanceRecord>> GetRecordsAsync() {
            var records = await MergeDatabase.ListAsync<AttendanceRecord>();
            var filtered = records?.Where(r => r.GroupId == Id).ToList();
            return filtered == null || !filtered.Any() ? new List<AttendanceRecord>() : filtered;
        }
    }
}
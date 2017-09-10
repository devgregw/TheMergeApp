#region LICENSE

// Project MergeApi:  MergeGroupAttendanceRecord.cs (in Solution MergeApi)
// Created by Greg Whatley on 08/07/2017 at 8:04 AM.
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
using System.Globalization;
using System.Threading.Tasks;
using MergeApi.Client;
using MergeApi.Framework.Interfaces;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Models.Core.Attendance {
    public sealed class MergeGroupAttendanceRecord : IIdentifiable {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "date")]
        private string _date;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "studentCount")]
        public int StudentCount { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "image")]
        public string Image { get; set; }

        [JsonIgnore]
        public DateTime Date {
            get => DateTime.ParseExact(_date, "MMddyyyy", CultureInfo.CurrentCulture);
            set => _date = value.ToString("MMddyyyy");
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "mergeGroupId")]
        public string MergeGroupId { get; set; }

        [Obsolete("Do not use this.")]
        [JsonIgnore]
        public string Id {
            get { return $"{_date}{MergeGroupId}"; }
            set {
                // do nothing
            }
        }

        public async Task<MergeGroup> GetMergeGroupAsync() => await MergeDatabase.GetAsync<MergeGroup>(MergeGroupId);
    }
}
#region LICENSE

// Project MergeApi:  MergeGroup.cs (in Solution MergeApi)
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
using MergeApi.Converters;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Interfaces;
using MergeApi.Tools;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Models.Core {
    public sealed class MergeGroup : IIdentifiable, INamable {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("leaders")]
        public List<string> Leaders { get; set; }

        public string LeadersFormatted => Leaders == null ? "" : Leaders.Format();

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("coordinates")]
        public CoordinatePair Coordinates { get; set; }

        [JsonProperty("cover")]
        public string CoverImage { get; set; }

        [JsonProperty("contactMediums")]
        [JsonConverter(typeof(ClassableListJsonConverter<MediumBase>))]
        public List<MediumBase> ContactMediums { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
#region LICENSE

// Project MergeApi:  TabTip.cs (in Solution MergeApi)
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

using MergeApi.Converters;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Interfaces;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Models.Core.Tab {
    public sealed class TabTip : TargetableBase, IIdentifiable {
        [JsonProperty("persistent")] private string _persistent;

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("action")]
        [JsonConverter(typeof(ClassableJsonConverter))]
        public ActionBase Action { get; set; }

        [JsonIgnore]
        public bool Persistent {
            get => bool.Parse(_persistent ?? "false");
            set => _persistent = value.ToString();
        }

        [JsonProperty("tab")]
        public Framework.Enumerations.Tab Tab { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
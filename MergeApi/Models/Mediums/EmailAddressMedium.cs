#region LICENSE

// Project MergeApi:  EmailAddressMedium.cs (in Solution MergeApi)
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

using System.Text.RegularExpressions;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Tools;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Models.Mediums {
    public sealed class EmailAddressMedium : MediumBase {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "kind")]
        public EmailAddressKind Kind { get; set; }

        public static bool IsValidEmailAddress(string address) {
            if (string.IsNullOrWhiteSpace(address))
                return false;
            return Regex.IsMatch(address, @"^(([^<>()[\]\\.,;:\s@\""]+"
                                          + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                                          + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                                          + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                                          + @"[a-zA-Z]{2,}))$");
        }

        public static EmailAddressMedium Create(string who, string address, EmailAddressKind kind) {
            Utilities.AssertCondition(IsValidEmailAddress(address), address);
            return new EmailAddressMedium {
                Who = who,
                Address = address,
                Kind = kind
            };
        }

        public override string ToFriendlyString() {
            return $"{Who}: {Address} ({Kind})";
        }
    }
}
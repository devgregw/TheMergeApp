#region LICENSE

// Project MergeApi:  CallAction.cs (in Solution MergeApi)
// Created by Greg Whatley on 06/23/2017 at 10:42 AM.
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

using System.Threading.Tasks;
using MergeApi.Client;
using MergeApi.Converters;
using MergeApi.Framework.Abstractions;
using MergeApi.Models.Mediums;
using MergeApi.Tools;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Models.Actions {
    public sealed class CallAction : ActionBase {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "contactMedium")]
        [JsonConverter(typeof(ClassableJsonConverter))]
        public PhoneNumberMedium ContactMedium1 { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "phoneNumber")]
        public string PhoneNumber2 { get; set; }

        public static CallAction FromContactMedium(PhoneNumberMedium medium) {
            Utilities.AssertCondition(medium != null, medium);
            return new CallAction {
                ContactMedium1 = medium,
                ParamGroup = "1"
            };
        }

        public static CallAction FromPhoneNumber(string number) {
            Utilities.AssertCondition(PhoneNumberMedium.IsValidPhoneNumber(number), number);
            return new CallAction {
                PhoneNumber2 = number,
                ParamGroup = "2"
            };
        }

        public override void Invoke() => MergeDatabase.ActionInvocationReceiver.InvokeCallAction(this);

#pragma warning disable 1998
        public override async Task<ValidationResult> ValidateAsync() =>
#pragma warning restore 1998
            new ValidationResult(this);

        public override string ToFriendlyString() => $"Call: {(ParamGroup == "1" ? ContactMedium1.ToFriendlyString() : PhoneNumber2)}";
    }
}
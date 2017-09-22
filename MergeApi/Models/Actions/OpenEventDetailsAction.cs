#region LICENSE

// Project MergeApi:  OpenEventDetailsAction.cs (in Solution MergeApi)
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

using System;
using System.Threading.Tasks;
using Firebase.Database;
using MergeApi.Client;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Core;
using MergeApi.Tools;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Models.Actions {
    public sealed class OpenEventDetailsAction : ActionBase {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "eventId")]
        public string EventId1 { get; set; }

        public static OpenEventDetailsAction FromEventId(string eid) {
            Utilities.AssertCondition(!string.IsNullOrWhiteSpace(eid), eid);
            return new OpenEventDetailsAction {
                EventId1 = eid,
                ParamGroup = "1"
            };
        }

        public override void Invoke() => MergeDatabase.ActionInvocationReceiver.InvokeOpenEventDetailsActionAsync(this);

        public override async Task<ValidationResult> ValidateAsync() {
            try {
                var e = await MergeDatabase.GetAsync<MergeEvent>(EventId1);
                var v = await e.ValidateAsync();
                return v.ResultType != ValidationResultType.Success
                    ? new ValidationResult(this, ValidationResultType.EventValidationFailure, v)
                    : new ValidationResult(this);
            } catch (FirebaseException) {
                return new ValidationResult(this, ValidationResultType.EventNotFound, EventId1);
            } catch (Exception ex) {
                return new ValidationResult(this, ValidationResultType.Exception, ex);
            }
        }

        public override string ToFriendlyString() => $"Open event details: events/{EventId1}";
    }
}
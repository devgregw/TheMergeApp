#region LICENSE

// Project MergeApi:  OpenGroupDetailsAction.cs (in Solution MergeApi)
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
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Core;
using MergeApi.Tools;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Models.Actions {
    public sealed class OpenGroupDetailsAction : ActionBase {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "groupId")]
        public string GroupId1 { get; set; }

        public static OpenGroupDetailsAction FromGroupId(string id) => new OpenGroupDetailsAction {
            ParamGroup = "1",
            GroupId1 = id
        };

        public override async Task<ValidationResult> ValidateAsync() {
            try {
                //try to get the group..  if it doesn't exist, an exception will be thrown
                await MergeDatabase.GetAsync<MergeGroup>(GroupId1);
                //if we made it here, the group exists
                return new ValidationResult(this);
            } catch (FirebaseException) {
                return new ValidationResult(this, ValidationResultType.GroupNotFound, GroupId1);
            } catch (Exception ex) {
                return new ValidationResult(this, ValidationResultType.Exception, ex);
            }
        }

        public override string ToFriendlyString() => $"Open group details: groups/{GroupId1}";

        public override void Invoke() => MergeDatabase.ActionInvocationReceiver.InvokeOpenGroupDetailsActionAsync(this);
    }
}
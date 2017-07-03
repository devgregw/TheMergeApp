#region LICENSE

// Project MergeApi:  ShowContactInfoAction.cs (in Solution MergeApi)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using MergeApi.Client;
using MergeApi.Converters;
using MergeApi.Exceptions;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Core;
using MergeApi.Tools;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Models.Actions {
    public sealed class ShowContactInfoAction : ActionBase {
        [JsonProperty("groupId")]
        public string GroupId1 { get; set; }

        [JsonProperty("name")]
        public string Name2 { get; set; }

        [JsonProperty("contactMediums")]
        [JsonConverter(typeof(ClassableListJsonConverter<MediumBase>))]
        public List<MediumBase> ContactMediums2 { get; set; }

        public static ShowContactInfoAction FromGroupId(string groupId) {
            Utilities.AssertCondition(!string.IsNullOrWhiteSpace(groupId), groupId);
            return new ShowContactInfoAction {
                GroupId1 = groupId,
                ParamGroup = "1"
            };
        }

        public static ShowContactInfoAction FromContactMediums(string name, IEnumerable<MediumBase> mediums) {
            Utilities.AssertCondition(!string.IsNullOrWhiteSpace(name), name);
            Utilities.AssertCondition(mediums != null, mediums);
            return new ShowContactInfoAction {
                Name2 = name,
                ContactMediums2 = mediums.ToList(),
                ParamGroup = "2"
            };
        }

        public override void Invoke() {
            MergeDatabase.ActionInvocationReceiver.InvokeShowContactInfoActionAsync(this);
        }

        public override async Task<ValidationResult> ValidateAsync() {
            switch (ParamGroup) {
                case "1": {
                    try {
                        //try to get the group..  if it doesn't exist, an exception will be thrown
                        await MergeDatabase.GetAsync<MergeGroup>(GroupId1);
                        //if we made it here, it exists
                        return new ValidationResult(this);
                    } catch (FirebaseException) {
                        return new ValidationResult(this, ValidationResultType.GroupNotFound, GroupId1);
                    } catch (Exception ex) {
                        return new ValidationResult(this, ValidationResultType.Exception, ex);
                    }
                }
                case "2":
                    return new ValidationResult(this);
                case "3": {
                    return new ValidationResult(this);
                    /*try {
                        await MergeDatabase.GetLeaderAsync(LeaderId3);
                        return new ValidationResult(this);
                    } catch (Exception ex) {
                        if (ex is ApiResponseException) {
                            var api = ex.Cast<Exception, ApiResponseException>();
                            if (api.Response.ResponseCode == 404)
                                return new ValidationResult(this, ValidationResultType.LeaderNotFound, LeaderId3);
                        }
                        return new ValidationResult(this, ValidationResultType.Exception, ex);
                    }*/
                }
            }
            return new ValidationResult(this, ValidationResultType.Exception,
                new InvalidParamGroupException(GetType(), ParamGroup));
        }

        public override string ToFriendlyString() {
            switch (ParamGroup) {
                case "1":
                    return $"Show contact info: groups/{GroupId1}";
                case "2":
                    return
                        $"Show contact info: {Name2} ({ContactMediums2.Count} medium{(ContactMediums2.Count > 1 ? "s" : "")})";
                default:
                    return "Show contact info: ERROR";
            }
        }
    }
}
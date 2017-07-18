#region LICENSE

// Project MergeApi:  GetDirectionsAction.cs (in Solution MergeApi)
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
using MergeApi.Exceptions;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Core;
using MergeApi.Tools;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Models.Actions {
    public sealed class GetDirectionsAction : ActionBase {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "eventId")]
        public string EventId1 { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "groupId")]
        public string GroupId2 { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "address")]
        public string Address3 { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "coordinates")]
        public CoordinatePair Coordinates4 { get; set; }

        public static GetDirectionsAction FromEventId(string eventId) {
            Utilities.AssertCondition(!string.IsNullOrWhiteSpace(eventId), eventId);
            return new GetDirectionsAction {
                EventId1 = eventId,
                ParamGroup = "1"
            };
        }

        public static GetDirectionsAction FromGroupId(string groupId) {
            Utilities.AssertCondition(!string.IsNullOrWhiteSpace(groupId), groupId);
            return new GetDirectionsAction {
                GroupId2 = groupId,
                ParamGroup = "2"
            };
        }

        public static GetDirectionsAction FromAddress(string address) {
            Utilities.AssertCondition(!string.IsNullOrWhiteSpace(address), address);
            return new GetDirectionsAction {
                Address3 = address,
                ParamGroup = "3"
            };
        }

        public static GetDirectionsAction FromCoordinates(CoordinatePair coordinates) {
            Utilities.AssertCondition(coordinates != null, coordinates);
            return new GetDirectionsAction {
                Coordinates4 = coordinates,
                ParamGroup = "4"
            };
        }

        public override void Invoke() {
            MergeDatabase.ActionInvocationReceiver.InvokeGetDirectionsActionAsync(this);
        }

        public override async Task<ValidationResult> ValidateAsync() {
            switch (ParamGroup) {
                case "1": {
                    try {
                        var e = await MergeDatabase.GetAsync<MergeEvent>(EventId1);
                        if (string.IsNullOrWhiteSpace(e.Address))
                            return new ValidationResult(this, ValidationResultType.NoAddress, e);
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
                case "2": {
                    try {
                        //try to get the group..  if it doesn't exist, an exception will be thrown
                        await MergeDatabase.GetAsync<MergeGroup>(GroupId2);
                        //if we made it here, the group exists
                        return new ValidationResult(this);
                    } catch (FirebaseException) {
                        return new ValidationResult(this, ValidationResultType.GroupNotFound, GroupId2);
                    } catch (Exception ex) {
                        return new ValidationResult(this, ValidationResultType.Exception, ex);
                    }
                }
                case "3":
                case "4": {
                    return new ValidationResult(this);
                }
            }
            return new ValidationResult(this, ValidationResultType.Exception,
                new InvalidParamGroupException(GetType(), ParamGroup));
        }

        public override string ToFriendlyString() {
            switch (ParamGroup) {
                case "1":
                    return $"Get directions: events/{EventId1}";
                case "2":
                    return $"Get directions: groups/{GroupId2}";
                case "3":
                    return $"Get directions: {Address3}";
                case "4":
                    return $"Get directions: {Coordinates4}";
                default:
                    return "Get directions: ERROR";
            }
        }
    }
}
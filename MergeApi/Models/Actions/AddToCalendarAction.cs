#region LICENSE

// Project MergeApi:  AddToCalendarAction.cs (in Solution MergeApi)
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
using System.Globalization;
using System.Linq;
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
    public sealed class AddToCalendarAction : ActionBase {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "eventId")]
        public string EventId1 { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "title")]
        public string Title2 { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "location")]
        public string Location2 { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "start")]
        public DateTime? StartDate2 { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "end")]
        public DateTime? EndDate2 { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "recurrenceRule")]
        public RecurrenceRule RecurrenceRule2 { get; set; }

        public static AddToCalendarAction FromEventId(string eventId) {
            Utilities.AssertCondition(!string.IsNullOrWhiteSpace(eventId), eventId);
            return new AddToCalendarAction {
                EventId1 = eventId,
                ParamGroup = "1"
            };
        }

        public static AddToCalendarAction FromEventInfo(string title, string loc, DateTime start, DateTime end,
            RecurrenceRule rule) {
            Utilities.AssertCondition(s => !string.IsNullOrWhiteSpace(s), title, loc);
            Utilities.AssertCondition(end > start, end);
            return new AddToCalendarAction {
                Title2 = title,
                Location2 = loc,
                StartDate2 = start,
                EndDate2 = end,
                RecurrenceRule2 = rule,
                ParamGroup = "2"
            };
        }

        public override void Invoke() => MergeDatabase.ActionInvocationReceiver.InvokeAddToCalendarActionAsync(this);

        public override async Task<ValidationResult> ValidateAsync() {
            switch (ParamGroup) {
                case "1": {
                    try {
                        var e = await MergeDatabase.GetAsync<MergeEvent>(EventId1);
                        return await e.ValidateAsync();
                    } catch (FirebaseException) {
                        return new ValidationResult(this, ValidationResultType.EventNotFound, EventId1);
                    } catch (Exception ex) {
                        return new ValidationResult(this, ValidationResultType.Exception, ex);
                    }
                }
                case "2": {
                    var days = DateTime.Now.Subtract(RecurrenceRule2 == null
                        ? EndDate2.Value
                        : RecurrenceRule.GetAllOccurrences(StartDate2.Value, RecurrenceRule2).Last()).TotalDays;
                    return days >= 1d
                        ? new ValidationResult(this, ValidationResultType.OutdatedAction, this)
                        : new ValidationResult(this);
                }
            }
            return new ValidationResult(this, ValidationResultType.Exception,
                new InvalidParamGroupException(GetType(), ParamGroup));
        }

        public override string ToFriendlyString() => "Add to calendar: " +
                   (ParamGroup == "1"
                       ? "events/" + EventId1
                       : $"\"{Title2}\" at {Location2} from {StartDate2.Value.ToString("M/dd/yyyy h:mm tt", CultureInfo.CurrentUICulture)} to {EndDate2.Value.ToString("M/dd/yyyy h:mm tt", CultureInfo.CurrentUICulture)}"
                   );
    }
}
#region LICENSE

// Project MergeApi:  MergeEvent.cs (in Solution MergeApi)
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
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Tools;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Models.Core {
    public sealed class MergeEvent : ModelBase {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "endDate")]
        public DateTime? EndDate { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "recurrenceRule")]
        public RecurrenceRule RecurrenceRule { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "location")]
        public string Location { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "uri")]
        public string RegistrationUrl { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            PropertyName = "registrationClosingDate")]
        public DateTime? RegistrationClosingDate { get; set; }

        [JsonIgnore]
        public bool HasRegistration => !string.IsNullOrWhiteSpace(RegistrationUrl);

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "price")]
        public double? Price { get; set; }

        [JsonIgnore]
        public TimeSpan Duration => EndDate.GetValueOrDefault(DateTime.MaxValue) -
                                    StartDate.GetValueOrDefault(DateTime.MinValue);

        [JsonIgnore]
        public DateTime NextStartDate => RecurrenceRule == null
            ? StartDate.GetValueOrDefault(DateTime.MinValue)
            : RecurrenceRule.GetNextOccurrence(StartDate.GetValueOrDefault(DateTime.MinValue), RecurrenceRule)
                .GetValueOrDefault(DateTime.MinValue);

        [JsonIgnore]
        public DateTime NextEndDate => NextStartDate + Duration;

        public override async Task<ValidationResult> ValidateAsync() => DateTime.Now > NextEndDate
                ? new ValidationResult(this, ValidationResultType.OutdatedEvent, this)
                : new ValidationResult(this);
    }
}
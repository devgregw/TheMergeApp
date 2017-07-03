#region LICENSE

// Project MergeApi:  MergeEvent.cs (in Solution MergeApi)
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
using System.Threading.Tasks;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Tools;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Models.Core {
    public sealed class MergeEvent : ModelBase {
        [JsonProperty("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonProperty("recurrenceRule")]
        public RecurrenceRule RecurrenceRule { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("uri")]
        public string RegistrationUrl { get; set; }

        [JsonProperty("registrationClosingDate")]
        public DateTime? RegistrationClosingDate { get; set; }

        [JsonIgnore]
        public bool HasRegistration => !string.IsNullOrWhiteSpace(RegistrationUrl);

        [JsonProperty("price")]
        public double? Price { get; set; }

        public override async Task<ValidationResult> ValidateAsync() {
            // ReSharper disable once PossibleInvalidOperationException
            var days = DateTime.Now.Subtract(EndDate.Value).TotalDays;
            return days >= 1d
                ? new ValidationResult(this, ValidationResultType.OutdatedEvent, this)
                : new ValidationResult(this);
        }
    }
}
#region LICENSE

// Project MergeApi:  MergePage.cs (in Solution MergeApi)
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

using System.Collections.Generic;
using System.Threading.Tasks;
using MergeApi.Converters;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Actions;
using MergeApi.Tools;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Models.Core {
    public sealed class MergePage : ModelBase {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include, PropertyName = "hidden")]
        private string _hidden;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include, PropertyName = "leadersOnly")]
        private string _leadersOnly;

        [JsonIgnore]
        public bool Hidden {
            get => bool.TryParse(_hidden, out var result) && result;
            set => _hidden = value.ToString();
        }

        [JsonIgnore]
        public bool LeadersOnly {
            get => bool.TryParse(_leadersOnly, out var result) && result;
            set => _leadersOnly = value.ToString();
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include, PropertyName = "content")]
        [JsonConverter(typeof(ClassableListJsonConverter<ElementBase>))] private List<ElementBase> _content;

        [JsonIgnore]
        public List<ElementBase> Content {
            get => _content ?? new List<ElementBase>();
            set => _content = value;
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include, PropertyName = "buttonLabel")]
        public string ButtonLabel { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include, PropertyName = "importance")]
        public Importance Importance { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include, PropertyName = "buttonAction")]
        [JsonConverter(typeof(ClassableJsonConverter))]
        public ActionBase ButtonAction { get; set; }

        public override async Task<ValidationResult> ValidateAsync() {
            var invalid = new List<ElementBase>();
            foreach (var e in Content) {
                var v = await e.ValidateAsync();
                if (v.ResultType != ValidationResultType.Success)
                    invalid.Add(e);
            }
            if (invalid.Count > 0)
                return new ValidationResult(this, ValidationResultType.PageContentValidationFailure, invalid);
            if (ButtonAction != null) {
                if (ButtonAction is OpenPageAction && ((OpenPageAction) ButtonAction).PageId1 == Id)
                    return new ValidationResult(this);
                var v = await ButtonAction.ValidateAsync();
                if (v.ResultType != ValidationResultType.Success)
                    return new ValidationResult(this, ValidationResultType.PageActionValidationFailure, v);
            }
            return new ValidationResult(this);
        }
    }
}
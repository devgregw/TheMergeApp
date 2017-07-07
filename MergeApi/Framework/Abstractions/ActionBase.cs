#region LICENSE

// Project MergeApi:  ActionBase.cs (in Solution MergeApi)
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
using MergeApi.Converters;
using MergeApi.Exceptions;
using MergeApi.Framework.Interfaces;
using MergeApi.Tools;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Framework.Abstractions {
    public abstract class ActionBase : IClassable, IValidatable {
        private string _class;

        private string _paramGroup;
        public ActionBase() { }

        protected ActionBase(string pgp) {
            ParamGroup = pgp;
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "paramGroup")]
        public string ParamGroup {
            get => string.IsNullOrWhiteSpace(_paramGroup) ? "-1" : _paramGroup;
            set => _paramGroup = value;
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "class")]
        public string Class {
            get => GetType().FullName;
            set {
                if (Type.GetType(value) == null)
                    throw new UnknownClassException(value, "action");
                _class = value;
            }
        }

        public abstract Task<ValidationResult> ValidateAsync();

        public abstract string ToFriendlyString();

        public abstract void Invoke();

        private class ActionWrapper {
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "code")] [JsonConverter(typeof(ClassableJsonConverter))] private ActionBase _action;

            public static implicit operator ActionBase(ActionWrapper w) => w._action;
        }

        public static ActionBase FromJson(string json) => JsonConvert.DeserializeObject<ActionWrapper>(
            @"{""code"":" + json + "}");
    }
}
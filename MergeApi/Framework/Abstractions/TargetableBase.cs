#region LICENSE

// Project MergeApi:  TargetableBase.cs (in Solution MergeApi)
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
using System.Linq;
using MergeApi.Framework.Enumerations;
using Newtonsoft.Json;

#endregion

namespace MergeApi.Framework.Abstractions {
    public abstract class TargetableBase {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "grades")]
        public List<GradeLevel> GradeLevels { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "genders")]
        public List<Gender> Genders { get; set; }

        public bool CheckTargeting(IEnumerable<GradeLevel> setGradeLevels, IEnumerable<Gender> setGenders) => CheckTargeting(setGradeLevels.ToList(), setGenders.ToList());

        public bool CheckTargeting(List<GradeLevel> setGradeLevels, List<Gender> setGenders) {
            var grades = !setGradeLevels.Any() ? EnumConsts.AllGradeLevels : setGradeLevels;
            var genders = !setGenders.Any() ? EnumConsts.AllGenders : setGenders;
            return grades.Any(g => GradeLevels.Contains(g)) && genders.Any(g => Genders.Contains(g));
        }
    }
}
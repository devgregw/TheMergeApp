#region LICENSE

// Project MergeApi:  GenderConverter.cs (in Solution MergeApi)
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
using MergeApi.Tools;

#endregion

namespace MergeApi.Framework.Enumerations.Converters {
    public static class GenderConverter {
        public static Gender FromString(string gender) {
            Utilities.AssertCondition(s => !string.IsNullOrWhiteSpace(s), gender);
            return gender.ToEnum<Gender>(true);
        }

        public static Gender FromHumanString(string gender) {
            Utilities.AssertCondition(s => !string.IsNullOrWhiteSpace(s), gender);
            var g = gender.ToLower();
            if (g.Contains("guy") && g.Contains("girl"))
                throw new ArgumentException("The given string cannot be converted to a Gender.", nameof(gender));
            if (g.Contains("guy"))
                return Gender.Male;
            if (g.Contains("girl"))
                return Gender.Female;
            throw new ArgumentException("The given string cannot be converted to a Gender.", nameof(gender));
        }

        public static string ToHumanString(Gender gender, bool plural) {
            return $"{(gender == Gender.Male ? "guy" : "girl")}{(plural ? "s" : "")}";
        }
    }
}
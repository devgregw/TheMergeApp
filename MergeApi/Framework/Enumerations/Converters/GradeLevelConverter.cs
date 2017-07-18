#region LICENSE

// Project MergeApi:  GradeLevelConverter.cs (in Solution MergeApi)
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

using MergeApi.Tools;

#endregion

namespace MergeApi.Framework.Enumerations.Converters {
    public static class GradeLevelConverter {
        public static int ToInt32(GradeLevel grade) {
            return (int) grade;
        }

        public static GradeLevel FromString(string grade) {
            Utilities.AssertCondition(s => !string.IsNullOrWhiteSpace(s), grade);
            return grade.ToEnum<GradeLevel>(true);
        }

        public static GradeLevel FromInt32(int grade) {
            Utilities.AssertCondition(i => i >= 7 && i <= 12, grade);
            return (GradeLevel) grade;
        }
    }
}
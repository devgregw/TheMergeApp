#region LICENSE

// Project Merge.Android:  Utilities.cs (in Solution Merge.Android)
// Created by Greg Whatley on 06/30/2017 at 9:26 AM.
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
using System.Text;
using Android.Widget;
using Com.Nostra13.Universalimageloader.Core;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Tools;

#endregion

namespace Merge.Android.Helpers {
    public static class Utilities {
        public static void LoadImageForDisplay(string url, ImageView view) {
            ImageLoader.Instance.DisplayImage(url, view);
        }

        public static string GetTargetingString<T>(T obj) where T : TargetableBase {
            var genders = (from g in obj.Genders select g.ToString().ToLower() == "male" ? "guys" : "girls").Format();
            var sets = new List<List<GradeLevel>>();
            var inSet = false;
            var currentSet = new List<GradeLevel>();
            for (var i = 0; i < obj.GradeLevels.Count; i++)
                if (!inSet) {
                    inSet = true;
                    currentSet = new List<GradeLevel> {obj.GradeLevels[i]};
                    if (i + 1 >= obj.GradeLevels.Count) {
                        sets.Add(currentSet);
                        break;
                    }
                    if ((int) obj.GradeLevels[i] + 1 == (int) obj.GradeLevels[i + 1]) continue;
                    sets.Add(currentSet);
                    inSet = false;
                } else {
                    if (i + 1 >= obj.GradeLevels.Count) {
                        currentSet.Add(obj.GradeLevels[i]);
                        sets.Add(currentSet);
                        break;
                    }
                    if ((int) obj.GradeLevels[i] + 1 == (int) obj.GradeLevels[i + 1]) {
                        currentSet.Add(obj.GradeLevels[i]);
                    } else {
                        currentSet.Add(obj.GradeLevels[i]);
                        sets.Add(currentSet);
                        inSet = false;
                    }
                }
            var grades = "";
            foreach (var set in sets)
                if (set.Count == 1)
                    grades += (int) set[0] + "th,";
                else
                    grades += $"{(int) set.First()}th thru {(int) set.Last()}th,";
            try {
                if (grades.Contains(",")) {
                    var s = grades.Remove(grades.LastIndexOf(","));
                    var builder = new StringBuilder(s);
                    builder.Replace(",", "{and}", s.LastIndexOf(",") - 1, 2);
                    builder.Append(" grade");
                    grades = builder.ToString().Replace(",", ", ").Replace("{and}", ", and ");
                }
            } catch {
                grades = grades.Remove(grades.LastIndexOf(",")) + " grade";
            }
            return $"For {genders} in {grades}";
        }
    }
}
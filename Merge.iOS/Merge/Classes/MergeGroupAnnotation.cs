#region LICENSE

// Project Merge.iOS:  MergeGroupAnnotation.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/15/2017 at 10:46 AM.
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
using CoreLocation;
using MapKit;
using MergeApi.Models.Core;

#endregion

namespace Merge.Classes {
    public class MergeGroupAnnotation : MKAnnotation {
        public MergeGroupAnnotation(MergeGroup g) : this(g.Name, "Lead by " + g.LeadersFormatted,
            new CLLocationCoordinate2D(Convert.ToDouble(g.Coordinates.Latitude),
                Convert.ToDouble(g.Coordinates.Longitude))) {
            Group = g;
        }

        private MergeGroupAnnotation(string t, string st, CLLocationCoordinate2D pos) {
            Title = t;
            Subtitle = st;
            Coordinate = pos;
        }

        public MergeGroup Group { get; }

        public override string Title { get; }

        public override string Subtitle { get; }

        public override CLLocationCoordinate2D Coordinate { get; }
    }
}
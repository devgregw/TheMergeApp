#region LICENSE

// Project MergeApi:  CoordinatePair.cs (in Solution MergeApi)
// Created by Greg Whatley on 06/23/2017 at 10:43 AM.
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
using Newtonsoft.Json;

#endregion

namespace MergeApi.Tools {
    public sealed class CoordinatePair {
        public CoordinatePair() { }

        public CoordinatePair(decimal lat, decimal lng) {
            Utilities.AssertCondition(-90 <= lat && lat <= 90, lat);
            Utilities.AssertCondition(-180 <= lng && lng <= 180, lng);
            Latitude = lat;
            Longitude = lng;
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "latitude")]
        public decimal Latitude { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "longitude")]
        public decimal Longitude { get; set; }

        public static double GetDistanceBetween(CoordinatePair one, CoordinatePair two) {
            return Math.Sqrt(Math.Pow(Convert.ToDouble(one.Latitude - two.Latitude), 2) +
                             Math.Pow(Convert.ToDouble(one.Longitude - two.Longitude), 2));
        }

        public override string ToString() {
            return $"({Latitude}, {Longitude})";
        }
    }
}
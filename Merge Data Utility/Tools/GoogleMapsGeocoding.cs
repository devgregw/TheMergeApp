#region LICENSE

// Project Merge Data Utility:  GoogleMapsGeocoding.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 03/20/2017 at 6:42 PM.
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
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using MergeApi.Tools;

#endregion

namespace Merge_Data_Utility.Tools {
    public static class GoogleMapsGeocoding {
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public static async Task<Tuple<string, CoordinatePair>> GetCoordinates(string address) {
            var url =
                $"https://maps.googleapis.com/maps/api/geocode/xml?address={WebUtility.UrlEncode(address)}&key=AIzaSyDavAj3Zw5hpQ3yBRHpNfrjJbX3z3ChBwg";
            using (var client = new WebClient()) {
                var doc =
                    XDocument.Parse(await client.DownloadStringTaskAsync(url))
                        .Root;
                if (doc.Element("status").Value == "ZERO_RESULTS")
                    return null;
                var result = doc.Element("result");
                var loc = result.Element("geometry").Element("location");
                var coords = new CoordinatePair(Convert.ToDecimal(loc.Element("lat").Value),
                    Convert.ToDecimal(loc.Element("lng").Value));
                return new Tuple<string, CoordinatePair>(result.Element("formatted_address").Value, coords);
            }
        }
    }
}
#region LICENSE

// Project Merge.iOS:  GroupsMapPage.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/07/2017 at 10:27 AM.
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
using System.Linq;
using Foundation;
using MapKit;
using Merge.Classes.UI.Controls;
using MergeApi;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using MergeApi.Tools;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Collections.Generic;
using CoreLocation;

#endregion

namespace Merge.Classes.UI {
    public class GroupsMapPage : ContentPage {
        public const double EarthRadiusInKilometers = 6367.0;

        public  double ToRadian(double val) { return val * (Math.PI / 180); }
        public double DiffRadian(double val1, double val2) { return ToRadian(val2) - ToRadian(val1); }

        public double CalcDistance(double lat1, double lng1, double lat2, double lng2) {
            double radius = EarthRadiusInKilometers;
            return radius * 2 * Math.Asin(Math.Min(1, Math.Sqrt((Math.Pow(Math.Sin((DiffRadian(lat1, lat2)) / 2.0), 2.0) + Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) * Math.Pow(Math.Sin((DiffRadian(lng1, lng2)) / 2.0), 2.0)))));
        }

        public GroupsMapPage() {
            Title = "Merge Groups";
            new Action(async () => {
                new NSObject().InvokeOnMainThread(() => ((App)Application.Current).ShowLoader("Loading..."));
                var groups = (await MergeDatabase.ListAsync<MergeGroup>()).ToList();
                new NSObject().InvokeOnMainThread(((App)Application.Current).HideLoader);
                var latitudes = groups.Select(g => Convert.ToDouble(g.Coordinates.Latitude)).ToList();
                var longitudes = groups.Select(g => Convert.ToDouble(g.Coordinates.Longitude)).ToList();

                double lowestLat = latitudes.Min();
                double highestLat = latitudes.Max();
                double lowestLong = longitudes.Min();
                double highestLong = longitudes.Max();
                double finalLat = (lowestLat + highestLat) / 2;
                double finalLong = (lowestLong + highestLong) / 2;
                double distance = CalcDistance(lowestLat, lowestLong, highestLat, highestLong);

                var map = new Map(MapSpan.FromCenterAndRadius(new Position(finalLat, finalLong), new Distance(distance * 500)))
                {
                    IsShowingUser = true,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };
                groups.ForEach(g => {
                    var pin = new Pin
                    {
                        Position = new Position(Convert.ToDouble(g.Coordinates.Latitude), Convert.ToDouble(g.Coordinates.Longitude)),
                        Address = g.Address,
                        Label = g.Name,
                        Type = PinType.Place
                    };
                    pin.Clicked += (s, e) => OpenGroupDetailsAction.FromGroupId(g.Id).Invoke();
                    map.Pins.Add(pin);
                });
                Content = map;
                /*Content = new MapView(groups.Select(g => new MergeGroupAnnotation(g)).Cast<MKAnnotation>().ToList(),
                    a => OpenGroupDetailsAction.FromGroupId(((MergeGroupAnnotation) a).Group.Id).Invoke(), true, true) {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };*/
            }).Invoke();
        }
    }
}
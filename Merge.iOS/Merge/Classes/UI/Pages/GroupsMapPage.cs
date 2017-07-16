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
using System.Collections.Generic;
using System.Linq;
using CoreLocation;
using Foundation;
using MapKit;
using Merge.Classes.Helpers;
using Merge.Classes.UI;
using Merge.Classes.UI.Controls;
using MergeApi.Client;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

#endregion

namespace Merge.Classes.UI {
    public class GroupsMapPage : ContentPage {
        public GroupsMapPage() {
            Title = "Merge Groups";
            new Action(async () => {
                new NSObject().InvokeOnMainThread(() => ((App)Application.Current).ShowLoader("Loading..."));
                var groups = (await MergeDatabase.ListAsync<MergeGroup>()).ToList();
                new NSObject().InvokeOnMainThread(((App)Application.Current).HideLoader);
                Content = new MapView(groups.Select(g => new MergeGroupAnnotation(g)).Cast<MKAnnotation>().ToList(), a => OpenGroupDetailsAction.FromGroupId(((MergeGroupAnnotation)a).Group.Id).Invoke(), true, true) {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };
            }).Invoke();
        }
    }
}
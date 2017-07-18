#region LICENSE

// Project Merge.iOS:  TabPageDelegates.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/08/2017 at 5:57 PM.
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
using Merge.Classes.Helpers;
using Merge.Classes.UI.Controls;
using Merge.Classes.UI.Pages;
using Merge.iOS;
using MergeApi.Models.Core;
using MergeApi.Models.Core.Tab;
using MergeApi.Tools;
using Xamarin.Forms;

#endregion

namespace Merge.Classes {
    public sealed class TabPageDelegates : IGenericTabPageMetaDelegate, IGenericTabPageDelegate<MergePage, int>,
        IGenericTabPageDelegate<MergeEvent, DateTime>, IGenericTabPageDelegate<MergeGroup, double> {
        private static TabPageDelegates _instance;

        private IEnumerable<TabHeader> _headers;

        private IEnumerable<TabTip> _tips;

        private TabPageDelegates() { }

        public static TabPageDelegates Instance => _instance ?? (_instance = new TabPageDelegates());

        public void Nullify() {
            _tips = null;
            _headers = null;
            DataCache.Pages = null;
            DataCache.Events = null;
            DataCache.Groups = null;
        }

        #region Meta

        public IEnumerable<TabTip> GetTips() => _tips;

        public void SetTips(IEnumerable<TabTip> value) => _tips = value;

        public bool TipDoesPassThroughFilter(TabTip tip) => tip.CheckTargeting(PreferenceHelper.GradeLevels,
                                                                PreferenceHelper.Genders) && !PreferenceHelper
                                                                .DismissedTips.Contains(tip.Id);

        public IEnumerable<TabHeader> GetHeaders() => _headers;

        public void SetHeaders(IEnumerable<TabHeader> value) => _headers = value;

        #endregion

        #region Pages

        IEnumerable<MergePage> IGenericTabPageDelegate<MergePage, int>.GetItems() => DataCache.Pages;

        public void SetItems(IEnumerable<MergePage> value) => DataCache.Pages = value;

        public int TransformForSorting(MergePage input) => (int) input.Importance;

        public View TransformIntoView(MergePage input) => new DataView(input);

        public bool DoesPassThroughFilter(MergePage input) => !input.LeadersOnly &&
                                                              input.CheckTargeting(PreferenceHelper.GradeLevels,
                                                                  PreferenceHelper.Genders);

        int IGenericTabPageDelegate<MergePage, int>.GetTab() => 0;

        #endregion

        #region Groups

        IEnumerable<MergeGroup> IGenericTabPageDelegate<MergeGroup, double>.GetItems() => DataCache.Groups;

        public void SetItems(IEnumerable<MergeGroup> value) => DataCache.Groups = value;

        public double TransformForSorting(MergeGroup input) {
            if (CLLocationManager.LocationServicesEnabled)
                AppDelegate.LocationManager.StartMonitoringSignificantLocationChanges();
            if (MergeLocationDelegate.Location == null) return 0;
            var c = MergeLocationDelegate.Location.Coordinate;
            AppDelegate.LocationManager.StopMonitoringSignificantLocationChanges();
            return Math.Sqrt(Math.Pow(c.Latitude - Convert.ToDouble(input.Coordinates.Latitude), 2) +
                             Math.Pow(c.Longitude - Convert.ToDouble(input.Coordinates.Longitude), 2));
        }

        public View TransformIntoView(MergeGroup input) => new DataView(input);

        public bool DoesPassThroughFilter(MergeGroup input) => true;

        int IGenericTabPageDelegate<MergeGroup, double>.GetTab() => 2;

        #endregion

        #region  Events

        IEnumerable<MergeEvent> IGenericTabPageDelegate<MergeEvent, DateTime>.GetItems() => DataCache.Events;

        public void SetItems(IEnumerable<MergeEvent> value) => DataCache.Events = value;

        public DateTime TransformForSorting(MergeEvent input) => input.RecurrenceRule == null
            ? input.StartDate.Value
            : RecurrenceRule.GetNextOccurrence(input.StartDate.Value, input.RecurrenceRule)
                .GetValueOrDefault(DateTime.MaxValue);

        public View TransformIntoView(MergeEvent input) => new DataView(input);

        public bool DoesPassThroughFilter(MergeEvent input) => input.CheckTargeting(PreferenceHelper.GradeLevels,
            PreferenceHelper.Genders);

        int IGenericTabPageDelegate<MergeEvent, DateTime>.GetTab() => 1;

        #endregion
    }
}
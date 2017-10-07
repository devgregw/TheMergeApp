#region LICENSE

// Project Merge.iOS:  LeaderResourcesPage.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/14/2017 at 9:17 AM.
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
using Merge.Classes.UI.Controls;
using Merge.iOS.Helpers;
using MergeApi.Models.Core;
using MergeApi.Models.Core.Tab;
using MergeApi.Tools;
using Xamarin.Forms;

#endregion

namespace Merge.Classes.UI.Pages.LeadersOnly {
    public class LeaderResourcesPage : GenericTabPage<MergePage, int> {
        public LeaderResourcesPage() : base("Leader Resources", "", LeaderResoucesPageDelegate.Instance,
            LeaderResoucesPageDelegate.Instance) {
            ToolbarItems.Add(new ToolbarItem("Attendance Manager", Images.MergeGroups,
                () => Navigation.PushAsync(AttendanceListPage.CreateMain(Navigation))));
        }

        public sealed class LeaderResoucesPageDelegate : IGenericTabPageDelegate<MergePage, int>,
            IGenericTabPageMetaDelegate {
            private static LeaderResoucesPageDelegate _instance;

            private IEnumerable<MergePage> _pages;

            public static LeaderResoucesPageDelegate Instance => _instance ??
                                                                 (_instance = new LeaderResoucesPageDelegate());

            public IEnumerable<MergePage> GetItems() => _pages;

            public void SetItems(IEnumerable<MergePage> value) => _pages = value;

            public int TransformForSorting(MergePage input) => (int) input.Importance;

            public View TransformIntoView(MergePage input, ValidationResult r) => new DataView(input, r);

            public bool DoesPassThroughFilter(MergePage input) => input.LeadersOnly && Helpers.Utilities.IfRelease(!input.Hidden, true);

            public int GetTab() => 0;

            public IEnumerable<TabTip> GetTips() => new List<TabTip>();

            public void SetTips(IEnumerable<TabTip> value) { }

            public bool TipDoesPassThroughFilter(TabTip tip) => true;

            public IEnumerable<TabHeader> GetHeaders() => new List<TabHeader>();

            public void SetHeaders(IEnumerable<TabHeader> value) { }
        }
    }
}
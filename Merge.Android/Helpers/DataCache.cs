using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MergeApi.Models.Core;

namespace Merge.Android.Helpers {
    public static class DataCache {
        public static IEnumerable<MergePage> Pages;

        public static IEnumerable<MergeEvent> Events;

        public static IEnumerable<MergeGroup> Groups;
    }
}
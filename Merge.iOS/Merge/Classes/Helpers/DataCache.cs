using System;
using System.Collections.Generic;
using System.Text;
using MergeApi.Models.Core;

namespace Merge.Classes.Helpers
{
    public static class DataCache {
        public static IEnumerable<MergePage> Pages;

        public static IEnumerable<MergeEvent> Events;

        public static IEnumerable<MergeGroup> Groups;
    }
}

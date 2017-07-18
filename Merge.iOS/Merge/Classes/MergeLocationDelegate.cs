using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLocation;

namespace Merge.Classes
{
    public sealed class MergeLocationDelegate : CLLocationManagerDelegate
    {
        public static CLLocation Location { get; set; }

        public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations) {
            Location = locations.Last();
        }
    }
}

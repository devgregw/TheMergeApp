using System;
using System.Collections.Generic;
using System.Text;
using CoreLocation;
using MapKit;
using MergeApi.Models.Core;

namespace Merge.Classes
{
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

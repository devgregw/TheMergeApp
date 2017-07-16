using System;
using System.Collections.Generic;
using System.Text;
using CoreLocation;
using MapKit;

namespace Merge.Classes
{
    public sealed class MapAnnotation : MKAnnotation
    {
        public MapAnnotation(CLLocationCoordinate2D coordinates) {
            Coordinate = coordinates;
        }

        public MapAnnotation(string title, string subtitle, CLLocationCoordinate2D coordinates) : this(coordinates) {
            Title = title;
            Subtitle = subtitle;
        }

        public override string Title { get; }

        public override string Subtitle { get; }

        public override CLLocationCoordinate2D Coordinate { get; }
    }
}

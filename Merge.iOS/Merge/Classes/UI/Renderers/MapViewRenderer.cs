using System;
using System.Collections.Generic;
using System.Text;
using CoreLocation;
using MapKit;
using Merge.Classes.Helpers;
using Merge.Classes.UI.Controls;
using Merge.Classes.UI.Renderers;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MapView), typeof(MapViewRenderer))]

namespace Merge.Classes.UI.Renderers {
    public class MapViewRenderer : ViewRenderer {
        protected override void OnElementChanged(ElementChangedEventArgs<View> e) {
            base.OnElementChanged(e);
            var xview = (MapView)Element;
            var map = new MKMapView(Frame);
            map.UserInteractionEnabled = xview.UserInteractionEnabled;
            map.ShowsUserLocation = xview.ShowUserLocation;
            var annotations = new List<IMKAnnotation>();
            foreach (var a in xview.Annotations) {
                annotations.Add(a);
                map.AddAnnotation(a);
            }
            if (!xview.UserInteractionEnabled)
                map.SetCamera(new MKMapCamera {
                    Altitude = 10000d,
                    CenterCoordinate = annotations[0].Coordinate
                }, true);
            else
                map.ShowAnnotations(annotations.ToArray(), true);
            map.Delegate = new MapViewDelegate(a => xview.Handler(a.Annotation));
            SetNativeControl(map);
        }

        private class MapViewDelegate : MKMapViewDelegate {
            private readonly Action<MKAnnotationView> _annotationTapped;

            public MapViewDelegate(Action<MKAnnotationView> annotationTapped) {
                _annotationTapped = annotationTapped;
            }

            public override void DidSelectAnnotationView(MKMapView mapView, MKAnnotationView view) {
                _annotationTapped?.Invoke(view);
            }
        }
    }
}

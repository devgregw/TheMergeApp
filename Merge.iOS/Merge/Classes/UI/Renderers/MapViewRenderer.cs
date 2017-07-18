#region LICENSE

// Project Merge.iOS:  MapViewRenderer.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/15/2017 at 10:38 AM.
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
using MapKit;
using Merge.Classes.UI.Controls;
using Merge.Classes.UI.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

#endregion

[assembly: ExportRenderer(typeof(MapView), typeof(MapViewRenderer))]

namespace Merge.Classes.UI.Renderers {
    public class MapViewRenderer : ViewRenderer {
        protected override void OnElementChanged(ElementChangedEventArgs<View> e) {
            base.OnElementChanged(e);
            var xview = (MapView) Element;
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
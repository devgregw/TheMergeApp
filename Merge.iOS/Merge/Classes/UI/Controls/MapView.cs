using System;
using System.Collections.Generic;
using System.Text;
using MapKit;
using MergeApi.Models.Core;
using Xamarin.Forms;

namespace Merge.Classes.UI.Controls
{
    public class MapView : View {
        public MapView(List<MKAnnotation> annotations, Action<IMKAnnotation> handler, bool allowUserInteraction, bool showUserLocation) {
            Annotations = annotations;
            Handler = handler;
            UserInteractionEnabled = allowUserInteraction;
            ShowUserLocation = showUserLocation;
        }

        public readonly bool ShowUserLocation;

        public readonly bool UserInteractionEnabled;

        public readonly Action<IMKAnnotation> Handler;

        public readonly List<MKAnnotation> Annotations;
    }
}

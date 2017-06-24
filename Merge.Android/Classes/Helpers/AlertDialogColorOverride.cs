using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace Merge.Android.Classes.Helpers {
    public class AlertDialogColorOverride : Java.Lang.Object, IDialogInterfaceOnShowListener {
        public static AlertDialogColorOverride Instance => new AlertDialogColorOverride();

        private AlertDialogColorOverride() { }

        public void OnShow(IDialogInterface dialog) {
            var map = new Dictionary<DialogButtonType, Color> {
                { DialogButtonType.Positive, Color.Argb(255, 33, 150, 243) },
                { DialogButtonType.Negative, Color.Argb(255, 77, 77, 77) },
                { DialogButtonType.Neutral, Color.Argb(255, 77, 77, 77) }
            };
            foreach (var type in map) {
                var button = ((AlertDialog)dialog).GetButton((int)type.Key);
                button?.SetTextColor(type.Value);
            }
        }
    }
}
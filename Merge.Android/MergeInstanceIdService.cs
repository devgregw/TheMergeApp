using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Firebase.Iid;
using Merge.Android.Classes.Helpers;

namespace Merge.Android {
    [Service(Label = "Merge Instance ID Service")]
    [IntentFilter(new [] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public sealed class MergeInstanceIdService : FirebaseInstanceIdService {
        public override void OnTokenRefresh() {
            Log.Info("MergeInstanceIdService", "Instance ID refreshed: " + (FirebaseInstanceId.Instance?.Token ?? "NULL"));
        }
    }
}
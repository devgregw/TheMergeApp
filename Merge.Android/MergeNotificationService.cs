using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Messaging;
using Android.Graphics;
using Android.Support.V4.App;
using Android.Util;

namespace Merge.Android {
    [Service(Label = "Merge Notification Service")]
    [IntentFilter(new []{ "com.google.firebase.MESSAGING_EVENT" })]
    public sealed class MergeNotificationService : FirebaseMessagingService {

        public override void OnMessageReceived(RemoteMessage message) {
            Log.Info("MergeNotificationService", "Notification received: " + message.MessageId);
            var fbn = message.GetNotification();
            var builder = new NotificationCompat.Builder(this).SetContentTitle(fbn.Title).SetContentText(fbn.Body)
                .SetSmallIcon(Resource.Drawable.ic_notification);
            /*if (message.Data.ContainsKey("image")) {
                builder.SetStyle(new NotificationCompat.BigPictureStyle()
                    .BigPicture(BitmapFactory.DecodeResource(Resources, Resource.Drawable.LargeHeader))
                    .SetSummaryText(fbn.Title));
            }*/
            if (message.Data.ContainsKey("action")) {
                var intent = new Intent(this, typeof(MainActivity));
                intent.PutExtra("action", message.Data["action"]);
                builder.SetContentIntent(PendingIntent.GetActivity(this, 1, intent, PendingIntentFlags.Immutable));
            }
            ((NotificationManager)GetSystemService(NotificationService)).Notify(0, builder.Build());
        }
    }
}
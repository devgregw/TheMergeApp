﻿#region LICENSE

// Project Merge.Android:  MergeNotificationService.cs (in Solution Merge.Android)
// Created by Greg Whatley on 06/30/2017 at 9:27 AM.
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

using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Android.Util;
using Firebase.Messaging;
using Merge.Android.UI.Activities;

#endregion

namespace Merge.Android.Services {
    [Service(Label = "Merge Notification Service")]
    [IntentFilter(new[] {"com.google.firebase.MESSAGING_EVENT"})]
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
            ((NotificationManager) GetSystemService(NotificationService)).Notify(0, builder.Build());
        }
    }
}
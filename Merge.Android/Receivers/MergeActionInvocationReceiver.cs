#region LICENSE

// Project Merge.Android:  MergeActionInvocationReceiver.cs (in Solution Merge.Android)
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Merge.Android.Helpers;
using Merge.Android.UI.Activities;
using MergeApi.Client;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Interfaces;
using MergeApi.Framework.Interfaces.Receivers;
using MergeApi.Models.Actions;
using MergeApi.Models.Mediums;
using MergeApi.Tools;
using Newtonsoft.Json;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Calendar = Java.Util.Calendar;
using TimeZone = Java.Util.TimeZone;
using Uri = Android.Net.Uri;

#endregion

namespace Merge.Android.Receivers {
    public sealed class MergeActionInvocationReceiver : IActionInvocationReceiver {
        private Context _context;

        public async void InvokeAddToCalendarActionAsync(AddToCalendarAction action) {
            /*var intentBuilder = new Func<DateTime, DateTime, string, string, RecurrenceRule, Intent>((start, end, loc, title, rule) => {
                var i = new Intent(Intent.ActionInsert);
                i.SetType("vnd.android.cursor.dir/event");
                i.SetData(CalendarContract.Events.ContentUri);
                i.PutExtra(CalendarContract.ExtraEventBeginTime, start.GetMillisecondsSinceEpoch());
                i.PutExtra(CalendarContract.ExtraEventAllDay, false);
                i.PutExtra(CalendarContract.ExtraEventEndTime, end.GetMillisecondsSinceEpoch());
                i.PutExtra("title", title);
                i.PutExtra("eventLocation", loc);
                i.PutExtra("android.intent.extra.EMAIL", "");
                return i;
            });
            Intent intent;
            switch (action.ParamGroup) {
                case "1":
                    var e = await LoadAsync(async () => await MergeDatabase.GetAsync<MergeEvent>(action.EventId1));
                    // ReSharper disable once PossibleInvalidOperationException
                    intent = intentBuilder(e.StartDate.Value, e.EndDate.Value,
                        string.IsNullOrWhiteSpace(e.Address) ? e.Location : e.Address, e.Title, e.RecurrenceRule);
                    break;
                case "2":
                    intent = intentBuilder(action.StartDate2, action.EndDate2, action.Location2, action.Title2, action.RecurrenceRule2);
                    break;
                default:
                    intent = null;
                    break;
            }
            StartActivity(intent);*/
            var adder = new Action<DateTime, DateTime, string, string, RecurrenceRule>(
                (start, end, loc, title, rule) => {
                    var uri = Uri.Parse("content://com.android.calendar/events");
                    var values = new ContentValues();
                    values.Put(CalendarContract.EventsColumns.CalendarId, 1);
                    values.Put(CalendarContract.EventsColumns.Title, title);
                    values.Put(CalendarContract.EventsColumns.EventLocation, loc);
                    values.Put(CalendarContract.EventsColumns.EventTimezone, TimeZone.Default.ID);
                    var begin = Calendar.Instance;
                    var calEnd = Calendar.Instance;
                    calEnd.Set(end.Year, end.Month - 1, end.Day, end.Hour, end.Minute);
                    begin.Set(start.Year, start.Month - 1, start.Day, start.Hour, start.Minute);
                    values.Put(CalendarContract.EventsColumns.Dtstart, begin.TimeInMillis);
                    if (rule == null) {
                        values.Put(CalendarContract.EventsColumns.Dtend, calEnd.TimeInMillis);
                    } else {
                        values.Put(CalendarContract.EventsColumns.Duration,
                            $"P{(calEnd.TimeInMillis - begin.TimeInMillis) / 1000}S");
                        var rrule = $"FREQ={rule.Frequency.ToString().ToUpper()};INTERVAL={rule.Interval}";
                        if (rule.End.HasValue)
                            rrule +=
                                $";UNTIL={rule.End.Value.ToUniversalTime().ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.CurrentUICulture)}";
                        //$";UNTIL={end.ToUniversalTime().ToString("u", CultureInfo.CurrentUICulture).Replace("-", "").Replace(" ", "T").Replace(":", "")}";
                        else if (rule.Count.HasValue)
                            rrule += $";COUNT={rule.Count - 1}";
                        values.Put(CalendarContract.EventsColumns.Rrule, rrule);
                        Console.WriteLine("RRULE: " + rrule);
                    }
                    try {
                        _context.ContentResolver.Insert(uri, values);
                        Toast.MakeText(_context, $"\"{title}\" was added to your calendar.", ToastLength.Long).Show();
                    } catch (SecurityException) {
                        Toast.MakeText(_context,
                            $"\"{title}\" could not be added to your calendar because you didn't grant Merge permission to read and write to your calendar.",
                            ToastLength.Long).Show();
                    }
                });
            switch (action.ParamGroup) {
                case "1":
                    var e = await GetOrLoad(action.EventId1, () => DataCache.Events, v => DataCache.Events = v);
                    // ReSharper disable once PossibleInvalidOperationException
                    adder(e.StartDate.Value, e.EndDate.Value,
                        string.IsNullOrWhiteSpace(e.Address) ? e.Location : e.Address, e.Title, e.RecurrenceRule);
                    break;
                case "2":
                    adder(action.StartDate2.Value, action.EndDate2.Value, action.Location2, action.Title2,
                        action.RecurrenceRule2);
                    break;
            }
        }

        public async void InvokeGetDirectionsActionAsync(GetDirectionsAction action) {
            var intentBuilder = new Func<string, Intent>(address => {
                var i = new Intent(Intent.ActionView, Uri.Parse($"google.navigation:q={address}"));
                i = i.SetPackage("com.google.android.apps.maps");
                return i;
            });
            Intent intent;
            switch (action.ParamGroup) {
                case "1":
                    var e = await GetOrLoad(action.EventId1, () => DataCache.Events, v => DataCache.Events = v);
                    if (string.IsNullOrWhiteSpace(e.Address)) {
                        MakeToast("No address specified!").Show();
                        return;
                    }
                    intent = intentBuilder(e.Address);
                    break;
                case "2":
                    var g = await GetOrLoad(action.GroupId2, () => DataCache.Groups, v => DataCache.Groups = v);
                    intent = intentBuilder(g.Address);
                    break;
                case "3":
                    intent = intentBuilder(action.Address3);
                    break;
                case "4":
                    var special = new Intent(Intent.ActionView,
                        Uri.Parse(
                            $"google.navigation:q={action.Coordinates4.Latitude},{action.Coordinates4.Longitude}"));
                    special = special.SetPackage("com.google.android.apps.maps");
                    intent = special;
                    break;
                default:
                    intent = null;
                    break;
            }
            StartActivity(intent);
        }

        public void InvokeLaunchUriAction(LaunchUriAction action) {
            StartActivity(Intent.CreateChooser(new Intent(Intent.ActionView, Uri.Parse(action.Uri1)), "Choose an app"));
        }

        public void InvokeCallAction(CallAction action) {
            var intentBuilder =
                new Func<string, Intent>(number => new Intent(Intent.ActionCall, Uri.Parse($"tel:{number}")));
            Intent intent;
            switch (action.ParamGroup) {
                case "1":
                    intent = intentBuilder(action.ContactMedium1.PhoneNumber);
                    break;
                case "2":
                    intent = intentBuilder(action.PhoneNumber2);
                    break;
                default:
                    intent = null;
                    break;
            }
            StartActivity(intent);
        }

        public void InvokeTextAction(TextAction action) {
            var intentBuilder =
                new Func<string, Intent>(number => new Intent(Intent.ActionView, Uri.Parse($"sms:{number}")));
            Intent intent;
            switch (action.ParamGroup) {
                case "1":
                    if (!action.ContactMedium1.CanReceiveSMS) {
                        MakeToast("This phone number cannot receive SMS!").Show();
                        return;
                    } else {
                        intent = intentBuilder(action.ContactMedium1.PhoneNumber);
                        break;
                    }
                case "2":
                    intent = intentBuilder(action.PhoneNumber2);
                    break;
                default:
                    intent = null;
                    break;
            }
            StartActivity(intent);
        }

        public void InvokeEmailAction(EmailAction action) {
            var intentBuilder =
                new Func<string, Intent>(address => new Intent(Intent.ActionSendto, Uri.Parse($"mailto:{address}")));
            Intent intent;
            switch (action.ParamGroup) {
                case "1":
                    intent = intentBuilder(action.ContactMedium1.Address);
                    break;
                case "2":
                    intent = intentBuilder(action.Address2);
                    break;
                default:
                    intent = null;
                    break;
            }
            StartActivity(intent);
        }

        public async void InvokeShowContactInfoActionAsync(ShowContactInfoAction action) {
            var maker = new Func<List<MediumBase>, Dictionary<string, MediumBase>>(mediums => {
                var result = new Dictionary<string, MediumBase>();
                foreach (var medium in mediums)
                    if (medium is EmailAddressMedium) {
                        var email = (EmailAddressMedium) medium;
                        result.Add($"Email {email.Who} ({email.Kind.ToString()})", email);
                    } else if (medium is PhoneNumberMedium) {
                        var phone = (PhoneNumberMedium) medium;
                        result.Add($"Call {phone.Who} ({phone.Kind.ToString()})", phone);
                        if (phone.CanReceiveSMS)
                            result.Add($"Text {phone.Who} ({phone.Kind.ToString()})", phone);
                    }
                return result;
            });
            var items = new Dictionary<string, MediumBase>();
            var name = "";
            switch (action.ParamGroup) {
                case "1":
                    var g = await GetOrLoad(action.GroupId1, () => DataCache.Groups, v => DataCache.Groups = v);
                    items = maker(g.ContactMediums);
                    name = g.Name;
                    break;
                case "2":
                    items = maker(action.ContactMediums2);
                    name = action.Name2;
                    break;
            }
            var builder = new AlertDialog.Builder(_context);
            var strings = new List<string>(items.Keys).ToArray();
            var dialog = builder.SetTitle("Contact " + name)
                .SetItems(strings, (s, e) => {
                    var item = strings[e.Which];
                    if (item.Contains("Email")) {
                        items.TryGetValue(item, out var value);
                        EmailAction.FromContactMedium((EmailAddressMedium) value).Invoke();
                    } else if (item.Contains("Call") || item.Contains("Text")) {
                        items.TryGetValue(item, out var value);
                        if (item.Contains("Call"))
                            CallAction.FromContactMedium((PhoneNumberMedium) value).Invoke();
                        else
                            TextAction.FromContactMedium((PhoneNumberMedium) value).Invoke();
                    }
                })
                .SetCancelable(true)
                .SetPositiveButton("Close", (s, e) => { })
                .SetNegativeButton("View Contact Info", (s, e) => {
                    var msg = "";
                    var dupes = new List<PhoneNumberMedium>();
                    foreach (var medium in items.Values)
                        if (medium is EmailAddressMedium) {
                            msg +=
                                $"{(medium as EmailAddressMedium).Who + " (" + ((EmailAddressMedium) medium).Kind.ToString().ToLower() + ")"}\n {(medium as EmailAddressMedium).Address}\n\n";
                        } else if (medium is PhoneNumberMedium) {
                            if (((PhoneNumberMedium) medium).CanReceiveSMS) {
                                if (dupes.Contains((PhoneNumberMedium) medium))
                                    continue;
                                dupes.Add((PhoneNumberMedium) medium);
                            }
                            msg +=
                                $"{(medium as PhoneNumberMedium).Who + " (" + ((PhoneNumberMedium) medium).Kind.ToString().ToLower() + ")"}\n {(medium as PhoneNumberMedium).PhoneNumber}\n\n";
                        }
                    msg = msg.Remove(msg.Length - 2);
                    new AlertDialog.Builder(_context).SetTitle($"Contact {name}")
                        .SetMessage(msg)
                        .SetPositiveButton("Close", (ss, ee) => { })
                        .Show();
                }).Create();
            dialog.Show();
        }

        public void InvokeOpenGroupMapPageAction(OpenGroupMapPageAction action) {
            StartActivity(new Intent(_context, typeof(GroupMapActivity)));
        }

        public async void InvokeOpenEventDetailsActionAsync(OpenEventDetailsAction action) {
            var e = await GetOrLoad(action.EventId1, () => DataCache.Events, v => DataCache.Events = v);
            var intent = new Intent(_context, typeof(DataDetailActivity));
            intent.PutExtra("json", JsonConvert.SerializeObject(e));
            intent.PutExtra("title", e.Title);
            intent.PutExtra("url", e.CoverImage);
            intent.PutExtra("type", "event");
            StartActivity(intent);
        }

        public async void InvokeOpenGroupDetailsActionAsync(OpenGroupDetailsAction action) {
            var g = await GetOrLoad(action.GroupId1, () => DataCache.Groups, v => DataCache.Groups = v);
            var intent = new Intent(_context, typeof(DataDetailActivity));
            intent.PutExtra("json", JsonConvert.SerializeObject(g));
            intent.PutExtra("title", g.Name);
            intent.PutExtra("url", g.CoverImage);
            intent.PutExtra("type", "group");
            StartActivity(intent);
        }

        public async void InvokeOpenPageActionAsync(OpenPageAction action) {
            var p = await GetOrLoad(action.PageId1, () => DataCache.Pages, v => DataCache.Pages = v);
            var intent = new Intent(_context, typeof(DataDetailActivity));
            intent.PutExtra("json", JsonConvert.SerializeObject(p));
            intent.PutExtra("title", p.Title);
            intent.PutExtra("url", p.CoverImage);
            intent.PutExtra("type", "page");
            StartActivity(intent);
        }

        private async Task<T> GetOrLoad<T>(string id, Func<IEnumerable<T>> getter,
            Func<IEnumerable<T>, IEnumerable<T>> setter) where T : IIdentifiable {
            return (getter() == null || getter().All(i => i.Id != id)
                ? await LoadAsync(async () => setter(await MergeDatabase.ListAsync<T>()))
                : getter()).First(i => i.Id == id);
        }

        public void SetContext(Context c) {
            _context = c;
            /*_dialog = new ProgressDialog(_context) {
                Indeterminate = true
            };
            _dialog.SetMessage("Loading...");
            _dialog.SetCancelable(false);*/
            //_dialog = new AlertDialog.Builder(_context).SetCancelable(false).SetView(Resource.Layout.LoadingLayout).Create();
        }

        private Toast MakeToast(string message) {
            var t = Toast.MakeText(_context, message, ToastLength.Long);
            t.SetGravity(GravityFlags.Center, 0, 0);
            return t;
        }

        //private ProgressDialog _dialog;

        private async Task<T> LoadAsync<T>(Func<Task<T>> loader) {
            //var t = MakeToast("Please wait...");
            //t.Show();
            var _dialog = new ProgressDialog(_context) {
                Indeterminate = true
            };
            _dialog.SetMessage("Loading...");
            _dialog.SetCancelable(false);
            _dialog.Show();
            var res = await Task.Run(loader);
            _dialog.Dismiss();
            _dialog.Dispose();
            //t.Cancel();
            return res;
        }

        private void StartActivity(Intent i) {
            if (i != null)
                _context.StartActivity(i);
            else
                MakeToast("The Intent was null!").Show();
        }
    }
}
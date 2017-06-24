using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Merge.Android.Classes.Helpers;
using MergeApi.Framework.Interfaces.Receivers;
using MergeApi.Models.Actions;
using MergeApi.Client;
using MergeApi.Framework.Abstractions;
using MergeApi.Models.Core;
using MergeApi.Models.Mediums;
using Uri = Android.Net.Uri;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Newtonsoft.Json;

namespace Merge.Android {
    public sealed class MergeActionInvocationReceiver : IActionInvocationReceiver {
        private Context _context;

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

        public async void InvokeAddToCalendarActionAsync(AddToCalendarAction action) {
            var intentBuilder = new Func<DateTime, DateTime, string, string, Intent>((start, end, loc, title) => {
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
                    intent = intentBuilder(e.StartDate.Value, e.EndDate.Value, string.IsNullOrWhiteSpace(e.Address) ? e.Location : e.Address, e.Title);
                    break;
                case "2":
                    intent = intentBuilder(action.StartDate2, action.EndDate2, action.Location2, action.Title2);
                    break;
                default:
                    intent = null;
                    break;
            }
            StartActivity(intent);
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
                    var e = await LoadAsync(async () => await MergeDatabase.GetAsync<MergeEvent>(action.EventId1));
                    if (string.IsNullOrWhiteSpace(e.Address)) {
                        MakeToast("No address specified!").Show();
                        return;
                    }
                    intent = intentBuilder(e.Address);
                    break;
                case "2":
                    var g = await LoadAsync(async () => await MergeDatabase.GetAsync<MergeGroup>(action.GroupId2));
                    intent = intentBuilder(g.Address);
                    break;
                case "3":
                    intent = intentBuilder(action.Address3);
                    break;
                case "4":
                    var special = new Intent(Intent.ActionView, Uri.Parse($"google.navigation:q={action.Coordinates4.Latitude},{action.Coordinates4.Longitude}"));
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
            var intentBuilder = new Func<string, Intent>(number => new Intent(Intent.ActionCall, Uri.Parse($"tel:{number}")));
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
                foreach (var medium in mediums) {
                    if (medium is EmailAddressMedium) {
                        var email = (EmailAddressMedium) medium;
                        result.Add($"Email {email.Who} ({email.Kind.ToString()})", email);
                    } else if (medium is PhoneNumberMedium) {
                        var phone = (PhoneNumberMedium) medium;
                        result.Add($"Call {phone.Who} ({phone.Kind.ToString()})", phone);
                        if (phone.CanReceiveSMS)
                            result.Add($"Text {phone.Who} ({phone.Kind.ToString()})", phone);
                    }
                }
                return result;
            });
            var items = new Dictionary<string, MediumBase>();
            var name = "";
            switch (action.ParamGroup) {
                case "1":
                    var g = await LoadAsync(async () => await MergeDatabase.GetAsync<MergeGroup>(action.GroupId1));
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
                        EmailAction.FromContactMedium((EmailAddressMedium)value).Invoke();
                    } else if (item.Contains("Call") || item.Contains("Text")) {
                        items.TryGetValue(item, out var value);
                        if (item.Contains("Call"))
                            CallAction.FromContactMedium((PhoneNumberMedium)value).Invoke();
                        else
                            TextAction.FromContactMedium((PhoneNumberMedium)value).Invoke();
                    }
                })
                .SetCancelable(true)
                .SetPositiveButton("Close", (s, e) => { })
                .SetNegativeButton("View Contact Info", (s, e) => {
                    var msg = "";
                    var dupes = new List<PhoneNumberMedium>();
                    foreach (var medium in items.Values) {
                        if (medium is EmailAddressMedium) {
                            msg +=
                                $"{(medium as EmailAddressMedium).Who + " (" + ((EmailAddressMedium)medium).Kind.ToString().ToLower() + ")"}\n {(medium as EmailAddressMedium).Address}\n\n";
                        } else if (medium is PhoneNumberMedium) {
                            if (((PhoneNumberMedium)medium).CanReceiveSMS) {
                                if (dupes.Contains((PhoneNumberMedium)medium))
                                    continue;
                                dupes.Add((PhoneNumberMedium)medium);
                            }
                            msg +=
                                $"{(medium as PhoneNumberMedium).Who + " (" + ((PhoneNumberMedium)medium).Kind.ToString().ToLower() + ")"}\n {(medium as PhoneNumberMedium).PhoneNumber}\n\n";
                        }
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
            var e = await LoadAsync(async () => await MergeDatabase.GetAsync<MergeEvent>(action.EventId1));
            var intent = new Intent(_context, typeof(DataDetailActivity));
            intent.PutExtra("json", JsonConvert.SerializeObject(e));
            intent.PutExtra("title", e.Title);
            intent.PutExtra("url", e.CoverImage);
            intent.PutExtra("type", "event");
            StartActivity(intent);
        }

        public async void InvokeOpenGroupDetailsActionAsync(OpenGroupDetailsAction action) {
            var g = await LoadAsync(async () => await MergeDatabase.GetAsync<MergeGroup>(action.GroupId1));
            var intent = new Intent(_context, typeof(DataDetailActivity));
            intent.PutExtra("json", JsonConvert.SerializeObject(g));
            intent.PutExtra("title", g.Name);
            intent.PutExtra("url", g.CoverImage);
            intent.PutExtra("type", "group");
            StartActivity(intent);
        }

        public async void InvokeOpenPageActionAsync(OpenPageAction action) {
            var p = await LoadAsync(async () => await MergeDatabase.GetAsync<MergePage>(action.PageId1));
            var intent = new Intent(_context, typeof(DataDetailActivity));
            intent.PutExtra("json", JsonConvert.SerializeObject(p));
            intent.PutExtra("title", p.Title);
            intent.PutExtra("url", p.CoverImage);
            intent.PutExtra("type", "page");
            StartActivity(intent);
        }
    }
}
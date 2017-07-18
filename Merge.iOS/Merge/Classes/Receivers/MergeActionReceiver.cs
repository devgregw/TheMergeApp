#region LICENSE

// Project Merge.iOS:  MergeActionReceiver.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 06/27/2017 at 2:16 PM.
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
using System.Linq;
using System.Threading.Tasks;
using CoreLocation;
using EventKit;
using Foundation;
using MapKit;
using Merge.Classes.Helpers;
using Merge.Classes.UI;
using Merge.Classes.UI.Pages;
using MergeApi.Client;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Interfaces;
using MergeApi.Framework.Interfaces.Receivers;
using MergeApi.Models.Actions;
using MergeApi.Models.Mediums;
using MergeApi.Tools;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

#endregion

namespace Merge.Classes.Receivers {
    public sealed class MergeActionReceiver : IActionInvocationReceiver {
        public async void InvokeAddToCalendarActionAsync(AddToCalendarAction action) {
            void InternalInvoke(DateTime start, DateTime end, string location, string title, RecurrenceRule rule) {
                var eventStore = new EKEventStore();
                eventStore.RequestAccess(EKEntityType.Event, (granted, error) => {
                    if (granted) {
                        NSError saveError;
                        var newEvent = EKEvent.FromStore(eventStore);
                        newEvent.AllDay = false;
                        newEvent.EndDate = end.ToNSDate();
                        newEvent.StartDate = start.ToNSDate();
                        newEvent.Location = location;
                        newEvent.Title = title;
                        if (rule != null) {
                            EKRecurrenceEnd ekend = null;
                            if (rule.End.HasValue)
                                ekend = EKRecurrenceEnd.FromEndDate(rule.End.Value.ToNSDate());
                            else if (rule.Count.HasValue)
                                ekend = EKRecurrenceEnd.FromOccurrenceCount(rule.Count.Value);
                            newEvent.AddRecurrenceRule(
                                new EKRecurrenceRule(
                                    rule.Frequency.Manipulate(f => f.ToString().ToEnum<EKRecurrenceFrequency>()),
                                    rule.Interval, ekend));
                        }
                        newEvent.Calendar = eventStore.DefaultCalendarForNewEvents;
                        eventStore.SaveEvent(newEvent, EKSpan.ThisEvent, true, out saveError);
                        if (saveError != null)
                            ShowErrorAlert("Add To Calendar",
                                $"An error occurred while adding \"{title}\" to your calendar.", saveError);
                        else
                            ShowSimpleAlert("Add To Calendar",
                                $"\"{title}\" was added to your calendar successfully.");
                    } else {
                        if (error == null)
                            ShowSimpleAlert("Add To Calendar",
                                $"\"{title}\" could not be added to your calendar because you did not give Merge permission.");
                        else
                            ShowErrorAlert("Add To Calendar",
                                "An error occurred while requesting access to your calendar.", error);
                    }
                });
            }

            switch (action.ParamGroup) {
                case "1":
                    try {
                        var e = await GetOrLoad(action.EventId1, () => DataCache.Events, v => DataCache.Events = v);
                        InternalInvoke(e.StartDate.Value, e.EndDate.Value,
                            string.IsNullOrWhiteSpace(e.Address) ? e.Location : e.Address, e.Title, e.RecurrenceRule);
                    } catch (Exception ex) {
                        ShowErrorAlert("Add To Calendar", "An error occurred while loading content.", ex);
                    }
                    break;
                case "2":
                    InternalInvoke(action.StartDate2.Value, action.EndDate2.Value, action.Location2, action.Title2,
                        action.RecurrenceRule2);
                    break;
                default:
                    ShowBadParamGroupAlert(action);
                    break;
            }
        }

        public async void InvokeGetDirectionsActionAsync(GetDirectionsAction action) {
            void InternalInvokeWithCoordinates(CLLocationCoordinate2D coords, NSDictionary addressDictionary = null) {
                new MKMapItem(new MKPlacemark(coords, addressDictionary)).OpenInMaps(new MKLaunchOptions {
                    DirectionsMode = MKDirectionsMode.Driving
                });
            }

            async Task InternalInvokeWithAddress(string address) {
                var geocodeResult = await LoadAsync(async () => await new CLGeocoder().GeocodeAddressAsync(address));
                InternalInvokeWithCoordinates(geocodeResult[0].Location.Coordinate, geocodeResult[0].AddressDictionary);
            }

            switch (action.ParamGroup) {
                case "1":
                    try {
                        var e = await GetOrLoad(action.EventId1, () => DataCache.Events, v => DataCache.Events = v);
                        if (string.IsNullOrWhiteSpace(e.Address))
                            ShowSimpleAlert("Get Directions",
                                $"Directions to \"{e.Title}\" cannot be computed because the event's address is unspecified.");
                        else
                            await InternalInvokeWithAddress(e.Address);
                    } catch (Exception ex) {
                        ShowErrorAlert("Get Directions", "An error occurred while loading content.", ex);
                    }
                    break;
                case "2":
                    try {
                        var g = await GetOrLoad(action.GroupId2, () => DataCache.Groups, v => DataCache.Groups = v);
                        await InternalInvokeWithAddress(g.Address);
                    } catch (Exception ex) {
                        ShowErrorAlert("Get Directions", "An error occurred while loading content.", ex);
                    }
                    break;
                case "3":
                    await InternalInvokeWithAddress(action.Address3);
                    break;
                case "4":
                    InternalInvokeWithCoordinates(
                        action.Coordinates4.Manipulate(p => new CLLocationCoordinate2D(Convert.ToDouble(p.Latitude),
                            Convert.ToDouble(p.Longitude))));
                    break;
                default:
                    ShowBadParamGroupAlert(action);
                    break;
            }
        }

        public void InvokeLaunchUriAction(LaunchUriAction action) {
            UIApplication.SharedApplication.OpenUrl(NSUrl.FromString(action.Uri1.Replace(" ", "%20")));
        }

        public void InvokeCallAction(CallAction action) {
            void InternalInvoke(string number) {
                UIApplication.SharedApplication.OpenUrl(NSUrl.FromString($"telprompt://{number}"));
            }

            switch (action.ParamGroup) {
                case "1":
                    InternalInvoke(action.ContactMedium1.PhoneNumber);
                    break;
                case "2":
                    InternalInvoke(action.PhoneNumber2);
                    break;
                default:
                    ShowBadParamGroupAlert(action);
                    break;
            }
        }

        public void InvokeTextAction(TextAction action) {
            void InternalInvoke(string number) {
                UIApplication.SharedApplication.OpenUrl(NSUrl.FromString($"sms:{number}"));
            }

            switch (action.ParamGroup) {
                case "1":
                    if (action.ContactMedium1.CanReceiveSMS)
                        InternalInvoke(action.ContactMedium1.PhoneNumber);
                    else
                        ShowSimpleAlert("Send Text",
                            $"A text message cannot be created for \"{action.ContactMedium1.PhoneNumber}\" because it has been marked as non-sms capable.");
                    break;
                case "2":
                    InternalInvoke(action.PhoneNumber2);
                    break;
                default:
                    ShowBadParamGroupAlert(action);
                    break;
            }
        }

        public void InvokeEmailAction(EmailAction action) {
            void InternalInvoke(string address) {
                UIApplication.SharedApplication.OpenUrl(NSUrl.FromString($"mailto://{address}"));
            }

            switch (action.ParamGroup) {
                case "1":
                    InternalInvoke(action.ContactMedium1.Address);
                    break;
                case "2":
                    InternalInvoke(action.Address2);
                    break;
                default:
                    ShowBadParamGroupAlert(action);
                    break;
            }
        }

        public async void InvokeShowContactInfoActionAsync(ShowContactInfoAction action) {
            Dictionary<string, MediumBase> MakeItems(List<MediumBase> mediums) {
                var list = new Dictionary<string, MediumBase>();
                foreach (var cm in mediums)
                    if (cm is EmailAddressMedium) {
                        list.Add(
                            "Email " + ((EmailAddressMedium) cm).Who + " (" +
                            ((EmailAddressMedium) cm).Kind.ToString().ToLower() + ")", cm);
                    } else if (cm is PhoneNumberMedium) {
                        list.Add(
                            "Call " + ((PhoneNumberMedium) cm).Who + " (" +
                            ((PhoneNumberMedium) cm).Kind.ToString().ToLower() + ")", cm);
                        if (((PhoneNumberMedium) cm).CanReceiveSMS)
                            list.Add(
                                "Text " + ((PhoneNumberMedium) cm).Who + " (" +
                                ((PhoneNumberMedium) cm).Kind.ToString().ToLower() + ")", cm);
                    }
                return list;
            }

            Dictionary<string, MediumBase> items = null;
            string name = null;
            switch (action.ParamGroup) {
                case "1":
                    try {
                        var g = await GetOrLoad(action.GroupId1, () => DataCache.Groups, v => DataCache.Groups = v);
                        items = MakeItems(g.ContactMediums);
                        name = g.Name;
                    } catch (Exception ex) {
                        ShowErrorAlert("Show Contact Info", "An error occurred while loading content.", ex);
                    }
                    break;
                case "2":
                    items = MakeItems(action.ContactMediums2);
                    name = action.Name2;
                    break;
                default:
                    ShowBadParamGroupAlert(action);
                    break;
            }
            if (items == null || name == null)
                return;
            AlertHelper.ShowSheet("Contact " + name, (s, i) => {
                var labels = items.Keys.ToArray();
                var mediums = items.Values.ToArray();
                if (i == 0) {
                    var msg = "";
                    foreach (var medium in mediums)
                        if (medium is EmailAddressMedium) {
                            msg +=
                                $"{(medium as EmailAddressMedium).Who + " (" + ((EmailAddressMedium) medium).Kind.ToString().ToLower() + ")"}\n {(medium as EmailAddressMedium).Address}\n\n";
                        } else if (medium is PhoneNumberMedium) {
                            var nmsg =
                                $"{(medium as PhoneNumberMedium).Who + " (" + ((PhoneNumberMedium) medium).Kind.ToString().ToLower() + ")"}\n {(medium as PhoneNumberMedium).PhoneNumber}\n\n";
                            if (!msg.Contains(nmsg))
                                msg += nmsg;
                        }
                    msg = msg.Remove(msg.Length - 2);
                    AlertHelper.ShowAlert("Contact Info for " + name, msg, null, "Close", null);
                    return;
                }
                var ni = i - 1;
                if (ni == labels.Length)
                    return;
                var item = labels[ni];
                if (item.Contains("Email"))
                    EmailAction.FromContactMedium((EmailAddressMedium) mediums[ni]).Invoke();
                else if (item.Contains("Call"))
                    CallAction.FromContactMedium((PhoneNumberMedium) mediums[ni]).Invoke();
                else if (item.Contains("Text"))
                    TextAction.FromContactMedium((PhoneNumberMedium) mediums[ni]).Invoke();
            }, "Cancel", null, new[] {"View Info"}.Concat(items.Keys.ToArray()).ToArray());
        }

        public void InvokeOpenGroupMapPageAction(OpenGroupMapPageAction action) {
            ((NavigationPage) Application.Current.MainPage).PushAsync(new GroupsMapPage());
        }

        public async void InvokeOpenGroupDetailsActionAsync(OpenGroupDetailsAction action) {
            try {
                await ((NavigationPage) Application.Current.MainPage).PushAsync(new DataDetailPage(
                    await GetOrLoad(action.GroupId1, () => DataCache.Groups, v => DataCache.Groups = v)));
            } catch (Exception ex) {
                ShowErrorAlert("Open Group Details", "An error occurred while loading content.", ex);
            }
        }

        public async void InvokeOpenEventDetailsActionAsync(OpenEventDetailsAction action) {
            try {
                await ((NavigationPage) Application.Current.MainPage).PushAsync(new DataDetailPage(
                    await GetOrLoad(action.EventId1, () => DataCache.Events, v => DataCache.Events = v)));
            } catch (Exception ex) {
                ShowErrorAlert("Open Event Details", "An error occurred while loading content.", ex);
            }
        }

        public async void InvokeOpenPageActionAsync(OpenPageAction action) {
            try {
                await ((NavigationPage) Application.Current.MainPage).PushAsync(new DataDetailPage(
                    await GetOrLoad(action.PageId1, () => DataCache.Pages, v => DataCache.Pages = v)));
            } catch (Exception ex) {
                ShowErrorAlert("Open Page", "An error occurred while loading content.", ex);
            }
        }

        private void ShowErrorAlert(string title, string message, NSError error) {
            AlertHelper.ShowAlert(title,
                $"{message}\n{error.LocalizedDescription} ({error.GetType().FullName})\n{error.LocalizedFailureReason}",
                null, "OK", null);
        }

        private void ShowErrorAlert(string title, string message, Exception error) {
            AlertHelper.ShowAlert(title, $"{message}\n{error.Message} ({error.GetType().FullName})", null, "OK", null);
        }

        public void ShowSimpleAlert(string title, string message) {
            AlertHelper.ShowAlert(title, message, null, "OK", null);
        }

        public void ShowBadParamGroupAlert(ActionBase action) {
            ShowSimpleAlert("Invalid Parameter Group",
                $"The parameter group \"{action.ParamGroup}\" is undefined for the type \"{action.GetType().FullName}\".");
        }

        public void ShowLoadingScreen(string message = "Loading...") {
            ((App) Application.Current).ShowLoader(message);
        }

        public void HideLoadingScreen() {
            ((App) Application.Current).HideLoader();
        }

        public async Task<T> GetOrLoad<T>(string id, Func<IEnumerable<T>> getter,
            Func<IEnumerable<T>, IEnumerable<T>> setter) where T : IIdentifiable {
            return (getter() == null || getter().All(i => i.Id != id)
                    ? await LoadAsync(async () => setter(await MergeDatabase.ListAsync<T>()))
                    : getter())
                .First(i => i.Id == id);
        }

        public async Task<T> LoadAsync<T>(Func<Task<T>> task) {
            ShowLoadingScreen();
            T result;
            try {
                result = await Task.Run(task);
            } finally {
                HideLoadingScreen();
            }
            return result;
        }
    }
}
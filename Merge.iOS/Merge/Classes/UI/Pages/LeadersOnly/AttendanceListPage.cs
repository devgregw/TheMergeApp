#region LICENSE

// Project Merge.iOS:  AttendanceListPage.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/14/2017 at 6:02 PM.
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
using Foundation;
using Merge.Classes.Helpers;
using MergeApi.Client;
using MergeApi.Models.Core.Attendance;
using Xamarin.Forms;

#endregion

namespace Merge.Classes.UI.Pages.LeadersOnly {
    public class AttendanceListPage : ContentPage {
        public enum GroupType {
            JuniorHigh,
            HighSchool
        }

        private Func<Task<List<Cell>>> _getter;

        private TableSection _section;

        private bool _shouldShowLoader;

        private AttendanceListPage() {
            _section = new TableSection();
            Content = new TableView(new TableRoot {
                _section
            }) {
                Intent = TableIntent.Data
            };
        }

        private AttendanceListPage(string title, Func<Task<List<Cell>>> items, bool shouldShowLoader) : this() {
            Title = title;
            _getter = items;
            _shouldShowLoader = shouldShowLoader;
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            if (_shouldShowLoader)
                new NSObject().InvokeOnMainThread(() => ((App) Application.Current).ShowLoader("Loading..."));
            _section.Clear();
            try {
                (await _getter()).ForEach(_section.Add);
            } catch {
                AlertHelper.ShowAlert("Error",
                    "An error occurred while loading data.  Try checking your internet connection.",
                    (a, i) => Navigation.PopAsync(), "OK");
            } finally {
                if (_shouldShowLoader)
                    new NSObject().InvokeOnMainThread(() => ((App) Application.Current).HideLoader());
            }
        }

        public static AttendanceListPage CreateMain(INavigation navigation) {
            return new AttendanceListPage("Attendance Manager", () => Task.FromResult(new List<Cell> {
                new TextCell {
                    Text = "Junior High",
                    Command = new Command(
                        () => navigation.PushAsync(CreateGroupsList(navigation, GroupType.JuniorHigh)))
                },
                new TextCell {
                    Text = "High School",
                    Command = new Command(
                        () => navigation.PushAsync(CreateGroupsList(navigation, GroupType.HighSchool)))
                }
            }), false);
        }

        public static AttendanceListPage CreateGroupsList(INavigation navigation, GroupType type) {
            return new AttendanceListPage("Select Group", async () => {
                return (await MergeDatabase.ListAsync<AttendanceGroup>()).Where(g => type == GroupType.JuniorHigh
                    ? (int) g.GradeLevel <= 8
                    : (int) g.GradeLevel >= 9).Select(g => new TextCell {
                    Text = g.Summary,
                    Detail = $"attendance/groups/{g.Id}",
                    DetailColor = Color.Gray,
                    Command = new Command(() => navigation.PushAsync(CreateRecordsList(navigation, g)))
                }).Cast<Cell>().ToList();
                ;
            }, true);
        }

        public static AttendanceListPage CreateRecordsList(INavigation navigation, AttendanceGroup group) {
            return new AttendanceListPage("Records", async () => {
                var addCell = new ViewCell {
                    View = new Button {
                        Text = "Add Record",
                        Command = new Command(() => navigation.PushModalAsync(new RecordEditorPage(group, null)
                            .WrapInNavigationPage()))
                    }
                };
                var editCell = new ViewCell {
                    View = new Button {
                        Text = "Edit Group",
                        Command = new Command(
                            () => navigation.PushModalAsync(new GroupEditorPage(group).WrapInNavigationPage()))
                    }
                };
                var cells = (await MergeDatabase.ListAsync<AttendanceRecord>()).Where(r => r.GroupId == group.Id)
                    .OrderByDescending(r => r.Date).Select(r => new TextCell {
                        Text = r.Date.ToLongDateString(),
                        Detail = $"attendance/groups/{group.Id}/records/{r.DateString}",
                        DetailColor = Color.Gray,
                        Command = new Command(
                            () => navigation.PushModalAsync(new RecordEditorPage(group, r).WrapInNavigationPage()))
                    }).Cast<Cell>().ToList();
                cells.Insert(0, addCell);
                cells.Insert(0, editCell);
                return cells;
            }, true);
        }
    }
}
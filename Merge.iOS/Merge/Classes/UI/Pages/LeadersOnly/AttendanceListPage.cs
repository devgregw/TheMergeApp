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
using MergeApi.Tools;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using Merge.Classes.UI.Pages.LeadersOnly;
using System.ComponentModel;
using CoreGraphics;
using MergeApi.Models.Core;

#endregion

[assembly: ExportRenderer(typeof(AttendanceListPage.AttendanceListCell), typeof(AttendanceListCellRenderer))]

namespace Merge.Classes.UI.Pages.LeadersOnly {
    public class AttendanceListCellRenderer : CellRenderer {
        static readonly Color DefaultDetailColor = new Color(.32, .4, .57);
        static readonly Color DefaultTextColor = Color.Black;
        
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv) {
            var textCell = (TextCell)item;
            var tvc = reusableCell as CellTableViewCell;
            if (tvc == null)
                tvc = new CellTableViewCell(UITableViewCellStyle.Subtitle, item.GetType().FullName);
            else
                tvc.Cell.PropertyChanged -= tvc.HandlePropertyChanged;
            tvc.Cell = textCell;
            textCell.PropertyChanged += tvc.HandlePropertyChanged;
            tvc.PropertyChanged = HandlePropertyChanged;
            tvc.TextLabel.Text = textCell.Text;
            tvc.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
            tvc.TextLabel.Lines = 0;
            tvc.DetailTextLabel.Text = textCell.Detail;
            tvc.DetailTextLabel.LineBreakMode = UILineBreakMode.WordWrap;
            tvc.DetailTextLabel.Lines = 0;
            tvc.TextLabel.TextColor = textCell.TextColor.ToUIColor(DefaultTextColor);
            tvc.DetailTextLabel.TextColor = textCell.DetailColor.ToUIColor(DefaultDetailColor);
            WireUpForceUpdateSizeRequested(item, tvc, tv);
            UpdateIsEnabled(tvc, textCell);
            UpdateBackground(tvc, item);
            return tvc;
        }
        
        protected virtual void HandlePropertyChanged(object sender, PropertyChangedEventArgs args) {
            var tvc = (CellTableViewCell)sender;
            var textCell = (TextCell)tvc.Cell;
            if (args.PropertyName == TextCell.TextProperty.PropertyName) {
                tvc.TextLabel.Text = ((TextCell)tvc.Cell).Text;
                tvc.TextLabel.SizeToFit();
            } else if (args.PropertyName == TextCell.DetailProperty.PropertyName) {
                tvc.DetailTextLabel.Text = ((TextCell)tvc.Cell).Detail;
                tvc.DetailTextLabel.SizeToFit();
            } else if (args.PropertyName == TextCell.TextColorProperty.PropertyName)
                tvc.TextLabel.TextColor = textCell.TextColor.ToUIColor(DefaultTextColor);
            else if (args.PropertyName == TextCell.DetailColorProperty.PropertyName)
                tvc.DetailTextLabel.TextColor = textCell.DetailColor.ToUIColor(DefaultTextColor);
            else if (args.PropertyName == Cell.IsEnabledProperty.PropertyName)
                UpdateIsEnabled(tvc, textCell);
        }

        static void UpdateIsEnabled(CellTableViewCell cell, TextCell entryCell) {
            cell.UserInteractionEnabled = entryCell.IsEnabled;
            cell.TextLabel.Enabled = entryCell.IsEnabled;
            cell.DetailTextLabel.Enabled = entryCell.IsEnabled;
        }
    }

    public class AttendanceListPage : ContentPage {
        public class AttendanceListCell : TextCell {
            public AttendanceListCell(string main, string sub) {
                Text = main;
                Detail = sub;
            }
        }

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
                Intent = TableIntent.Menu,
                HasUnevenRows = true
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
                new NSObject().InvokeOnMainThread(() => ((App)Application.Current).ShowLoader("Loading..."));
            _section.Clear();
            try {
                (await _getter()).ForEach(_section.Add);
            } catch {
                AlertHelper.ShowAlert("Error",
                    "An error occurred while loading data.  Try checking your internet connection.",
                    b => Navigation.PopAsync(), "OK");
            } finally {
                if (_shouldShowLoader)
                    new NSObject().InvokeOnMainThread(() => ((App)Application.Current).HideLoader());
            }
        }

        /*private static ViewCell CreateTextCell(string text, Action tapped) => new ViewCell {
            View = new Label {
                Text = text,
                LineBreakMode = LineBreakMode.WordWrap,
                MinimumHeightRequest = 40,
                //Margin = new Thickness(10)
            }
        }.Manipulate(c => {
            c.Tapped += (s, e) => tapped();
            return c;
        });*/

        private static Cell CreateTextCell(string text, string detail, Action tapped) => new AttendanceListCell(text, detail).Manipulate(c => {
            c.Tapped += (s, e) => tapped();
            return c;
        });

        private static int IntFromWord(string w) {
            switch (w.ToLower()) {
                case "one":
                    return 1;
                case "two":
                    return 2;
                case "three":
                    return 3;
                case "four":
                    return 4;
                case "five":
                    return 5;
            }
            return -1;
        }

        public static AttendanceListPage CreateMain(INavigation navigation) => new AttendanceListPage("Attendance Manager", () => Task.FromResult(new List<Cell> {
                CreateTextCell("Junior High", "", () => navigation.PushAsync(CreateGroupsList(navigation, GroupType.JuniorHigh))),
                CreateTextCell("High School", "", () => navigation.PushAsync(CreateGroupsList(navigation, GroupType.HighSchool))),
                CreateTextCell("Merge Groups", "", () => navigation.PushAsync(CreateMergeGroupsList(navigation)))}), false);

        public static AttendanceListPage CreateGroupsList(INavigation navigation, GroupType type) => new AttendanceListPage("Select Group", async () => {
            return (await MergeDatabase.ListAsync<AttendanceGroup>()).Where(g => type == GroupType.JuniorHigh
                ? (int)g.GradeLevel <= 8
                : (int)g.GradeLevel >= 9).OrderBy(g => (int)g.GradeLevel).ThenBy(g => IntFromWord(g.LeaderNames.ElementAt(0).Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries).Last())).Select(g => CreateTextCell(g.Summary, g.Id, () => navigation.PushAsync(CreateRecordsList(navigation, g)))).Cast<Cell>().ToList();
            ;
        }, true);

        public static AttendanceListPage CreateMergeGroupsList(INavigation navigation) => new AttendanceListPage("Select Merge Group", async () => {
            return (await MergeDatabase.ListAsync<MergeGroup>()).OrderBy(g => g.Id).Select(g => CreateTextCell(g.Name, g.Id, () => navigation.PushAsync(CreateMergeGroupRecordsList(navigation, g)))).Cast<Cell>().ToList();
            ;
        }, true);

        public static AttendanceListPage CreateRecordsList(INavigation navigation, AttendanceGroup group) => new AttendanceListPage("Records", async () => {
            var addCell = new ViewCell {
                View = new Button {
                    Text = "Add Record",
                    Command = new Command(() => navigation.PushAsync(new RecordEditorPage(group, null)))
                }
            };
            var editCell = new ViewCell {
                View = new Button {
                    Text = "Edit Group",
                    Command = new Command(
                        () => navigation.PushAsync(new GroupEditorPage(group)))
                }
            };
            var cells = (await MergeDatabase.ListAsync<AttendanceRecord>()).Where(r => r.GroupId == group.Id)
                .OrderByDescending(r => r.Date).Select(r => CreateTextCell(r.Date.ToLongDateString(), $"{r.Students.Count} students (leaders {(r.LeadersPresent ? "" : "not ")}present)", () => navigation.PushAsync(new RecordEditorPage(group, r)))).Cast<Cell>().ToList();
            cells.Insert(0, addCell);
            cells.Insert(0, editCell);
            return cells;
        }, true);

        public static AttendanceListPage CreateMergeGroupRecordsList(INavigation navigation, MergeGroup group) => new AttendanceListPage("Records", async () => {
            var addCell = new ViewCell {
                View = new Button {
                    Text = "Add Record",
                    Command = new Command(() => navigation.PushAsync(new MergeGroupRecordEditorPage(group, null)))
                }
            };
            var cells = (await MergeDatabase.ListAsync<MergeGroupAttendanceRecord>()).Where(r => r.MergeGroupId == group.Id)
                .OrderByDescending(r => r.Date).Select(r => CreateTextCell(r.Date.ToLongDateString(), $"{r.StudentCount} students (image {(!string.IsNullOrWhiteSpace(r.Image) ? "" : "not ")}uploaded)", () => navigation.PushAsync(new MergeGroupRecordEditorPage(group, r)))).Cast<Cell>().ToList();
            cells.Insert(0, addCell);
            return cells;
        }, true);
    }
}
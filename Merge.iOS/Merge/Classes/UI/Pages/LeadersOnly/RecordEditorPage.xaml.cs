#region LICENSE

// Project Merge.iOS:  RecordEditorPage.xaml.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/14/2017 at 9:18 PM.
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
using Merge.iOS.Helpers;
using MergeApi.Client;
using MergeApi.Models.Core.Attendance;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.Xaml;

#endregion

namespace Merge.Classes.UI.Pages.LeadersOnly {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecordEditorPage : ContentPage {
        private AttendanceGroup _group;

        private List<string> _names;
        private AttendanceRecord _record;

        private RecordEditorPage() {
            InitializeComponent();
        }

        public RecordEditorPage(AttendanceGroup group, AttendanceRecord record) : this() {
            _group = group;
            _record = record;
            _names = new List<string>();
            var checkedNames = new List<string>();
            var enable = true;
            groupId.Text = $"{group.Summary} ({group.Id})";
            if (record != null) {
                enable = false;
                addStudentButton.IsVisible = false;
                leadersPresent.IsEnabled = false;
                leadersPresent.IsToggled = record.LeadersPresent;
                checkedNames = record.Students;
                date.Text = record.Date.ToLongDateString();
            } else {
                date.Text = DateTime.Now.ToLongDateString();
            }
            foreach (var name in group.StudentNames)
                AddStudentToList(name, checkedNames.Contains(name), enable);
            foreach (var name in checkedNames.Where(n => !group.StudentNames.Contains(n)))
                AddStudentToList(name, true, enable);
			this.AddToolbarItem("Close", Images.Dismiss, async (s, e) => {
				if (enable)
					AlertHelper.ShowSheet(null, async b => {
						if (b == "Save")
							await SaveAndExit();
						else if (b == "Don't Save")
							await Navigation.PopModalAsync();
				}, "Cancel", "Don't Save", ((ToolbarItem)s).ToUIBarButtonItem(), "Save");
				else
					await Navigation.PopModalAsync();
			});
            if (enable)
                ToolbarItems.Add(new ToolbarItem("Save", Images.Save, async () => await SaveAndExit()));
        }

        private async Task SaveAndExit() {
            new NSObject().InvokeOnMainThread(() => ((App) Application.Current).ShowLoader("Saving changes..."));
            _group.StudentNames = _names;
            _record = new AttendanceRecord {
                GroupId = _group.Id,
                Date = DateTime.Parse(date.Text),
                LeadersPresent = leadersPresent.IsToggled,
                Students = GetStudents().Where(IsStudentChecked).ToList()
            };
            await MergeDatabase.UpdateAsync(_group);
            await MergeDatabase.UpdateAsync(_record);
            new NSObject().InvokeOnMainThread(((App) Application.Current).HideLoader);
            await Navigation.PopModalAsync();
        }

        private List<string> GetStudents() {
            return studentsList.Children.OfType<StackLayout>().Select(layout => ((Label) layout.Children[0]).Text)
                .ToList();
        }

        private bool IsStudentChecked(string name) {
            return (from layout in studentsList.Children.OfType<StackLayout>()
                where ((Label) layout.Children[0]).Text == name
                select ((Switch) layout.Children[1]).IsToggled).FirstOrDefault();
        }

        private void AddStudentToList(string name, bool check, bool enable) {
            _names.Add(name);
            studentsList.Children.Add(new StackLayout {
                Margin = new Thickness(10, 0, 0, 0),
                Orientation = StackOrientation.Horizontal,
                Spacing = 5,
                Children = {
                    new Label {
                        Text = name,
                        TextColor = Color.Black,
                        FontSize = 14d,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.FillAndExpand
                    },
                    new Switch {
                        IsEnabled = enable,
                        IsToggled = check
                    }
                }
            });
        }

        private void AddStudent(object sender, EventArgs e) {
            AlertHelper.ShowTextInputAlert("Add Student", "Type the student's name then tap 'Add'.", false, f => { },
                (b, i) => {
                    if (b == "Add")
                        AddStudentToList(i, true, true);
                }, "Add", "Cancel");
        }
    }
}
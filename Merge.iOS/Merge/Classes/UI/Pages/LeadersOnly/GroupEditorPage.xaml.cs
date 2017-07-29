#region LICENSE

// Project Merge.iOS:  GroupEditorPage.xaml.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 07/14/2017 at 8:54 PM.
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
    public partial class GroupEditorPage : ContentPage {
        private AttendanceGroup _group;

        private List<string> _students;

        public GroupEditorPage(AttendanceGroup group) {
            InitializeComponent();
            _students = new List<string>();
            _group = group;
            groupId.Text = $"{_group.Summary} ({_group.Id})";
            _group.StudentNames.ForEach(AddStudentToList);
			this.AddToolbarItem("Close", Images.Dismiss, (s, e) => {
				AlertHelper.ShowSheet(null, async b => {
					if (b == "Save")
						await SaveAndExit();
					else if (b == "Don't Save")
						await Navigation.PopModalAsync();
				}, "Cancel", "Don't Save", ((ToolbarItem)s).ToUIBarButtonItem(), "Save");
			});
            ToolbarItems.Add(new ToolbarItem("Save", Images.Save, async () => await SaveAndExit()));
        }

        private void AddStudentToList(string name) {
            _students.Add(name);
            studentsList.Children.Add(new StackLayout {
                Margin = new Thickness(10, 0, 10, 0),
                Spacing = 5,
                Orientation = StackOrientation.Horizontal,
                Children = {
                    new Label {
                        TextColor = Color.Black,
                        Text = name,
                        FontSize = 14d,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.Center
                    },
                    new Button {
                        Text = "Remove",
                        TextColor = Color.Red,
                        FontSize = 14d,
                        Command = new Command(() => {
                            studentsList.Children.RemoveAt(_students.IndexOf(name));
                            _students.Remove(name);
                        })
                    }
                }
            });
        }

        private async Task SaveAndExit() {
            new NSObject().InvokeOnMainThread(() => ((App) Application.Current).ShowLoader("Saving changes..."));
            _group.StudentNames = _students;
            await MergeDatabase.UpdateAsync(_group);
            new NSObject().InvokeOnMainThread(((App) Application.Current).HideLoader);
            await Navigation.PopModalAsync();
        }

        private void AddStudent(object sender, EventArgs e) {
            AlertHelper.ShowTextInputAlert("Add Student", "Type the student's name then tap 'Add'.", false, f => { },
                (b, i) => {
                    if (b == "Add")
                        AddStudentToList(i);
                }, "Add", "Cancel");
        }
    }
}
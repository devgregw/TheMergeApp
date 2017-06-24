#region LICENSE

// Project Merge Data Utility:  AnnouncementEditorPage.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 03/20/2017 at 6:42 PM.
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

#endregion

namespace Merge_Data_Utility.UI.Pages.Editors {
    /// <summary>
    ///     Interaction logic for AnnouncementEditorPage.xaml
    /// </summary>
    /*public partial class AnnouncementEditorPage : EditorPage {
        public AnnouncementEditorPage() {
            InitializeComponent();
        }

        public AnnouncementEditorPage(MergeAnnouncement source, bool draft) : this() {
            SetSource(source, draft);
        }

        protected override InputValidationResult ValidateInput() {
            var errors = new List<string>();
            errors.Add(string.IsNullOrWhiteSpace(titleBox.Text) ? "The title is invalid." : "");
            errors.Add(string.IsNullOrWhiteSpace(sdescBox.Text) ? "The short description is invalid." : "");
            errors.Add(string.IsNullOrWhiteSpace(descBox.Text) ? "The description is invalid." : "");
            errors.Add(gradesField.Value.Count == 0 ? "At least one grade level must be selected." : "");
            errors.Add(gendersField.Value.Count == 0 ? "At least one gender must be selected." : "");
            if (buttonCheck.IsChecked.GetValueOrDefault(false)) {
                errors.Add(string.IsNullOrWhiteSpace(buttonLabel.Text) ? "No button label specified." : "");
                errors.Add(buttonAction.SelectedAction == null ? "No action selected." : "");
            }
            if (GetSource<MergeAnnouncement>() == null ||
                string.IsNullOrWhiteSpace(GetSource<MergeAnnouncement>().CoverImage))
                errors.Add(string.IsNullOrWhiteSpace(coverField.Value) ? "No cover image selected." : "");
            errors.Add(!colorField.SelectedColor.HasValue ? "No color selected." : "");
            errors.RemoveAll(string.IsNullOrWhiteSpace);
            return new InputValidationResult(errors);
        }

        protected override async Task<object> MakeObject() {
            var nsrc = new MergeAnnouncement {
                Id = idField.Id,
                Title = titleBox.Text,
                ShortDescription = sdescBox.Text,
                Description = descBox.Text,
                GradeLevels = gradesField.Value,
                Genders = gendersField.Value,
                Color = colorField.SelectedColor?.ToHex(),
                Theme = themeField.SelectedTheme,
                ButtonLabel = buttonCheck.IsChecked.GetValueOrDefault(false) ? buttonLabel.Text : "",
                ButtonAction = buttonCheck.IsChecked.GetValueOrDefault(false) ? buttonAction.SelectedAction : null,
                Importance = ((ComboBoxItem) importance.SelectedItem).Content.ToString().ToEnum<Importance>(true)
            };
            if (IsDraft) // don't upload the file yet!  this is just a draft!
                nsrc.CoverImage = coverField.Value;
            else nsrc.CoverImage = await coverField.PerformChangesAsync($"Announcement{idField.Id}");
            return nsrc;
        }

        public override string GetIdentifier() {
            return $"announcements/{idField.Id}{(IsDraft ? " (draft)" : "")}";
        }

        public override async Task<bool> Publish() {
            IsDraft = false;
            var res = ValidateInput();
            if (!res.IsInputValid) {
                res.Display(Window);
            } else {
                var reference = GetLoaderReference();
                reference.StartLoading("Processing...");
                var o = await MakeObject();
                DraftManager.AutoDelete((IIdentifiable) o);
                var resp = await MergeDatabase.UpdateAnnouncementAsync((MergeAnnouncement) o);
                if (!resp.HasErrors)
                    return true;
                var error = resp.Errors[0];
                MessageBox.Show(Window,
                    $"Could not update announcements/{error.ObjectId} ({error.ObjectType.FullName}):\n{error.Cause.Message}\n{error.Cause.GetType().FullName}");
                return true;
            }
            return false;
        }

        public override async Task SaveAsDraft() {
            IsDraft = true;
            var o = await MakeObject();
            DraftManager.AddDraftedAnnouncement((MergeAnnouncement) o);
        }

        protected override void Update() {
            idField.Regenerated += (s, e) => { UpdateTitle(); };
            colorField.Prepare(previewField, coverField);
            themeField.Prepare(previewField);
            if (HasSource) {
                var src = GetSource<MergeAnnouncement>();
                idField.SetId(src.Id, false);
                titleBox.Text = src.Title;
                sdescBox.Text = src.ShortDescription;
                descBox.Text = src.Description;
                gradesField.Value = src.GradeLevels;
                gendersField.Value = src.Genders;
                coverField.Value = src.CoverImage;
                colorField.SelectedColor = src.Color?.ToColor();
                themeField.SelectedTheme = src.Theme;
                importance.SelectedIndex = importance.Items.Cast<ComboBoxItem>()
                    .Select(i => i.Content.ToString().ToEnum<Importance>(true))
                    .ToList()
                    .IndexOf(src.Importance);
                if (string.IsNullOrWhiteSpace(src.ButtonLabel) || src.ButtonAction == null) return;
                buttonCheck.IsChecked = true;
                buttonLabel.Text = src.ButtonLabel;
                buttonAction.DefaultAction = src.ButtonAction;
                buttonAction.Reset();
            } else {
                idField.SetId("", true);
                themeField.SelectedTheme = Theme.Auto;
                importance.SelectedIndex = 0;
            }
        }

        private void ButtonEnabled(object sender, RoutedEventArgs e) {
            buttonLabel.IsEnabled = true;
            buttonAction.IsEnabled = true;
        }

        private void ButtonDisabled(object sender, RoutedEventArgs e) {
            buttonLabel.IsEnabled = false;
            buttonAction.IsEnabled = false;
        }
    }
    */
}
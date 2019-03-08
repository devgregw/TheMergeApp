#region LICENSE

// Project Merge Data Utility:  ModelBaseFieldCollectionControl.xaml.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 06/23/2017 at 10:45 AM.
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
using System.Windows;
using System.Windows.Controls;
using MergeApi;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using Merge_Data_Utility.Tools;

#endregion

namespace Merge_Data_Utility.UI.Controls.EditorFields {
    /// <summary>
    ///     Interaction logic for ModelBaseFieldCollectionControl.xaml
    /// </summary>
    public partial class ModelBaseFieldCollectionControl : UserControl {
        public static DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(string),
            typeof(ModelBaseFieldCollectionControl), new PropertyMetadata("object",
                (o, args) => { ((ModelBaseFieldCollectionControl) o).UpdateType(); }));

        private ModelBase _source;

        public EventHandler<RegeneratedEventArgs> IdRegenerated;

        public ModelBaseFieldCollectionControl() {
            InitializeComponent();
        }

        public string Type {
            get => (string) GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public string Id => idField.Id;

        public string Title => titleBox.Text;

        private void UpdateType() {
            foreach (var header in ((StackPanel) Content).Children.OfType<FieldHeader>())
                header.Description = header.Description.Replace("${type}", Type);
        }

        public void Prepare(ModelBase src, bool isPage = false) {
            _source = src;
            colorField.Prepare(previewField, coverField);
            themeField.Prepare(previewField);
            idField.Regenerated += IdRegenerated;
            descHeader.Description += "  Pages must have either a description, content elements, or both.";
            if (src != null) {
                idField.SetId(src.Id, false);
                titleBox.Text = src.Title;
                sdescBox.Text = src.ShortDescription;
                descBox.Text = src.Description;
                gradesField.Value = src.GradeLevels;
                gendersField.Value = src.Genders;
                if (!string.IsNullOrWhiteSpace(src.CoverImage))
                    coverField.SetOriginalValue(src.CoverImage);
                colorField.SelectedColor = src.Color.ToColor();
                themeField.SelectedTheme = src.Theme;
            } else {
                idField.SetId(MergeDatabase.NewId(), true);
                themeField.SelectedTheme = Theme.Auto;
            }
            UpdateType();
        }

        public List<string> GetValidationErrors(bool descriptionOptional, bool page = false) {
            var errors = new List<string>();
            errors.Add(string.IsNullOrWhiteSpace(titleBox.Text) ? "The title is invalid." : "");
            errors.Add(string.IsNullOrWhiteSpace(sdescBox.Text) ? "The short description is invalid." : "");
            if (!descriptionOptional)
                errors.Add(string.IsNullOrWhiteSpace(descBox.Text)
                    ? page
                        ? "A description is required if no content elements are configured."
                        : "The description is invalid."
                    : "");
            errors.Add(gradesField.Value.Count == 0 ? "At least one grade level must be selected." : "");
            errors.Add(gendersField.Value.Count == 0 ? "At least one gender must be selected." : "");
            if (string.IsNullOrWhiteSpace(_source?.CoverImage))
                errors.Add(string.IsNullOrWhiteSpace(coverField.Value) ? "No cover image selected." : "");
            errors.Add(!colorField.SelectedColor.HasValue ? "No color selected." : "");
            errors.RemoveAll(string.IsNullOrWhiteSpace);
            return errors;
        }

        public async Task ApplyToAsync(ModelBase x, bool draft) {
            x.Id = idField.Id;
            x.Title = titleBox.Text;
            x.ShortDescription = sdescBox.Text;
            x.Description = descBox.Text;
            x.GradeLevels = gradesField.Value;
            x.Genders = gendersField.Value;
            x.Color = colorField.SelectedColor?.ToHex();
            x.Theme = themeField.SelectedTheme;
            if (draft) // don't upload the file yet!  this is just a draft!
                x.CoverImage = coverField.Value;
            else
                x.CoverImage =
                    await coverField.PerformChangesAsync(
                        $"{x.GetType().Name.Replace("Merge", "")}_{idField.Id}".ToLower(), "");
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Merge_Data_Utility.UI.Windows;
using Xceed.Wpf.Toolkit;

namespace Merge_Data_Utility.UI.EditorStructure.Fields {
    public sealed class ColorChooserField : EditorField<Color?> {
        public ColorChooserField(EditorSchema parent, string title, string desc, string propName, ThemePreviewField preview,
            ImageUploadField image) : base(parent) {
            Title = title;
            Description = desc;
            PropertyName = propName;
            _preview = preview;
            _imageField = image;
        }

        private ThemePreviewField _preview;

        private ImageUploadField _imageField;

        public override string Title { get; }

        public override string Description { get; }

        public override string PropertyName { get; }

        private ColorPicker GetPicker() {
            return (ColorPicker) ((StackPanel) Element).Children[0];
        }

        private Color? GetSelectedColor() {
            return GetPicker().SelectedColor;
        }

        public override bool CheckInput() {
            return GetPicker().SelectedColor.HasValue;
        }

        public override UIElement Build() {
            var picker = new ColorPicker {
                UsingAlphaChannel = false,
                Width = 200
            };
            picker.SelectedColorChanged += (s, e) => _preview.Update(GetSelectedColor());
            var main = new StackPanel {
                HorizontalAlignment = HorizontalAlignment.Left,
                Children = {
                    picker
                }
            };
            if (_imageField != null) {
                var btn = new Button {
                    Content = "Get color from image..."
                };
                btn.Click += (s, e) => {
                    if (string.IsNullOrWhiteSpace(_imageField.RealValue))
                        System.Windows.MessageBox.Show("Choose an image first.", "Palette", MessageBoxButton.OK, MessageBoxImage.Error,
                            MessageBoxResult.OK);
                    var window = new PaletteWindow(_imageField.RealValue);
                    window.ShowDialog();
                    GetPicker().SelectedColor = window.ChosenColor ?? GetPicker().SelectedColor;
                };
                main.Children.Add(btn);
            }
            Element = main;
            return Element;
        }

        public override Color? RawValue {
            get { return GetPicker().SelectedColor; }
            set { GetPicker().SelectedColor = value; }
        }

        public override Color? RealValue {
            get { return RawValue; }
            set { RawValue = value; }
        }

    }
}

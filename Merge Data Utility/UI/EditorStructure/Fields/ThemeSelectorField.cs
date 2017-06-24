using System.Windows;
using System.Windows.Controls;
using MergeApi.Framework.Enumerations;

namespace Merge_Data_Utility.UI.EditorStructure.Fields {
    public sealed class ThemeSelectorField : EditorField<Theme> {
        public ThemeSelectorField(EditorSchema parent, string title, string desc, string propName, ThemePreviewField preview) : base(parent) {
            Title = title;
            Description = desc;
            PropertyName = propName;
            _preview = preview;
        }

        private ThemePreviewField _preview;

        public override string Title { get; }

        public override string Description { get; }

        public override string PropertyName { get; }

        private ComboBox GetBox() {
            return (ComboBox) Element;
        }

        public override bool CheckInput() {
            return GetBox().SelectedIndex >= 0;
        }

        private Theme GetSelectedField() {
            if (GetBox().SelectedIndex == -1)
                return Theme.Auto;
            return (Theme) ((ComboBoxItem) GetBox().SelectedItem).Tag;
        }

        public override UIElement Build() {
            var box = new ComboBox {
                Width = 200,
                Items = {
                    new ComboBoxItem {
                        Content = "Auto",
                        Tag = Theme.Auto
                    },
                    new ComboBoxItem {
                        Content = "Light",
                        Tag = Theme.Light
                    },
                    new ComboBoxItem {
                        Content = "Dark",
                        Tag = Theme.Dark
                    }
                },
                SelectedIndex = 0
            };
            box.SelectionChanged += (s, e) => _preview.Update(GetSelectedField());
            Element = box;
            return Element;
        }

        public override Theme RawValue {
            get { return (Theme) ((ComboBoxItem) GetBox().SelectedItem).Tag; }
            set { GetBox().SelectedIndex = value == Theme.Auto ? 0 : value == Theme.Light ? 1 : 2; }
        }

        public override Theme RealValue {
            get {
                return RawValue;
            }
            set { RawValue = value; }
        }
    }
}

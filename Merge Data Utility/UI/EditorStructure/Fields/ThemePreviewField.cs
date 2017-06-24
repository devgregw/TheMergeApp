using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MergeApi.Framework.Enumerations;

namespace Merge_Data_Utility.UI.EditorStructure.Fields {
    public sealed class ThemePreviewField : EditorField<object> {
        public ThemePreviewField(EditorSchema parent, string title, string desc) : base(parent) {
            Title = title;
            Description = desc;
        }

        private Color? _color = Colors.White;

        private Theme _theme = Theme.Auto;

        public override string Title { get; }

        public override string Description { get; }

        public override string PropertyName => null;

        public void Update(Color? newColor) {
            _color = newColor;
            GetGrid().Background = new SolidColorBrush(newColor.GetValueOrDefault(Colors.White));
        }

        public void Update(Theme newTheme) {
            _theme = newTheme;
            GetTextBlock().Foreground =
                new SolidColorBrush(_theme == Theme.Auto
                    ? GetContrastColor(_color.GetValueOrDefault(Colors.White))
                    : _theme == Theme.Light ? Colors.White : Colors.Black);
        }

        public override bool CheckInput() {
            return true;
        }


        private Color GetContrastColor(Color c) {
            var a = 1 - (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255;
            var d = a < 0.5 ? 0 : 255;
            return Color.FromArgb(255, (byte)d, (byte)d, (byte)d);
        }

        private Grid GetGrid() {
            return (Grid) Element;
        }

        private TextBlock GetTextBlock() {
            return (TextBlock)GetGrid().Children[1];
        }

        public override UIElement Build() {
            Element = new Grid {
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 100,
                Height = 100,
                Background = new SolidColorBrush(_color.GetValueOrDefault(Colors.White)),
                Children = {
                    new Border {
                        BorderThickness = new Thickness(1, 0, 0, 0),
                        BorderBrush = new SolidColorBrush(Colors.Black)
                    },
                    new TextBlock {
                        Text = "This text should be clearly visible.",
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(5, 0, 5, 0),
                        Foreground = new SolidColorBrush(_theme == Theme.Auto ? GetContrastColor(_color.GetValueOrDefault(Colors.White)) : _theme == Theme.Light ? Colors.White : Colors.Black)
                    }
                }
            };
            return Element;
        }

        public override void Apply(object instance) {
            
        }

        public override void SetExistingValue(object instance) {
            
        }

        public override object RawValue { get; set; }

        public override object RealValue { get; set; }
    }
}

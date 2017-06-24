using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Merge_Data_Utility.UI.EditorStructure.Fields {
    public sealed class AddressField : EditorField<string> {
        private readonly Dictionary<string, string> _presets = new Dictionary<string, string> {
            { "Main/Multipurpose/Student Building(s)", "8001 Anderson Blvd, Fort Worth Texas, 76120" },
            { "The Connection", "8200 Anderson Blvd, Fort Worth Texas, 76120" }
        };

        public AddressField(EditorSchema parent, string title, string desc, string propName) : base(parent) {
            Title = title;
            Description = desc;
            PropertyName = propName;
        }

        public override string Title { get; }

        public override string Description { get; }

        public override string PropertyName { get; }

        public override bool CheckInput() {
            return RealValue.Length == 0 || !string.IsNullOrWhiteSpace(RealValue);
        }

        private TextBox GetTextBox() {
            return (TextBox) ((StackPanel) Element).Children[1];
        }

        public override UIElement Build() {
            var box = new TextBox {
                Width = 250,
                HorizontalAlignment = HorizontalAlignment.Left,
                InputScope = new InputScope {
                    Names = { InputScopeNameValue.PostalAddress }
                }
            };
            var buttonPanel = new StackPanel {
                Orientation = Orientation.Horizontal
            };
            foreach (var p in _presets) {
                var button = new Button {
                    Margin = new Thickness(0, 0, 5, 0),
                    Content = p.Key,
                    Tag = p.Value
                };
                button.Click += (s, e) => RealValue = button.Tag.ToString();
                buttonPanel.Children.Add(button);
            }
            SpellCheck.SetIsEnabled((TextBox)Element, true);
            Element = new StackPanel {
                HorizontalAlignment = HorizontalAlignment.Left,
                Children = {
                    buttonPanel,
                    box
                }
            };
            return Element;
        }

        public override string RawValue {
            get { return GetTextBox().Text; }
            set { GetTextBox().Text = value; }
        }

        public override string RealValue {
            get { return RawValue; }
            set { RawValue = value; }
        }
    }
}

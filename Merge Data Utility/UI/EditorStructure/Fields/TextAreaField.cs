using System.Windows;
using System.Windows.Controls;

namespace Merge_Data_Utility.UI.EditorStructure.Fields {
    public sealed class TextAreaField : EditorField<string> {
        public TextAreaField(EditorSchema parent, string title, string desc, string propName, int width, int height,
            bool allowEnter, bool spellCheck, bool readOnly) : base(parent) {
            _width = width;
            _height = height;
            _return = allowEnter;
            _spellCheck = spellCheck;
            _readOnly = readOnly;
        }

        private int _width, _height;

        private bool _return, _spellCheck, _readOnly;

        public override string Title { get; }

        public override string Description { get; }

        public override string PropertyName { get; }

        public override bool CheckInput() {
            return !string.IsNullOrWhiteSpace(RealValue);
        }

        public override UIElement Build() {
            Element = new TextBox {
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = _width,
                Height = _height,
                AcceptsReturn = _return,
                AcceptsTab = false,
                IsReadOnly = _readOnly,
                TextWrapping = TextWrapping.Wrap
            };
            SpellCheck.SetIsEnabled((TextBox)Element, _spellCheck);
            return Element;
        }

        public override string RawValue {
            get { return ((TextBox) Element).Text; }
            set { ((TextBox) Element).Text = value; }
        }

        public override string RealValue {
            get { return RawValue; }
            set { RawValue = value; }
        }
    }
}

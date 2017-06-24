using System.Windows;
using System.Windows.Controls;

namespace Merge_Data_Utility.UI.EditorStructure.Fields {
    public sealed class TextField : EditorField<string> {
        public TextField(EditorSchema parent, string title, string desc, string propName, int width, bool spellCheck, bool readOnly) : base(parent) {
            Title = title;
            Description = desc;
            PropertyName = propName;
            _width = width;
            _spellCheck = spellCheck;
            _readOnly = readOnly;
        }

        private readonly int _width;

        private readonly bool _spellCheck, _readOnly;

        public override string Title { get; }

        public override string Description { get; }

        public override string PropertyName { get; }

        public override bool CheckInput() {
            return !string.IsNullOrWhiteSpace(RealValue);
        }

        public override UIElement Build() {
            Element = new TextBox {
                Width = _width,
                HorizontalAlignment = HorizontalAlignment.Left,
                IsReadOnly = _readOnly,
                AcceptsTab = false,
                AcceptsReturn = false
            };
            SpellCheck.SetIsEnabled((TextBox)Element, _spellCheck);
            return Element;
        }

        public override string RawValue {
            get {
                return ((TextBox)Element).Text;
            }
            set { ((TextBox) Element).Text = value; }
        }

        public override string RealValue {
            get {
                return RawValue;
            }
            set { RawValue = value; }
        }
    }
}

using System.Windows;
using System.Windows.Controls;

namespace Merge_Data_Utility.UI.EditorStructure.Fields {
    public sealed class CheckBoxField : EditorField<bool?, bool> {
        public CheckBoxField(EditorSchema parent, string title, string desc, string propName, string checkBoxLabel)
            : base(parent) {
            Title = title;
            Description = desc;
            PropertyName = propName;
            _label = checkBoxLabel;
        }

        private string _label;

        public override string Title { get; }

        public override string Description { get; }
        
        public override string PropertyName { get; }

        private CheckBox GetBox() {
            return (CheckBox) Element;
        }

        public override bool CheckInput() {
            return true;
        }

        public override UIElement Build() {
            Element = new CheckBox {
                Content = _label
            };
            return Element;
        }

        public override bool? RawValue {
            get { return GetBox().IsChecked; }
            set {
                GetBox().IsChecked = value;
            }
        }

        public override bool RealValue {
            get {
                return RawValue.GetValueOrDefault(false);
            }
            set { RawValue = value; }
        }
    }
}

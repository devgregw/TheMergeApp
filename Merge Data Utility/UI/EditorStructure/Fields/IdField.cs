using System.Windows;
using System.Windows.Controls;

namespace Merge_Data_Utility.UI.EditorStructure.Fields {
    public sealed class IdField : EditorField<string> {
        private readonly string _id;

        public IdField(EditorSchema parent) : base(parent) {}

        public override string Title => "ID";

        public override string Description => "This is a unique identifier for this object.  It cannot be changed.";

        public override string PropertyName => "Id";

        public override bool CheckInput() {
            return true;
        }

        public override UIElement Build() {
            Element = new TextBox {
                IsReadOnly = true,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 100,
                Text = _id
            };
            return Element;
        }

        public override string RawValue {
            get {
                return ((TextBox)Element).Text;
            }
            set { ((TextBox) Element).Text = value; }
        }

        public override string RealValue {
            get { return RawValue; }
            set { RawValue = value; }
        }
    }
}

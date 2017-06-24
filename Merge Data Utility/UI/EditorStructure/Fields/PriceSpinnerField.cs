using System.Windows;
using Xceed.Wpf.Toolkit;

namespace Merge_Data_Utility.UI.EditorStructure.Fields {
    public sealed class PriceSpinnerField : EditorField<double?> {
        public PriceSpinnerField(EditorSchema parent, string title, string desc, string propName) : base(parent) {
            Title = title;
            Description = desc;
            PropertyName = propName;
        }

        public override string Title { get; }

        public override string Description { get; }

        public override string PropertyName { get; }

        private DoubleUpDown GetSpinner() {
            return (DoubleUpDown) Element;
        }

        public override bool CheckInput() {
            return GetSpinner().Value.HasValue;
        }

        public override UIElement Build() {
            Element = new DoubleUpDown {
                Width = 100,
                Minimum = 0,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            return Element;
        }

        public override double? RawValue {
            get { return GetSpinner().Value; }
            set { GetSpinner().Value = value; }
        }

        public override double? RealValue {
            get {
                return RawValue;
            }
            set { RawValue = value; }
        }
    }
}

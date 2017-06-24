using System;
using System.Windows;
using Xceed.Wpf.Toolkit;

namespace Merge_Data_Utility.UI.EditorStructure.Fields {
    public sealed class DateTimeField : EditorField<DateTime?> {
        public DateTimeField(EditorSchema parent, string title, string desc, string propName) : base(parent) {
            Title = title;
            Description = desc;
            PropertyName = propName;
        }

        public override string Title { get; }

        public override string Description { get; }

        public override string PropertyName { get; }

        public override bool CheckInput() {
            return RawValue.HasValue;
        }

        private DateTimePicker GetPicker() {
            return (DateTimePicker) Element;
        }

        public override UIElement Build() {
            Element = new DateTimePicker {
                FormatString = "dddd, MMMM dd, yyyy h:mm tt",
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 300,
                Format = DateTimeFormat.Custom
            };
            return Element;
        }

        public override DateTime? RawValue {
            get { return GetPicker().Value; }
            set { GetPicker().Value = value; }
        }

        public override DateTime? RealValue {
            get {
                return RawValue;
            }
            set { RawValue = value; }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Framework.Enumerations;
using Xceed.Wpf.Toolkit;

namespace Merge_Data_Utility.UI.EditorStructure.Fields {
    public sealed class GenderSelectorField : EditorField<IList<bool>, List<Gender>> {
        public GenderSelectorField(EditorSchema parent, string propName, bool allowNone) : base(parent) {
            _allowNone = allowNone;
            PropertyName = propName;
        }

        private bool _allowNone;

        public override string Title => "Genders";

        public override string Description
            =>
                "Choose the genders that this object is intended for.    If a user has enabled filters and is outside of the target audience, they will not see this."
        ;
        public override string PropertyName { get; }

        private TextBlock MakeTextBlock(Gender g) {
            return new TextBlock { Text = g.ToString() };
        }

        private Button MakeButton(string text, IList<bool> values) {
            var b = new Button {
                Content = text
            };
            b.Click += (s, e) => SetSelection(values);
            return b;
        }

        public override bool CheckInput() {
            // return true if an empty selection is allowed or true if at least one item was selected
            // false if the selection is empty and empty selection is prohibited
            return _allowNone || RawValue.Any();
        }

        private void SetSelection(IList<bool> values) {
            // values in form { male, female }
            GetList().SelectedItems.Clear();
            for (var i = 0; i < values.Count; i++) {
                var item = values[i];
                if (!item) continue;
                var tb = MakeTextBlock(i == 0 ? Gender.Male : Gender.Female); // see comment under RealValue below
                if (GetList().SelectedItems.Contains(tb))
                    GetList().SelectedItems.Add(tb);
            }
        }

        private CheckListBox GetList() {
            return (CheckListBox)((StackPanel)Element).Children[0];
        }

        public override UIElement Build() {
            var box = new CheckListBox {
                HorizontalAlignment = HorizontalAlignment.Left
            };
            foreach (var g in EnumConsts.AllGenders)
                box.Items.Add(MakeTextBlock(g));
            Element = new StackPanel {
                Orientation = Orientation.Horizontal,
                Children = {
                    box,
                    new StackPanel {
                        Margin = new Thickness(5, 0, 0, 0),
                        Children = {
                            MakeButton("Select All", new[] {true, true}),
                            MakeButton("Select None", new[] {false, false})
                        }
                    }
                }
            };
            return Element;
        }

        public override IList<bool> RawValue {
            get {
                return EnumConsts.AllGenders.Select(g => GetList().SelectedItems.Contains(MakeTextBlock(g))).ToList();
            }
            set { SetSelection(value); }
        }

        public override List<Gender> RealValue {
            get {
                var genders = new List<Gender>();
                for (var i = 0; i < RawValue.Count; i++)
                    if (RawValue[i])
                        genders.Add(i == 0 ? Gender.Male : Gender.Female); // a 2 element array with indices 0 to 1
                return genders;
            }
            set {
                var bools = new List<bool> { false, false };
                foreach (var g in value)
                    bools[g == Gender.Male ? 0 : 1] = true;
                RawValue = bools;
            }
        }
    }
}

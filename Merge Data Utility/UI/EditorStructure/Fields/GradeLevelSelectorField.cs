using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Enumerations.Converters;
using Xceed.Wpf.Toolkit;

namespace Merge_Data_Utility.UI.EditorStructure.Fields {
    public sealed class GradeLevelSelectorField : EditorField<IList<bool>, List<GradeLevel>> {
        private readonly bool _allowNone;

        public GradeLevelSelectorField(EditorSchema parent, string propName, bool allowNone) : base(parent) {
            PropertyName = propName;
            _allowNone = allowNone;
        }

        public override string Title => "Grade Levels";

        public override string Description
            =>
                "Choose the grade levels that this object is intended for.  If a user has enabled filters and is outside of the target audience, they will not see this."
        ;

        public override string PropertyName { get; }

        private TextBlock MakeTextBlock(GradeLevel l) {
            return new TextBlock {Text = l.ToString()};
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
            // values in form { seventh, eighth, ninth, tenth, eleventh, twelfth }
            GetList().SelectedItems.Clear();
            for (var i = 0; i < values.Count; i++) {
                var item = values[i];
                if (!item) continue;
                var tb = MakeTextBlock(GradeLevelConverter.FromInt32(i + 7)); // see comment under RealValue below
                if (GetList().SelectedItems.Contains(tb))
                    GetList().SelectedItems.Add(tb);
            }
        }

        private CheckListBox GetList() {
            return (CheckListBox) ((StackPanel) Element).Children[0];
        }

        public override UIElement Build() {
            var box = new CheckListBox {
                HorizontalAlignment = HorizontalAlignment.Left
            };
            foreach (var l in EnumConsts.AllGradeLevels)
                box.Items.Add(MakeTextBlock(l));
            Element = new StackPanel {
                Orientation = Orientation.Horizontal,
                Children = {
                    box,
                    new StackPanel {
                        Margin = new Thickness(5, 0, 0, 0),
                        Children = {
                            MakeButton("Select All", new[] {true, true, true, true, true, true}),
                            MakeButton("Select None", new[] {false, false, false, false, false, false}),
                            new Separator(),
                            MakeButton("Select Junior High", new[] {true, true, false, false, false, false}),
                            MakeButton("Select High School", new[] {false, false, true, true, true, true}),
                            new Separator(),
                            MakeButton("Select Underclassmen", new[] {false, false, true, true, false, false}),
                            MakeButton("Select Upperclassmen", new[] {false, false, false, false, true, true})
                        }
                    }
                }
            };
            return Element;
        }

        public override IList<bool> RawValue {
            get {
                return EnumConsts.AllGradeLevels.Select(g => GetList().SelectedItems.Contains(MakeTextBlock(g))).ToList();
            }
            set { SetSelection(value); }
        }

        public override List<GradeLevel> RealValue {
            get {
                var grades = new List<GradeLevel>();
                for (var i = 0; i < RawValue.Count; i++)
                    if (RawValue[i])
                        grades.Add(GradeLevelConverter.FromInt32(i + 7)); // a 6 element array with indices 0 to 5, but FromInt32 takes 7 to 12.  So we add 7 (0 + 7 = 7th grade and 5 + 7 = 12th grade)
                return grades;
            }
            set {
                var bools = new List<bool> {false, false, false, false, false, false};
                foreach (var g in value)
                    bools[GradeLevelConverter.ToInt32(g) - 7] = true; // see above, this is the opposite (7th grade - 7 = index 0 and 12th grade - 7 = index 5)
                RawValue = bools;
            }
        }
    }
}

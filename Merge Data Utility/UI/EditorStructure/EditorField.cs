using System.Reflection;
using System.Windows;

namespace Merge_Data_Utility.UI.EditorStructure {
    public abstract class EditorField {
        public EditorSchema ParentSchema { get; }

        public abstract string Title { get; }

        public abstract string Description { get; }

        public abstract string PropertyName { get; }

        public abstract bool CheckInput();

        protected UIElement Element { get; set; }

        public abstract UIElement Build();

        public EditorField<TBoth> MakeApplicable<TBoth>() {
            return (EditorField<TBoth>) this;
        }

        public EditorField<TRaw, TReal> MakeApplicable<TRaw, TReal>() {
            return (EditorField<TRaw, TReal>) this;
        }

        public EditorField(EditorSchema parent) {
            ParentSchema = parent;
        }
    }

    public abstract class EditorField<TRaw, TReal> : EditorField {
        public abstract TRaw RawValue { get; set; }

        public abstract TReal RealValue { get; set; }

        public virtual void Apply(object instance) {
            ParentSchema.Type.GetTypeInfo().GetProperty(PropertyName).SetValue(instance, RealValue);
        }

        public virtual void SetExistingValue(object instance) {
            RealValue = (TReal)ParentSchema.Type.GetTypeInfo().GetProperty(PropertyName).GetValue(instance);
        }

        public EditorField(EditorSchema parent) : base(parent) {
            
        }
    }

    public abstract class EditorField<TBoth> : EditorField<TBoth, TBoth> {
        public EditorField(EditorSchema parent) : base(parent) {
        }
    }
}

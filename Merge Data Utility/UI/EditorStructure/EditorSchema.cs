using System;
using System.Collections.Generic;

namespace Merge_Data_Utility.UI.EditorStructure {
    public sealed class EditorSchema {
        public Type Type { get; }

        public List<EditorField> Fields { get; }
    }
}

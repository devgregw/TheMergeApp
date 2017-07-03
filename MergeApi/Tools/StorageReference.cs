using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergeApi.Tools {
    public sealed class StorageReference {
        public string Url => $"https://merge.devgregw.com/content/{Name}";

        public string Name { get; private set; }

        internal StorageReference(string name) {
            Name = name;
        }
    }
}

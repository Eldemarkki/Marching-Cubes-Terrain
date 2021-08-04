#if DEVELOPMENT_BUILD || UNITY_EDITOR
#define DEBUG
#endif

#if DEBUG

using System;

namespace Eldemarkki.VoxelTerrain.Debugging
{
    public class DebugProperty
    {
        public string Label { get; private set; }
        public Func<string> GetValue { get; private set; }

        public DebugProperty(string label, Func<string> getValue)
        {
            Label = label;
            GetValue = getValue;
        }

        public DebugProperty(string label, Func<int> getValue) : this(label, () => getValue().ToString()) { }

        public override string ToString()
        {
            return Label + ": " + GetValue();
        }
    }
}
#endif
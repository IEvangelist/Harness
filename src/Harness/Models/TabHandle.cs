using IEvangelist.Harness.Interfaces;

namespace IEvangelist.Harness.Models
{
    public class TabHandle : ITabHandle
    {
        public string Handle { get; set; }

        public override bool Equals(object obj) => Equals(obj as TabHandle);

        protected bool Equals(TabHandle other) => !(other is null) && Handle == other.Handle;

        public override int GetHashCode() => Handle?.GetHashCode() ?? 0;
    }
}
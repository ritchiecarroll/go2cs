// The part of go.lib that src/Moves.cs is compiled against: the two attributes go2cs-gen writes on promoted-method
// forwarders and their classes, the attribute of a method holder, the value-clone interface, the heap box and the variadic pack. Built as its own assembly and not committed: no test reads it.
namespace go
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class GoPromotedHostAttribute : System.Attribute
    {
        public GoPromotedHostAttribute(System.Type @struct) { }
    }

    public sealed class GoPromotedAttribute : System.Attribute { }

    // The mark of a receiver type's method holder, which the converter writes in package_info.cs.
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class GoMethodHostAttribute : System.Attribute
    {
        public GoMethodHostAttribute(System.Type receiver) { }
    }

    // The mark of a struct whose value copy needs a field-precise clone; go2cs-gen writes its Delta-named Clone().
    public interface IGoValueClone : System.ICloneable { }

    // golib's heap box, named as golib names it (U+0436).
    public sealed class \u0436<T>
    {
        public T Value;
    }
    // golib's variadic pack (wall E): a params tail that carries the caller's slice through a spread.
    [System.Runtime.CompilerServices.CollectionBuilder(typeof(VariadicBuilder), "Create")]
    public readonly ref struct variadic<T>
    {
        private readonly System.Span<T> m_elements;

        public variadic(System.Span<T> elements) { m_elements = elements; }

        public int Length => m_elements.Length;

        public System.Span<T>.Enumerator GetEnumerator() => m_elements.GetEnumerator();
    }

    public static class VariadicBuilder
    {
        public static variadic<T> Create<T>(System.ReadOnlySpan<T> items) => new variadic<T>(items.ToArray());
    }
}

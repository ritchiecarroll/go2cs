// The part of go.lib that src/Moves.cs is compiled against: the two attributes go2cs-gen writes on promoted-method
// forwarders and their classes, and the heap box. Built as its own assembly and not committed: no test reads it.
namespace go
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class GoPromotedHostAttribute : System.Attribute
    {
        public GoPromotedHostAttribute(System.Type @struct) { }
    }

    public sealed class GoPromotedAttribute : System.Attribute { }

    // golib's heap box, named as golib names it (U+0436).
    public sealed class \u0436<T>
    {
        public T Value;
    }
}

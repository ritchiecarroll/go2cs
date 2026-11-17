// The part of go.lib that src/Moves.cs is compiled against: the two attributes go2cs-gen writes on promoted-method
// forwarders and their classes, the attribute of a method holder, and the heap box. Built as its own assembly and not committed: no test reads it.
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

    // golib's heap box, named as golib names it (U+0436).
    public sealed class \u0436<T>
    {
        public T Value;
    }
}

// Two builds of one package for Test-GoApiCompatMoves.ps1. Without MOVED every promoted-method forwarder is in the
// package class, as go2cs-gen wrote them through 1.26.10.3. With MOVED each has left it, and only some of them the way
// the release announced: into a class marked [GoPromotedHost(typeof(S))], with the same name, signature and attributes.
// The attributes and the box are another assembly's (src/MovesLib.cs), as go.lib's are for a converted package: the
// generator reads each assembly alone, so it must find the marked classes without resolving them.
namespace go
{
    public static class pkg_package
    {
        public struct Inner { }

        public struct Outer { public Inner Inner; }

        public struct Box<T> { public Inner Inner; }

        public struct Solo { public Inner Inner; }

        public static int Val(this Inner i) => 1;

#if !MOVED
        // Moved as they are: the value form, the pointer form over the box, and a generic struct's.
        [GoPromoted] public static int Val(this Outer target) => 1;

        public static int Val(this \u0436<Outer> target) => 1;

        [GoPromoted] public static T Get<T>(this Box<T> target, T key) => key;

        // Moved, and changed on the way: another return type, a dropped attribute.
        [GoPromoted] public static int Changed(this Outer target) => 1;

        [GoPromoted] public static int Unmarked(this Outer target) => 1;

        // Moved to a class that is not marked, and to a class marked for another struct.
        [GoPromoted] public static int Elsewhere(this Outer target) => 1;

        [GoPromoted] public static int Misfiled(this Outer target) => 1;

        // Removed outright: one over a struct that has a marked class, one over a struct that has none.
        public static int Gone(this Outer target) => 1;

        public static int Only(this Solo target) => 1;
#endif
    }

#if MOVED
    [GoPromotedHost(typeof(pkg_package.Outer))]
    public static class forwardersOfOuter
    {
        [GoPromoted] public static int Val(this pkg_package.Outer target) => 1;

        public static int Val(this \u0436<pkg_package.Outer> target) => 1;

        [GoPromoted] public static long Changed(this pkg_package.Outer target) => 1;

        public static int Unmarked(this pkg_package.Outer target) => 1;
    }

    [GoPromotedHost(typeof(pkg_package.Box<>))]
    public static class forwardersOfBox
    {
        [GoPromoted] public static T Get<T>(this pkg_package.Box<T> target, T key) => key;

        [GoPromoted] public static int Misfiled(this pkg_package.Outer target) => 1;
    }

    public static class unmarkedClass
    {
        [GoPromoted] public static int Elsewhere(this pkg_package.Outer target) => 1;
    }
#endif
}

// A package below a path element that is a C# keyword: Go's internal/abi is namespace go.@internal, and the name an
// attribute stores for typeof(abi_package.Named) is go.internal.abi_package+Named, without the escape.
namespace go.@internal
{
    public static class abi_package
    {
        public struct Kind { }

        public struct Named { public Kind Kind; }

        public static int Size(this Kind k) => 1;

#if !MOVED
        [GoPromoted] public static int Size(this Named target) => 1;
#endif
    }

#if MOVED
    [GoPromotedHost(typeof(abi_package.Named))]
    public static class forwardersOfNamed
    {
        [GoPromoted] public static int Size(this abi_package.Named target) => 1;
    }
#endif
}

// A package's own methods. Without MOVED they are in the package class, as the converter wrote them through
// 1.26.10.3; with MOVED they are in the receiver type's method holder, a class marked [GoMethodHost(typeof(T))],
// named as the converter names it (package, U+1D1B, type). Both forms move: the value receiver and the pointer one.
namespace go
{
    public static class hold_package
    {
        public struct Clock { public long Ticks; }

#if !MOVED
        public static long Read(this Clock target) => target.Ticks;

        public static void Reset(this \u0436<Clock> target) { }
#endif
    }

#if MOVED
    [GoMethodHost(typeof(hold_package.Clock))]
    public static class hold\u1D1BClock
    {
        public static long Read(this hold_package.Clock target) => target.Ticks;

        public static void Reset(this \u0436<hold_package.Clock> target) { }
    }
#endif
}

// Nullable annotations. The compiler writes one NullableContext on a class, the value most of its members share, and
// repeats it only on a member that differs. So a method that moves to a class of other members carries other
// attributes than it did while it says the same: at base Named takes the package class's context and both forms of
// Plain carry their own; in the marked class it is the other way round. Those three are moves. Kind moves and its
// return type gains a "?", which is a change.
namespace go
{
    public static class ann_package
    {
        public struct Leaf { }

        public struct Tree { public Leaf Leaf; }

#nullable enable
        public static string Named(this Leaf l) => "";

        public static string Kind(this Leaf l) => "";

        public static string? Note(this Leaf l) => null;

        // Two more annotated members: with them most of the package class is annotated, so the class takes that
        // context and a member of it carries an attribute of its own only where it differs.
        public static string Text(this Leaf l) => "";

        public static string Label(this Leaf l) => "";

#if !MOVED
        [GoPromoted] public static string Named(this Tree target) => "";

        [GoPromoted] public static string Kind(this Tree target) => "";

#nullable disable
        [GoPromoted] public static string Plain(this Tree target) => "";

        public static string Plain(this \u0436<Tree> target) => "";
#endif
#nullable disable
    }

#if MOVED
    [GoPromotedHost(typeof(ann_package.Tree))]
    public static class forwardersOfTree
    {
#nullable enable
        [GoPromoted] public static string Named(this ann_package.Tree target) => "";

        [GoPromoted] public static string? Kind(this ann_package.Tree target) => "";

#nullable disable
        [GoPromoted] public static string Plain(this ann_package.Tree target) => "";

        public static string Plain(this \u0436<ann_package.Tree> target) => "";
    }
#endif
}

// THE DELTA DROP (OQ2). Without MOVED a type and an interface whose Go names a method of the package shared, and a
// constant, carry the converter's U+0394 prefix, as through 1.26.10.3; with MOVED they keep their Go names, and every
// member that names them names them so. Renames: the type, the interface, the constant (a property), a function over
// the type, a method of the type (now in its holder), a field and an interface member's return type that name it,
// and a type that implements the renamed interface; and Dial's method Spin, whose receiver parameter loses its prefix
// in the holder. NEGATIVE CONTROLS, which must stay red: Orphan's parameter type loses its U+0394 and no Go-named type
// replaces it; Shifted keeps the Go-named parameter and gains another; Retyped's return type becomes another type;
// Spun's receiver parameter takes another name, not its Go name.
namespace go
{
    public static class ren_package
    {
        public struct Dial { }

#if !MOVED
        public struct \u0394Stamp { public long At; }

        public interface \u0394Text { string Say(); }

        public struct \u0394Orphan { }

        public struct Holder { public \u0394Stamp When; }

        public interface IClock { \u0394Stamp Now(); }

        public struct Italic : \u0394Text { public string Say() => ""; }

        public static long \u0394Limit => 1;

        public static void Use(\u0394Stamp s) { }

        public static long Tick(this \u0394Stamp s) => s.At;

        public static void Orphan(\u0394Orphan g) { }

        public static void Shifted(\u0394Stamp s) { }

        public static \u0394Stamp Retyped() => default;

        public static void Spin(this Dial \u0394d) { }

        public static void Spun(this Dial \u0394d) { }
#else
        public struct Stamp { public long At; }

        public interface Text { string Say(); }

        public struct Holder { public Stamp When; }

        public interface IClock { Stamp Now(); }

        public struct Italic : Text { public string Say() => ""; }

        public static long Limit => 1;

        public static void Use(Stamp s) { }

        public static void Shifted(Stamp s, int extra) { }

        public static long Retyped() => default;
#endif
    }

#if MOVED
    [GoMethodHost(typeof(ren_package.Stamp))]
    public static class ren\u1D1BStamp
    {
        public static long Tick(this ren_package.Stamp s) => s.At;
    }

    [GoMethodHost(typeof(ren_package.Dial))]
    public static class ren\u1D1BDial
    {
        public static void Spin(this ren_package.Dial d) { }

        public static void Spun(this ren_package.Dial other) { }
    }
#endif
}

// THE CLONE DROP: a struct whose value copy no longer needs a clone loses go.IGoValueClone and its Delta-named Clone()
// (a zero-length array holds nothing to copy; syscall's InotifyEvent in 1.26.11.1). Event is the drop; Kept loses the
// method and keeps the interface, and Boxed is a class that loses both: neither is the drop.
namespace go
{
    public static class cln_package
    {
#if !MOVED
        public struct Event : IGoValueClone
        {
            public long Len;

            public Event \u0394Clone() => this;

            object System.ICloneable.Clone() => this;
        }

        public struct Kept : IGoValueClone
        {
            public long Len;

            public Kept \u0394Clone() => this;

            object System.ICloneable.Clone() => this;
        }

        public class Boxed : IGoValueClone
        {
            public long Len;

            public Boxed \u0394Clone() => this;

            object System.ICloneable.Clone() => this;
        }
#else
        public struct Event
        {
            public long Len;
        }

        public struct Kept : IGoValueClone
        {
            public long Len;

            object System.ICloneable.Clone() => this;
        }

        public class Boxed
        {
            public long Len;
        }
#endif
    }
}

// THE VARIADIC TAIL (wall E): a variadic parameter's params System.Span<T> became golib's params go.variadic<T>, and the
// spread property that returned a System.Span<T> returns a go.variadic<T>. Print, Insert, the delegate Logf (its Invoke
// and BeginInvoke), the interface method ILogger.Log and Ints.Spread are the change; Narrowed (another span type),
// Recast (another element type), Widened (another parameter changed too), Fixed (a span that is not params), Fixedf (a
// delegate's non-params span, in Invoke and BeginInvoke), Window (a property that became another span type) and Packed
// (a METHOD whose span return became a pack) are not, and are refused; so is Overloaded, whose variadic overload the
// baseline already had: its span overload was removed, not replaced.
namespace go
{
    public static class vat_package
    {
        public interface ISpread
        {
            System.Span<int> Elems { get; }
        }

        public interface ISpreadOf<T>
        {
            System.Span<T> Elems { get; }
        }

#if !MOVED
        public interface IHides : ISpread { }

        public interface IHidesOf<T> : ISpreadOf<T> { }

        public interface IWidens : ISpread { }

        public static int Print(params System.Span<object> a) => a.Length;

        public static int Insert<T>(int i, params System.Span<T> v) => i + v.Length;

        public delegate void Logf(string format, params System.Span<object> a);

        public interface ILogger
        {
            void Log(params System.Span<object> a);
        }

        public struct Ints
        {
            public System.Span<int> Spread => default;

            public System.Span<int> Window => default;
        }

        public static int Narrowed(params System.Span<object> a) => a.Length;

        public static int Recast(params System.Span<object> a) => a.Length;

        public static int Widened(int n, params System.Span<object> a) => n;

        public static int Fixed(System.Span<object> a, int n) => n;

        public delegate void Fixedf(System.Span<object> a, int n);

        public static System.Span<int> Packed() => default;

        public static int Overloaded(params System.Span<object> a) => a.Length;

        public static int Overloaded(params variadic<object> a) => a.Length;
#else
        public interface IHides : ISpread
        {
            new variadic<int> Elems { get; }
        }

        public interface IHidesOf<T> : ISpreadOf<T>
        {
            new variadic<T> Elems { get; }
        }

        public interface IWidens : ISpread
        {
            new System.ReadOnlySpan<int> Elems { get; }
        }

        public static int Print(params variadic<object> a) => a.Length;

        public static int Insert<T>(int i, params variadic<T> v) => i + v.Length;

        public delegate void Logf(string format, params variadic<object> a);

        public interface ILogger
        {
            void Log(params variadic<object> a);
        }

        public struct Ints
        {
            public variadic<int> Spread => default;

            public System.ReadOnlySpan<int> Window => default;
        }

        public static int Narrowed(params System.ReadOnlySpan<object> a) => a.Length;

        public static int Recast(params variadic<string> a) => a.Length;

        public static int Widened(long n, params variadic<object> a) => (int)n;

        public static int Fixed(variadic<object> a, int n) => n;

        public delegate void Fixedf(variadic<object> a, int n);

        public static variadic<int> Packed() => default;

        public static int Overloaded(params variadic<object> a) => a.Length;
#endif
    }
}

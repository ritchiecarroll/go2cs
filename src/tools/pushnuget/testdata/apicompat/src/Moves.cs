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

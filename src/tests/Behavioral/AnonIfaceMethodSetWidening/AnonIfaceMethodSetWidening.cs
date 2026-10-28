namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct gadget {
    internal nint n;
}

internal static @string Foo(this gadget g) {
    return fmt.Sprintf("foo %d"u8, g.n);
}

internal static @string Bar(this ref gadget g) {
    return "bar";
}

partial interface fooer {
    @string Foo();
}

internal partial interface main_type /*dyn*/ {
    @string Foo();
    @string Bar();
}

internal static void Main() {
    fooer v = new gadget(1);
    {
        var (_, ok) = v._<main_type>(ᐧ); if (ok) {
            fmt.Println("value-widened-wrong");
        } else {
            fmt.Println("value-not-widened-ok");
        }
    }

    fooer p = new gadgetжfooer(Ꮡ(new gadget(2)));
    {
        var (b, ok) = p._<main_type>(ᐧ); if (ok) {
            fmt.Println("pointer-widened-ok", b.Foo(), b.Bar());
        } else {
            fmt.Println("pointer-not-widened-wrong");
        }
    }
}

} // end main_package

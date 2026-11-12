namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct gadget {
    internal nint n;
}

} partial class mainᴛgadget {

internal static @string Foo(this gadget g) { return fmt.Sprintf("foo %d"u8, g.n); }
internal static @string Bar(this ref gadget g) { return "bar"; }

} partial class main_package {

partial interface fooer {
    @string Foo();
}

internal partial interface main_type /*dyn*/ {
    @string Foo();
    @string Bar();
}

internal static void main() {
    fooer v = new gadget(1);
    if (v._<main_type>(out _)) {
        fmt.Println("value-widened-wrong");
    } else {
        fmt.Println("value-not-widened-ok");
    }

    fooer p = new gadgetжfooer(Ꮡ(new gadget(2)));
    if (p._<main_type>(out var b)) {
        fmt.Println("pointer-widened-ok", b.Foo(), b.Bar());
    } else {
        fmt.Println("pointer-not-widened-wrong");
    }
}

} // end main_package

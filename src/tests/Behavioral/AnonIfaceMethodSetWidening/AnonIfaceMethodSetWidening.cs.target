namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct gadget {
    internal nint n;
}

internal static @string Foo(this gadget g) {
    return fmt.Sprintf("foo %d"u8, g.n);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string barˢ = "bar"u8;

internal static @string Bar(this ref gadget g) {
    return barˢ;
}

partial interface fooer {
    @string Foo();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object valueWidenedWrongˢ = (@string)"value-widened-wrong"u8;
private static readonly object valueNotWidenedOkˢ = (@string)"value-not-widened-ok"u8;
private static readonly object pointerWidenedOkˢ = (@string)"pointer-widened-ok"u8;
private static readonly object pointerNotWidenedWrongˢ = (@string)"pointer-not-widened-wrong"u8;

internal partial interface main_type /*dyn*/ {
    @string Foo();
    @string Bar();
}

internal static void Main() {
    fooer v = new gadget(1);
    {
        var (_, ok) = v._<main_type>(ᐧ); if (ok){
            fmt.Println(valueWidenedWrongˢ);
        } else {
            fmt.Println(valueNotWidenedOkˢ);
        }
    }
    fooer p = new gadgetжfooer(Ꮡ(new gadget(2)));
    {
        var (b, ok) = p._<main_type>(ᐧ); if (ok){
            fmt.Println(pointerWidenedOkˢ, b.Foo(), b.Bar());
        } else {
            fmt.Println(pointerNotWidenedWrongˢ);
        }
    }
}

} // end main_package

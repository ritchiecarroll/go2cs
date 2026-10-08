namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Element {
    public any Value;
}

internal static ж<Element> push(any v) {
    return Ꮡ(new Element(Value: v));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object bugEValue1ˢ = (@string)"BUG: e.Value != 1"u8;
private static readonly object okEValue1ˢ = (@string)"ok: e.Value == 1"u8;

internal static void Main() {
    var e = push((nint)(1));
    if (!AreEqual((~e).Value, (nint)(1))){
        fmt.Println(bugEValue1ˢ);
    } else {
        fmt.Println(okEValue1ˢ);
    }
    fmt.Println(AreEqual((~e).Value, (nint)(1)));
    fmt.Println(!AreEqual((~e).Value, (nint)(1)));
    fmt.Println(AreEqual((nint)(1), (~e).Value));
    fmt.Println(AreEqual((~e).Value, (nint)(2)));
    var n = push((nint)(-5));
    fmt.Println(AreEqual((~n).Value, (nint)(-5)));
    fmt.Println(AreEqual((~n).Value, (nint)(5)));
    any x = (nint)(42);
    fmt.Println(AreEqual(x, (nint)(42)));
    fmt.Println(!AreEqual(x, (nint)(42)));
}

} // end main_package

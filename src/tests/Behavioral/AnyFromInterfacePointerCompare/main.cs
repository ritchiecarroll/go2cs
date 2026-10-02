namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct node {
    internal nint id;
}

[GoRecv] internal static @string Error(this ref node n) {
    return fmt.Sprintf("node %d"u8, n.id);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object directPˢ = (@string)"direct == p:"u8;
private static readonly object directPˢ2 = (@string)"direct != p:"u8;
private static readonly object directQˢ = (@string)"direct == q:"u8;
private static readonly object widenedPˢ = (@string)"widened == p:"u8;
private static readonly object widenedPˢ2 = (@string)"widened != p:"u8;
private static readonly object pWidenedˢ = (@string)"p == widened:"u8;
private static readonly object widenedQˢ = (@string)"widened == q:"u8;
private static readonly object lookupˢ = (@string)"lookup:"u8;
private static readonly object lookupOtherˢ = (@string)"lookup other:"u8;

internal static void Main() {
    var p = Ꮡ(new node(id: 1));
    var q = Ꮡ(new node(id: 2));
    any direct = p.OrTypedNil();
    fmt.Println(directPˢ, direct == p);
    fmt.Println(directPˢ2, direct != p);
    fmt.Println(directQˢ, direct == q);
    error err = new nodeжerror(p);
    any widened = err;
    fmt.Println(widenedPˢ, widened == p);
    fmt.Println(widenedPˢ2, widened != p);
    fmt.Println(pWidenedˢ, p == widened);
    fmt.Println(widenedQˢ, widened == q);
    fmt.Println(lookupˢ, lookup(err, p));
    fmt.Println(lookupOtherˢ, lookup(err, q));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string foundˢ = "found"u8;
private static readonly @string missingˢ = "missing"u8;

internal static @string lookup(any key, ж<node> Ꮡwant) {
    if (key == Ꮡwant) {
        return foundˢ;
    }
    return missingˢ;
}

} // end main_package

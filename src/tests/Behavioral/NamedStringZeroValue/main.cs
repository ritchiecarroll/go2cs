global using Alias = go.main_package.Network;

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Network /*@string*/;

partial struct Label /*@string*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string tcpˢ = "tcp"u8;

internal static (ж<nint>, Network) lookup(bool ok) {
    if (!ok) {
        return (default!, (@string)"");
    }
    ref var n = ref heap<nint>(out var Ꮡn);
    n = 7;
    return (Ꮡn, tcpˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string namedˢ = "named"u8;

internal static Label single(bool ok) {
    if (!ok) {
        return (@string)""u8;
    }
    return namedˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string aliasedˢ = "aliased"u8;

internal static Alias viaAlias(bool ok) {
    if (!ok) {
        return (@string)""u8;
    }
    return aliasedˢ;
}

internal static void Main() {
    var (p0, net0) = lookup(false);
    var (p1, net1) = lookup(true);

    fmt.Println("zero tuple:", p0 == nil, ((@string)net0) == ""u8, len(net0));
    fmt.Println("set tuple:", p1.Value, (@string)net1, len(net1));

    fmt.Println("single zero:", ((@string)single(false)) == ""u8, len(single(false)));
    fmt.Println("single set:", (@string)single(true));

    fmt.Println("alias zero:", ((@string)viaAlias(false)) == ""u8);
    fmt.Println("alias set:", (@string)viaAlias(true));

    Network acc = default!;
    acc = acc + "a"u8;
    acc += "b"u8;

    fmt.Println("accumulated:", (@string)acc, acc == "ab"u8, len(acc));
}

} // end main_package

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct header {
    internal int32 kind;
    internal uint32 size;
    internal @string name;
}

partial struct view {
    internal int32 kind;
    internal uint32 size;
    internal @string name;
}

internal static partial ж<view> asView(ж<header> Ꮡh) {
    return Ꮡh.Reinterpret<header, view>();
}

internal static partial void churn() {
    var keep = new slice<slice<byte>>(64);
    for (nint i = 0; i < 400000; i++) {
        keep[(nint)(i & 63)] = new slice<byte>(24);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object aliasˢ = (@string)"alias:"u8;
private static readonly object lifetimeˢ = (@string)"lifetime:"u8;

internal static void Main() {
    var h = Ꮡ(new header(kind: 24, size: 16, name: "string"u8));
    var v = asView(h);
    v.Value.kind = 25;
    fmt.Println(aliasˢ, (~h).kind == 25, (~v).size == 16, (~v).name == "string"u8);
    var retained = asView(Ꮡ(new header(kind: 24, size: 16, name: "retained"u8)));
    churn();
    fmt.Println(lifetimeˢ, (~retained).kind == 24, (~retained).size == 16, (~retained).name == "retained"u8);
}

} // end main_package

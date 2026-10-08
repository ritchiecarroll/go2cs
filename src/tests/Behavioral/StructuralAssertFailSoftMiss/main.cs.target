namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct box<T> {
    internal T v;
}

internal static T Get<T>(this box<T> b) {
    return b.v;
}

partial struct keeper<T> {
    internal T v;
}

internal static T Get<T>(this ref keeper<T> k) {
    return k.v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string notAStringGetterˢ = "not a string getter"u8;

internal partial interface describe_type /*dyn*/ {
    @string Get();
}

internal static @string describe(any x) {
    {
        var (g, ok) = x._<describe_type>(ᐧ); if (ok) {
            return "string getter: "u8 + g.Get();
        }
    }
    return notAStringGetterˢ;
}

internal static void Main() {
    fmt.Println(describe(new box<@string>(v: "hello"u8)));
    fmt.Println(describe(new box<nint>(v: 5)));
    fmt.Println(describe(Ꮡ(new keeper<@string>(v: "kept"u8))));
    fmt.Println(describe(Ꮡ(new keeper<nint>(v: 7))));
}

} // end main_package

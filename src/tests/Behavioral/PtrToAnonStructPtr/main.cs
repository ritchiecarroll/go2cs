namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string decode(any output) {
    return fmt.Sprintf("%T"u8, output);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string bazˢ = "baz"u8;

[GoType("dyn")] internal partial struct main_data {
    public @string Foo;
}

internal static void Main() {
    ref var data = ref heap<ж<main_data>>(out var Ꮡdata);
    data = Ꮡ(new main_data());
    fmt.Println(decode(Ꮡdata));
    data.Value.Foo = bazˢ;
    fmt.Println((~data).Foo);
}

} // end main_package

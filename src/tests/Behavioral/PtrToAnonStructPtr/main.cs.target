namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string decode(any output) {
    return fmt.Sprintf("%T"u8, output);
}

internal partial struct main_data /*dyn*/ {
    public @string Foo;
}

internal static void Main() {
    ref var data = ref heap<ж<main_data>>(out var Ꮡdata);
    data = Ꮡ(new main_data());
    fmt.Println(decode(Ꮡdata));
    data.Value.Foo = "baz";
    fmt.Println((~data).Foo);
}

} // end main_package

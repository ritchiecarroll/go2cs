namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct semaRoot {
    internal nint n;
}

partial struct semTableᴛ1 /*dyn*/ {
    internal semaRoot root;
    internal array<byte> pad = new(40);
}

partial struct semTable /*[4]semTableᴛ1*/;

internal static ж<semaRoot> rootFor(this ref semTable t, nint i) {
    return Ꮡ(t.Value, i).of(semTableᴛ1.Ꮡroot);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object namedArrayAnonElementˢ = (@string)"named-array anon element compiles"u8;

internal static void Main() {
    fmt.Println(namedArrayAnonElementˢ);
}

} // end main_package

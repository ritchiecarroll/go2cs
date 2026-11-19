namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct semaRoot {
    internal nint n;
}

partial struct semTableᴛ1 /*dyn*/ {
    internal semaRoot root;
    internal array<byte> pad = new(40);
}

partial struct semTable /*[4]semTableᴛ1*/;

} partial class mainᴛsemTable {

internal static ж<semaRoot> rootFor(this ref semTable t, nint i) {
    return Ꮡ(t.Value, i).of(semTableᴛ1.Ꮡroot);
}

} partial class main_package {

internal static void main() {
    fmt.Println("named-array anon element compiles");
}

} // end main_package

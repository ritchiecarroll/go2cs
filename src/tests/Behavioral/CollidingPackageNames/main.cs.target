namespace go;

using fmt = fmt_package;
using dupmeta = collidea.dup_package;
using dup = collideb.dup_package;
using collidea;
using collideb;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string boxedInDuprenamedˢ = "boxed-in-duprenamed"u8;

internal static void Main() {
    fmt.Println(dupmeta.Greeting());
    fmt.Println(dup.Marker());

    dupmeta.Box<@string> b = default!;
    b.V = boxedInDuprenamedˢ;
    fmt.Println(b.Get());

    dupmeta.Widget w = default!;
    fmt.Println(w.Marker());
}

} // end main_package

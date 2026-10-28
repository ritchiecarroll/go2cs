namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct writer {
    internal slice<@string> @out;
}

internal static void typ(this ref writer w, @string typ) {
    w.@out = append(w.@out, "t:"u8 + typ);

    if (typ == "top"u8) {
        foreach (var (_, typΔ1) in new @string[]{"a", "b"}.slice()) {
            w.typ(typΔ1);
        }
    }
}

internal static void Main() {
    var w = Ꮡ(new writer());
    w.typ("top");

    foreach (var (_, line) in (~w).@out) {
        fmt.Println(line);
    }
}

} // end main_package

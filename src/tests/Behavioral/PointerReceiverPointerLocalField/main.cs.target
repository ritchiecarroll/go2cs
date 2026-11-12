namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct slot {
    internal nint n;
}

} partial class mainᴛslot {

internal static void inc(this ref slot s) { s.n++; }
internal static void add(this ref slot s, nint d) { s.n += d; }
internal static nint get(this ref slot s) { return s.n; }

} partial class main_package {

partial struct holder {
    internal slot s;
}

internal static partial ж<holder> get(ж<holder> h) { return h; }

internal static void main() {
    var @base = Ꮡ(new holder());
    var h = get(@base);

    h.Ꮡs.inc();
    h.Ꮡs.inc();
    h.Ꮡs.add(10);

    fmt.Println(@base.Ꮡs.get());
}

} // end main_package

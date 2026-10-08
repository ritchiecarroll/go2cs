namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct slot {
    internal nint n;
}

internal static void inc(this ref slot s) {
    s.n++;
}

internal static void add(this ref slot s, nint d) {
    s.n += d;
}

internal static nint get(this ref slot s) {
    return s.n;
}

partial struct holder {
    internal slot s;
}

internal static partial ж<holder> get(ж<holder> Ꮡh) {
    return Ꮡh;
}

internal static void Main() {
    var @base = Ꮡ(new holder(nil));
    var h = get(@base);
    h.of(holder.Ꮡs).inc();
    h.of(holder.Ꮡs).inc();
    h.of(holder.Ꮡs).add(10);
    fmt.Println(@base.of(holder.Ꮡs).get());
}

} // end main_package

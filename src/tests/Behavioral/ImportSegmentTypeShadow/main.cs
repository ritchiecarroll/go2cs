namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;
using static main_package;

partial class main_package {

partial struct sync {
    internal @string label;
    internal int32 hits;
}

} partial class mainᴛsync {

internal static void bump(this ref main_package.sync s, ж<atomic.Int32> c) {
    s.hits = c.Add(1);
}

} partial class main_package {

internal static void main() {
    ref var c = ref heap(new atomic.Int32(), out var Ꮡc);
    var s = new sync(label: "shadowed");

    s.bump(Ꮡc);
    s.bump(Ꮡc);
    s.bump(Ꮡc);

    fmt.Println(s.label, s.hits, Ꮡc.Load());
}

} // end main_package

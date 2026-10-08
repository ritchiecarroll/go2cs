namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

partial struct sync {
    internal @string label;
    internal int32 hits;
}

internal static void bump(this ref sync s, ж<atomic.Int32> Ꮡc) {
    s.hits = Ꮡc.Add(1);
}

internal static void Main() {
    ref var c = ref heap(new atomic.Int32(), out var Ꮡc);
    var s = new sync(label: "shadowed"u8);
    s.bump(Ꮡc);
    s.bump(Ꮡc);
    s.bump(Ꮡc);
    fmt.Println(s.label, s.hits, Ꮡc.Load());
}

} // end main_package

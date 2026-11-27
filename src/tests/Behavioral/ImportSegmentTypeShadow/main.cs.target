namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

partial struct sync {
    internal @string label;
    internal int32 hits;
}

internal static void bump(this ref sync s, ж<atomic.Int32> c) {
    s.hits = c.Add(1);
}

internal static void main() {
    ref var c = ref heap(new atomic.Int32(), out var Ꮡc);
    var s = new sync(label: "shadowed");

    s.bump(Ꮡc);
    s.bump(Ꮡc);
    s.bump(Ꮡc);

    var evalᴛ1 = Ꮡc.Load();
    fmt.Println(s.label, s.hits, evalᴛ1);
}

} // end main_package

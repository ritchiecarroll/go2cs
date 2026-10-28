namespace go;

using fmt = fmt_package;
using Δsync = sync_package;
using AliasStructToInterfaceLib = AliasStructToInterfaceLib_package;

partial class main_package {

partial interface mapInterface {
    (any value, bool ok) Load(any key);
    void Store(any key, any value);
}

internal static mapInterface newMap() {
    return new sync_MapжmapInterface(Ꮡ(new AliasStructToInterfaceLibꓸMap()));
}

partial interface storer {
    void Store(any key, any value);
}

internal static storer newDirect() {
    return new sync_Mapжstorer(Ꮡ(new Δsync.Map()));
}

internal static void Main() {
    var m = newMap();
    m.Store("a", (nint)1);
    var (v, ok) = m.Load("a");
    fmt.Println(v, ok);

    var d = newDirect();
    d.Store("b", (nint)2);
    (v, ok) = d._<storer, ж<Δsync.Map>>().Load((@string)"b"u8);
    fmt.Println(v, ok);
    (_, ok) = d._<storer, ж<Δsync.Map>>().Load((@string)"a"u8);
    fmt.Println(ok);
}

} // end main_package

namespace go;

using fmt = fmt_package;
using Δsync = sync_package;
using AliasStructToInterfaceLib = AliasStructToInterfaceLib_package;

partial class main_package {

[GoType] partial interface mapInterface {
    (any value, bool ok) Load(any key);
    void Store(any key, any value);
}

internal static mapInterface newMap() {
    return new sync_MapжmapInterface(Ꮡ(new AliasStructToInterfaceLibꓸMap()));
}

[GoType] partial interface storer {
    void Store(any key, any value);
}

internal static storer newDirect() {
    return new sync_Mapжstorer(Ꮡ(new Δsync.Map(nil)));
}

internal static void Main() {
    var m = newMap();
    m.Store((@string)"a"u8, (nint)(1));
    var (v, ok) = m.Load((@string)"a"u8);
    fmt.Println(v, ok);
    var d = newDirect();
    d.Store((@string)"b"u8, (nint)(2));
    (v, ok) = d._<ж<Δsync.Map>>().Load((@string)"b"u8);
    fmt.Println(v, ok);
    (_, ok) = d._<ж<Δsync.Map>>().Load((@string)"a"u8);
    fmt.Println(ok);
}

} // end main_package

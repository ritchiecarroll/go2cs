namespace go;

using fmt = fmt_package;

partial class main_package {

public partial struct hkdfState {
    internal @string name;
    internal nint size;
}

internal static @string describe(this ref hkdfState s) {
    return fmt.Sprintf("%s/%d"u8, s.name, s.size);
}

public partial struct cfg {
    internal bool verbose;
}

[GoLazy] partial class lazyᴛSupportedStates { public static map<uint16, Func<ж<hkdfState>>> SupportedStates = new map<uint16, Func<ж<hkdfState>>>{
    [0x0001] = () => Ꮡ(new hkdfState("sha256", 32)),
    [0x0002] = () => Ꮡ(new hkdfState("sha512", 64))
}; }

[GoLazy] partial class lazyᴛAppliers { public static slice<Action<ж<cfg>>> Appliers = new Action<ж<cfg>>[]{
    (ж<cfg> c) => {
        c.verbose = true;
    }
}.slice(); }

internal static void Main() {
    foreach (var (_, id) in new uint16[]{0x0001, 0x0002}.slice()) {
        var ctor = SupportedStates[id];
        var s = ctor();
        fmt.Println(s.describe());
    }

    var c = Ꮡ(new cfg());
    foreach (var (_, apply) in Appliers) {
        apply(c);
    }
    fmt.Println(c.verbose);
}

} // end main_package

namespace go;

using fmt = fmt_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class main_package {

/*linkname*/ internal static partial any clone(any m) {
    return mapclone(m);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object origLenˢ = (@string)"orig len:"u8;
private static readonly object cloneLenˢ = (@string)"clone len:"u8;
private static readonly object origAˢ = (@string)"orig a:"u8;
private static readonly object cloneAˢ = (@string)"clone a:"u8;
private static readonly object origHasDˢ = (@string)"orig has d:"u8;
private static readonly object origHasBˢ = (@string)"orig has b:"u8;
private static readonly object cloneCˢ = (@string)"clone c:"u8;
private static readonly object cloneDˢ = (@string)"clone d:"u8;
private static readonly object cloneHasBˢ = (@string)"clone has b:"u8;

internal static void Main() {
    var m = new map<@string, nint>{["a"] = 1, ["b"] = 2, ["c"] = 3};

    var mc = clone(m)._<map<@string, nint>>();

    mc["a"] = 99;
    mc["d"] = 4;
    delete(mc, "b"u8);

    fmt.Println(origLenˢ, len(m), cloneLenˢ, len(mc));
    fmt.Println(origAˢ, m["a"], cloneAˢ, mc["a"]);

    var (_, origHasD) = m["d", ꟷ];
    var (_, origHasB) = m["b", ꟷ];
    fmt.Println(origHasDˢ, origHasD, origHasBˢ, origHasB);

    var (_, cloneHasB) = mc["b", ꟷ];
    fmt.Println(cloneCˢ, mc["c"], cloneDˢ, mc["d"], cloneHasBˢ, cloneHasB);
}

} // end main_package

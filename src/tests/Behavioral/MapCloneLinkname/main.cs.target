namespace go;

using fmt = fmt_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class main_package {

/*linkname*/ internal static partial any clone(any m) {
    return mapclone(m);
}

internal static void Main() {
    var m = new map<@string, nint>{["a"] = 1, ["b"] = 2, ["c"] = 3};

    var mc = clone(m)._<map<@string, nint>>();

    mc["a"] = 99;
    mc["d"] = 4;
    delete(mc, "b"u8);

    fmt.Println("orig len:", len(m), "clone len:", len(mc));
    fmt.Println("orig a:", m["a"], "clone a:", mc["a"]);

    var (_, origHasD) = m["d", ꟷ];
    var (_, origHasB) = m["b", ꟷ];
    fmt.Println("orig has d:", origHasD, "orig has b:", origHasB);

    var (_, cloneHasB) = mc["b", ꟷ];
    fmt.Println("clone c:", mc["c"], "clone d:", mc["d"], "clone has b:", cloneHasB);
}

} // end main_package

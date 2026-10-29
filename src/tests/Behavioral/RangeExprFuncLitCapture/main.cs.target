namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct probe {
    internal @string name;
    internal Func<nint> run;
}

internal static void Main() {
    var vals = new nint[]{3, 1, 4, 1, 5}.slice();
    var counts = new map<@string, nint>{["a"] = 2, ["b"] = 7};
    nint total = 0;

        var valsʗ1 = vals;

        var valsʗ2 = vals;

        var countsʗ1 = counts;
    foreach (var (_, p) in new probe[]{
        new(name: "sum", run: () => {
            nint t = 0;
            foreach (var (_, v) in valsʗ1) {
                t += v;
            }
            return t;
        }),
        new(name: "len", run: () => len(valsʗ2)),
        new(name: "map", run: () => countsʗ1["a"] + countsʗ1["b"])
    }.slice()) {
        nint n = p.run();
        total += n;
        fmt.Println(p.name, n);
    }

    fmt.Println("total", total);

    for (nint round = 0; round < 2; round++) {
        ref var seen = ref heap<slice<@string>>(new @string[]{}.slice(), out var Ꮡseen);

        foreach (var (_, add) in new Action<@string>[]{
            (@string s) => {
                Ꮡseen.ValueSlot = append(Ꮡseen.ValueSlot, s + "!"u8);
            },
            (@string s) => {
                Ꮡseen.ValueSlot = append(Ꮡseen.ValueSlot, s + "?"u8);
            }
        }.slice()) {
            add(fmt.Sprint(round));
        }
        fmt.Println(round, seen);
    }
}

} // end main_package

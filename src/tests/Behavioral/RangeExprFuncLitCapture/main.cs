namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct probe {
    internal @string name;
    internal Func<nint> run;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object totalˢ = (@string)"total"u8;

internal static void Main() {
    var vals = new nint[]{3, 1, 4, 1, 5}.slice();
    var counts = new map<@string, nint>{["a"u8] = 2, ["b"u8] = 7};
    nint total = 0;
        var valsʗ1 = vals;

        var valsʗ2 = vals;

        var countsʗ1 = counts;
    foreach (var (_, p) in new probe[]{
        new(name: "sum"u8, run: () => {
            nint t = 0;
            foreach (var (_, v) in valsʗ1) {
                t += v;
            }
            return t;
        }),
        new(name: "len"u8, run: () => len(valsʗ2)),
        new(name: "map"u8, run: () => countsʗ1["a"u8] + countsʗ1["b"u8])
    }.slice()) {
        nint n = p.run();
        total += n;
        fmt.Println(p.name, n);
    }
    fmt.Println(totalˢ, total);
    for (nint round = 0; round < 2; round++) {
        ref var seen = ref heap<slice<@string>>(out var Ꮡseen);
        seen = new @string[]{}.slice();

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

namespace go;

using fmt = fmt_package;
using afoo = SameNameImportAlias.a.foo_package;
using bfoo = SameNameImportAlias.b.foo_package;
using SameNameImportAlias.a;
using SameNameImportAlias.b;

partial class main_package {

partial struct list /*[]global::go.SameNameImportAlias.a.foo_package.Inner*/;

partial struct table /*map[@string, bfoo.Other]*/;

partial struct holder {
    internal afoo.Inner a;
    internal bfoo.Other b;
    internal go.SameNameImportAlias.b.foo_package.ΔKind k;
}

internal static void Main() {
    afoo.Inner x = new afoo.Inner(N: 1);
    bfoo.Other y = new bfoo.Other(S: "s"u8);
    var z = new afoo.Inner(N: 2);
    fmt.Println(x.N, y.S, z.N);
    var l = new list(new afoo.Inner[]{new(N: 3)}.slice());
    var t = new table(new map<@string, bfoo.Other>{["k"u8] = new(S: "v"u8)});
    fmt.Println(l[0].N, t["k"u8].S);
    go.SameNameImportAlias.a.foo_package.ΔKind ka = new afoo.S(nil).Kind();
    go.SameNameImportAlias.b.foo_package.ΔKind kb = new bfoo.S(nil).Kind();
    var h = new holder(a: x, b: y, k: kb);
    fmt.Println(ka, kb, h.a.N, h.b.S, h.k);
}

} // end main_package

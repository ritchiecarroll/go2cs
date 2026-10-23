namespace go;

using fmt = fmt_package;
using tables = LazyPackageVars.tables_package;
using LazyPackageVars;

partial class main_package {

internal static void Main() {
    var (first, second) = tables.Seen();
    fmt.Println("order:", tables.Order, first, second);

    tables.Heavy = new tables.Entry[]{new(Name: "replaced", Weight: -1)}.slice();
    fmt.Println("heavy now:", tables.Heavy[0].Name, len(tables.Heavy));
    fmt.Println("ByName heavy:", len(tables.ByName["heavy"]), tables.ByName["heavy"][0].Name, tables.ByName["heavy"][2].Weight);
    fmt.Println("ByName all:", len(tables.ByName["all"]), tables.ByName["all"][0].Name);

    var p = tables.Roots[1];
    p.Value.Weight = 20;
    fmt.Println("roots:", tables.Roots[1] == p, (~tables.Roots[1]).Weight, (~tables.Roots[3]).Name);

    foreach (var (_, rule) in tables.Rules.ΔRangeSnapshot()) {
        fmt.Println("rule:", rule.Name, rule.Apply(7));
    }

    tables.Roots[0] = Ꮡ(new tables.Entry(Name: "swapped"));
    fmt.Println("swapped:", (~tables.Roots[0]).Name);

    var before = tables.GridAddr();
    before.Value[0] = 100;
    fmt.Println("grid:", tables.Grid[0], tables.ᏑGrid == before, tables.Grid[19]);
    tables.Grid[1] = 200;
    fmt.Println("grid via pointer:", before.Value[1]);
}

} // end main_package

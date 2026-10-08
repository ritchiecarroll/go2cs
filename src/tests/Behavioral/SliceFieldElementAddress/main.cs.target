namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct grid {
    internal slice<slice<nint>> rows;
}

internal static void addLine(this ref grid g) {
    g.rows = append(g.rows, (slice<nint>)(default!));
}

internal static void push(this ref grid g, nint v) {
    var row = Ꮡ(g.rows, len(g.rows) - 1);
    row.ValueSlot = append(row.ValueSlot, v);
}

internal static void Main() {
    grid g = default!;
    g.addLine();
    g.push(1);
    g.push(2);
    g.push(3);
    g.addLine();
    g.push(10);
    g.push(20);
    foreach (var (i, row) in g.rows) {
        fmt.Printf("row %d len=%d %v\n"u8, i, len(row), row);
    }
    var p = Ꮡ(g.rows, 0);
    (p.ValueSlot)[0] = 99;
    fmt.Printf("after mutate: %v\n"u8, g.rows[0]);
}

} // end main_package

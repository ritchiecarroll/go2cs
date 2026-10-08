namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct ints /*[]nint*/;

internal static void swap(this ref ints p, nint i, nint j) {
    ((p)[i], (p)[j]) = ((p)[j], (p)[i]);
}

internal static void set(this ref ints p, nint i, nint v) {
    (p)[i] = v;
}

internal static void show(ref ints p) {
    foreach (var (i, x) in p) {
        if (i > 0) {
            fmt.Print((@string)" "u8);
        }
        fmt.Print(x);
    }
    fmt.Println();
}

partial struct cell {
    internal nint v;
}

partial struct cells /*[]cell*/;

internal static ж<cell> at(this ref cells c, nint i) {
    return Ꮡ((c), i);
}

internal static void swapAt(this ref cells c, nint i, nint j) {
    (c.at(i).Value, c.at(j).Value) = (c.at(j).Value, c.at(i).Value);
}

internal static void setAt(this ref cells c, nint i, nint v) {
    c.at(i).Value = new cell(v);
}

internal static void showCells(ref cells c) {
    foreach (var (i, x) in c) {
        if (i > 0) {
            fmt.Print((@string)" "u8);
        }
        fmt.Print(x.v);
    }
    fmt.Println();
}

internal static void Main() {
    var p = Ꮡ(new ints(new nint[]{10, 20, 30, 40}.slice()));
    p.swap(0, 3);
    p.swap(1, 2);
    p.set(0, 99);
    show(ref (p).DerefOrNull());
    var q = Ꮡ(new ints(new nint[]{1, 2, 3, 4, 5}.slice()));
    for ((nint i, nint j) = (0, len(q.ValueSlot) - 1); i < j; (i, j) = (i + 1, j - 1)) {
        q.swap(i, j);
    }
    show(ref (q).DerefOrNull());
    var c = Ꮡ(new cells(new cell[]{new(10), new(20), new(30), new(40)}.slice()));
    c.swapAt(0, 3);
    c.swapAt(1, 2);
    c.setAt(0, 99);
    showCells(ref (c).DerefOrNull());
    var d = Ꮡ(new cells(new cell[]{new(1), new(2), new(3), new(4), new(5)}.slice()));
    for ((nint i, nint j) = (0, len(d.ValueSlot) - 1); i < j; (i, j) = (i + 1, j - 1)) {
        d.swapAt(i, j);
    }
    showCells(ref (d).DerefOrNull());
    var e = Ꮡ(new cells(new cell[]{new(5), new(3), new(9), new(1), new(7), new(2), new(8)}.slice()));
    for (nint i = 0; i < len(e.ValueSlot); i++) {
        nint min = i;
        for (nint j = i + 1; j < len(e.ValueSlot); j++) {
            if ((~e.at(j)).v < (~e.at(min)).v) {
                min = j;
            }
        }
        if (min != i) {
            e.swapAt(i, min);
        }
    }
    showCells(ref (e).DerefOrNull());
}

} // end main_package

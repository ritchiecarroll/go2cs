namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static void inc(this ref counter c) {
    c.n++;
}

partial struct holder {
    /*embed*/ internal counter counter;
    internal @string name;
}

partial struct slotBox {
    internal nint id;
    internal holder slot;
}

internal static void Main() {
    var b = new slotBox(id: 3);
    b.slot.counter.inc();
    b.slot.counter.inc();
    b.slot.counter.inc();
    fmt.Println("id:", b.id, "count:", b.slot.n, "name-empty:", b.slot.name == ""u8);
}

} // end main_package

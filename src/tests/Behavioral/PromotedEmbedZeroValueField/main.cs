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
    internal partial ref counter counter { get; }
    internal @string name;
}

partial struct slotBox {
    internal nint id;
    internal holder slot;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object countˢ = (@string)"count:"u8;
private static readonly object nameEmptyˢ = (@string)"name-empty:"u8;

internal static void Main() {
    var b = new slotBox(id: 3);
    b.slot.counter.inc();
    b.slot.counter.inc();
    b.slot.counter.inc();
    fmt.Println((@string)"id:"u8, b.id, countˢ, b.slot.n, nameEmptyˢ, b.slot.name == ""u8);
}

} // end main_package

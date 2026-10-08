namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt mask => 7;

partial struct hasher {
    internal array<uint32> head = new(mask + 1);
    internal array<uint32> prev = new(16);
    internal array<array<nint>> pairs = new(2, () => new(3));
}

internal static uint32 storeHead(this ref hasher h, uint32 hash, uint32 index) {
    var hh = Ꮡ(h.head, (uint32)(hash & (uint32)mask));
    var previous = hh.Value;
    h.prev[(uint32)(index & 15)] = previous;
    hh.Value = index + 1;
    return previous;
}

internal static void bumpPair(this ref hasher h, nint i, nint j) {
    var p = Ꮡ(h.pairs[i], j);
    p.Value += 10;
}

internal static uint32 headAt(this ref hasher h, nint i) {
    return h.head[i];
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object headSumˢ = (@string)"head sum:"u8;

internal static void Main() {
    var h = Ꮡ(new hasher(pairs: new array<nint>[]{new nint[]{0, 0, 0}.array(), new nint[]{0, 0, 0}.array()}.array()));
    fmt.Println(h.storeHead(3, 100));
    fmt.Println(h.storeHead(3, 200));
    fmt.Println(h.headAt(3));
    fmt.Println((~h).prev[(nint)(200 & 15)]);
    fmt.Println(h.storeHead(12, 7));
    fmt.Println(h.headAt(4));
    fmt.Println(h.headAt(3));
    h.bumpPair(1, 2);
    h.bumpPair(1, 2);
    h.bumpPair(0, 0);
    fmt.Println((~h).pairs[1][2], (~h).pairs[0][0], (~h).pairs[0][1]);
    var total = (uint32)0;
    for (nint i = 0; i < len((~h).head); i++) {
        total += (~h).head[i];
    }
    fmt.Println(headSumˢ, total);
}

} // end main_package

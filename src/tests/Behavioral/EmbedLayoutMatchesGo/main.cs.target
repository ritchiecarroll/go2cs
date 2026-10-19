namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using System.Runtime.InteropServices;

partial class main_package {

partial struct pair {
    internal uint32 a, b;
}

partial struct ordered {
    /*embed*/ internal pair pair;
    internal uint32 c;
}

partial struct nih {
}

[StructLayout(LayoutKind.Explicit, Size = 8)] partial struct leader {
    [FieldOffset(0)] internal readonly nih _;
    [FieldOffset(0)] /*embed*/ internal pair pair;
}

partial struct ptrEmbed {
    /*embed*/ internal ж<pair> pair;
    internal uint32 d;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object orderedFieldsˢ = (@string)"ordered fields:"u8;
private static readonly object orderedWholeˢ = (@string)"ordered whole: "u8;
private static readonly object orderedReadˢ = (@string)"ordered read:  "u8;
private static readonly object leaderFieldsˢ = (@string)"leader fields: "u8;
private static readonly object leaderReadˢ = (@string)"leader read:   "u8;
private static readonly object ptrEmbedˢ = (@string)"ptrEmbed:      "u8;

internal static void Main() {
    var buf = new slice<byte>(16);

    var o = Ꮡ(buf, 0).Reinterpret<byte, ordered>();
    (o.Value.a, o.Value.b, o.Value.c) = (1, 2, 3);
    fmt.Println(orderedFieldsˢ, buf[..12]);

    o.Value = new ordered(new pair(4, 5), 6);
    fmt.Println(orderedWholeˢ, buf[..12]);

    var v = o.Value;
    fmt.Println(orderedReadˢ, v.a, v.b, v.c, /* unsafe.Sizeof(v) */ (uintptr)12, /* unsafe.Offsetof(v.c) */ (uintptr)8);

    foreach (var (i, _) in buf) {
        buf[i] = 0;
    }

    var l = Ꮡ(buf, 0).Reinterpret<byte, leader>();
    (l.Value.a, l.Value.b) = (7, 8);
    fmt.Println(leaderFieldsˢ, buf[..8]);

    var w = l.Value;
    fmt.Println(leaderReadˢ, w.a, w.b, /* unsafe.Sizeof(w) */ (uintptr)8);

    var p = new ptrEmbed(Ꮡ(new pair(9, 10)), 11);
    var q = p.pair;
    q.Value.a = 12;
    fmt.Println(ptrEmbedˢ, p.a, p.b, p.d, p.pair == q);
}

} // end main_package

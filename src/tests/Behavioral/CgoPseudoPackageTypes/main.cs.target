global using cint = go.mainꓸC_package.@int;

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using C = mainꓸC_package;

partial class main_package {

partial struct pair {
    internal C.@int a, b;
}

internal static C.@int add(C.@int a, C.@int b) { return a + b; }

internal static nint /*n*/ walk(ж<C.struct_point> Ꮡp) {
    nint n = default;
    ref var p = ref Ꮡp.DerefOrNull();

    for (; Ꮡp != nil; Ꮡp = p.next, p = ref Ꮡp.DerefOrNull()) {
        n++;
    }

    return n;
}

internal static @string colorName(C.enum_color c) {
    switch (c) {
    case var ᴛ when ᴛ == C.RED: {
        return "red";
    }
    case var ᴛ when ᴛ == C.GREEN: {
        return "green";
    }}

    return "other";
}

internal static void Main() {
    Cꓸdiv_t d = default;
    d.quot = (C.@int)C.LIMIT;
    d.rem = 3;

    ref var p = ref heap<C.struct_point>(new C.struct_point(x: 1, y: 2), out var Ꮡp);
    p.name[0] = (rune)'a';
    var q = Ꮡ(new C.struct_point(x: 3, next: Ꮡp));

    var p2 = p.ΔClone();
    p2.name[0] = (rune)'b';

    var seg = new C.struct_segment(from: p.ΔClone(), to: q.Value.ΔClone());
    seg.to.name[1] = (rune)'z';

    C.@void v = default;
    C.union_value u = default;
    u[0] = 7;

    Cꓸsize_t size = 42;
    C.@long lim = C.LIMIT;
    cint k = add(2, 3);

    var big = (C.@int)2147483647;
    big++;

    var xs = slice<C.@int>([1, 2, 3]);
    var m = new map<C.@char, C.@double>{[(rune)'z'] = 1.5D};
    var pr = new pair(a: 4, b: 5);

    fmt.Println(d.quot, d.rem, p.x + p.y, p.name[0], p2.name[0], seg.from.x, seg.to.x, seg.to.name[1], q.name[1]);
    fmt.Println(/* unsafe.Sizeof(p) */ (uintptr)24, /* unsafe.Sizeof(v) */ (uintptr)0, /* unsafe.Sizeof(u) */ (uintptr)8, /* unsafe.Sizeof(seg) */ (uintptr)48, /* unsafe.Sizeof(d) */ (uintptr)8);
    fmt.Println(walk(q), u[0], size, lim, k, big, len(xs), xs[2], m[(rune)'z'], pr.a + pr.b);
    fmt.Println(colorName(C.RED), colorName(C.GREEN), colorName(C.BLUE), (float64)(C.HALF), C.GREETING + "!");
    fmt.Println((C.@uint)1, (C.schar)(-2), (C.uchar)3, (C.@short)(-4), (C.@ushort)5, (C.@ulong)6, (C.longlong)(-7), (C.ulonglong)8, (C.@float)0.25F, (C.@double)0.125D);
    fmt.Printf("%T %T %T %T %T\n"u8, p, k, d, size, (C.enum_color)C.BLUE);
}

} // end main_package

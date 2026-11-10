global using cint = go.main_package._Ctype_int;

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct pair {
    internal _Ctype_int a, b;
}

internal static _Ctype_int add(_Ctype_int a, _Ctype_int b) { return a + b; }

internal static nint /*n*/ walk(ж<_Ctype_struct_point> Ꮡp) {
    nint n = default;
    ref var p = ref Ꮡp.DerefOrNull();

    for (; Ꮡp != nil; Ꮡp = p.next) {
        p = ref Ꮡp.DerefOrNull();
        n++;
    }

    return n;
}

internal static @string colorName(_Ctype_enum_color c) {
    switch (c) {
    case var ᴛ when ᴛ == _Ciconst_RED: {
        return "red";
    }
    case var ᴛ when ᴛ == _Ciconst_GREEN: {
        return "green";
    }}

    return "other";
}

internal static void Main() {
    _Ctype_div_t d = default;
    d.quot = (_Ctype_int)_Ciconst_LIMIT;
    d.rem = 3;

    ref var p = ref heap<_Ctype_struct_point>(new _Ctype_struct_point(x: 1, y: 2), out var Ꮡp);
    p.name[0] = (rune)'a';
    var q = Ꮡ(new _Ctype_struct_point(x: 3, next: Ꮡp));

    var p2 = p.ΔClone();
    p2.name[0] = (rune)'b';

    var seg = new _Ctype_struct_segment(from: p.ΔClone(), to: q.Value.ΔClone());
    seg.to.name[1] = (rune)'z';

    _Ctype_void v = default;
    _Ctype_union_value u = default;
    u[0] = 7;

    _Ctype_size_t size = 42;
    _Ctype_long lim = _Ciconst_LIMIT;
    cint k = add(2, 3);

    var big = (_Ctype_int)2147483647;
    big++;

    var xs = slice<_Ctype_int>([1, 2, 3]);
    var m = new map<_Ctype_char, _Ctype_double>{[(rune)'z'] = 1.5D};
    var pr = new pair(a: 4, b: 5);

    fmt.Println(d.quot, d.rem, p.x + p.y, p.name[0], p2.name[0], seg.from.x, seg.to.x, seg.to.name[1], q.name[1]);
    fmt.Println(/* unsafe.Sizeof(p) */ (uintptr)24, /* unsafe.Sizeof(v) */ (uintptr)0, /* unsafe.Sizeof(u) */ (uintptr)8, /* unsafe.Sizeof(seg) */ (uintptr)48, /* unsafe.Sizeof(d) */ (uintptr)8);
    fmt.Println(walk(q), u[0], size, lim, k, big, len(xs), xs[2], m[(rune)'z'], pr.a + pr.b);
    fmt.Println(colorName(_Ciconst_RED), colorName(_Ciconst_GREEN), colorName(_Ciconst_BLUE), (float64)(_Cfconst_HALF), _Csconst_GREETING + "!");
    fmt.Println((_Ctype_uint)1, (_Ctype_schar)(-2), (_Ctype_uchar)3, (_Ctype_short)(-4), (_Ctype_ushort)5, (_Ctype_ulong)6, (_Ctype_longlong)(-7), (_Ctype_ulonglong)8, (_Ctype_float)0.25F, (_Ctype_double)0.125D);
    fmt.Printf("%T %T %T %T %T\n"u8, p, k, d, size, (_Ctype_enum_color)_Ciconst_BLUE);
}

} // end main_package

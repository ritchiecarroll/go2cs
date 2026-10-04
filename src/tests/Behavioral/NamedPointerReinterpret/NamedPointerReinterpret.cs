namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct @base {
    internal int64 a;
    internal int64 b;
}

[GoType("@base")] partial struct view;

internal static ж<@base> makeBase() {
    ref var x = ref heap<@base>(out var Ꮡx);
    x = new @base(a: 10, b: 20);
    return Ꮡx;
}

[GoType("ж<int64>")] partial class intRef;

[GoType("@string")] partial struct tail;

[GoRecv] internal static byte chop(this ref tail t) {
    var b = (t)[0];
    t = (t)[1..];
    return b;
}

internal static @string consume(@string sʗp) {
    ref var s = ref heap(sʗp, out var Ꮡs);

    var t = Ꮡs.Reinterpret<@string, tail>();
    var b1 = t.chop();
    var b2 = t.chop();
    return ((@string)new byte[]{b1, b2}.slice()) + ((@string)(t.Value));
}

internal static nint sliceToArray(slice<byte> s) {
    var a = new array<byte>(s, 4);
    var p = Ꮡ(array<byte>.Alias(s, 2));
    return (nint)a[3] + (nint)p.Value[1];
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nilrefˢ = "nilref"u8;
private static readonly @string otherˢ = "other"u8;

internal static @string classify(any v) {
    if (AreEqual(v, ((intRef)nil))) {
        return nilrefˢ;
    }
    return otherˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string abcdˢ = "abcd"u8;

internal static void Main() {
    var pb = makeBase();
    var pv = pb.Reinterpret<@base, view>();
    var bb = ((@base)(pv.Value));
    fmt.Println(bb.a, bb.b);
    any boxed = ((intRef)nil);
    fmt.Println(classify(boxed));
    ref var n = ref heap<int64>(out var Ꮡn);
    n = (int64)5;
    intRef r = Ꮡn;
    fmt.Println(r.Value, classify(r));
    fmt.Println(consume(abcdˢ));
    fmt.Println(sliceToArray(new byte[]{1, 2, 3, 4}.slice()));
    var v2 = new view(new @base(a: 5, b: 6));
    fmt.Println(v2.a + v2.b);
}

} // end main_package

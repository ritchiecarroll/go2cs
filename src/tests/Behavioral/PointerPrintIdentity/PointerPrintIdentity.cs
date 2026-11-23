namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct pair {
    internal nint n;
    internal @string s;
}

partial struct plain {
    internal nint n, m;
}

internal static void check(@string name, any p, @unsafe.Pointer up) {
    @string viaP = fmt.Sprintf("%p"u8, p);
    @string viaV = fmt.Sprintf("%v"u8, @unsafe.Pointer.OrTypedNil(up));
    @string viaX = fmt.Sprintf("%#x"u8, (uintptr)up);
    var viaR = reflect.ValueOf(p).Pointer() == (uintptr)up;
    fmt.Println(name, viaP == viaV, viaP == viaX, viaR);
}

internal static void main() {
    ref var i = ref heap<nint>(1, out var Ꮡi);
    @string first = fmt.Sprintf("%p"u8, Ꮡi);
    fmt.Println("int, %p first:", first == fmt.Sprintf("%v"u8, @unsafe.Pointer.FromPinnedBox(Ꮡi)));
    fmt.Println("int, %p again:", first == fmt.Sprintf("%p"u8, Ꮡi));

    ref var j = ref heap<nint>(2, out var Ꮡj);
    @string taken = fmt.Sprintf("%v"u8, @unsafe.Pointer.FromPinnedBox(Ꮡj));
    fmt.Println("int, address first:", taken == fmt.Sprintf("%p"u8, Ꮡj));

    ref var k = ref heap<nint>(3, out var Ꮡk);
    check("int local:", Ꮡk, @unsafe.Pointer.FromPinnedBox(Ꮡk));

    var h = @new<int64>();
    check("new(int64):", h.OrTypedNil(), @unsafe.Pointer.FromPinnedBox(h));

    ref var pl = ref heap(new plain(), out var Ꮡpl);
    check("reference-free field:", Ꮡpl.Ꮡm, @unsafe.Pointer.FromPinnedBox(Ꮡpl.Ꮡm));

    ref var pr = ref heap(new pair(), out var Ꮡpr);
    check("reference-bearing field:", Ꮡpr.Ꮡs, @unsafe.Pointer.FromPinnedBox(Ꮡpr.Ꮡs));

    var xs = slice<int32>([1, 2, 3]);
    check("slice element:", Ꮡ(xs, 1), @unsafe.Pointer.FromPinnedBox(Ꮡ(xs, 1)));

    ref var arr = ref heap(new array<byte>(4), out var Ꮡarr);
    check("array:", Ꮡarr, @unsafe.Pointer.FromPinnedBox(Ꮡarr));

    ref var str = ref heap<@string>("go", out var Ꮡstr);
    check("string local:", Ꮡstr, @unsafe.Pointer.FromPinnedBox(Ꮡstr));

    var ps = Ꮡ(new pair(n: 1, s: "x"));
    check("struct with a string:", ps.OrTypedNil(), @unsafe.Pointer.FromPinnedBox(ps));

    var z = @new<EmptyStruct>();
    check("zero-size:", z.OrTypedNil(), @unsafe.Pointer.FromPinnedBox(z));

    ref var a = ref heap<nint>(out var Ꮡa);
    a = 1;
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 2;
    var (pa1, pa2) = (Ꮡa, Ꮡa);
    fmt.Println("one variable, one %p:", fmt.Sprintf("%p"u8, pa1.OrTypedNil()) == fmt.Sprintf("%p"u8, pa2.OrTypedNil()));
    fmt.Println("two variables, two %p:", fmt.Sprintf("%p"u8, Ꮡa) != fmt.Sprintf("%p"u8, Ꮡb));
}

} // end main_package

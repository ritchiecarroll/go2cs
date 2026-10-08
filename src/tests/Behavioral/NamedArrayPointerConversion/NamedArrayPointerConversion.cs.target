namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct MyArray /*[4]byte*/;

partial struct MyEmptyArray /*[0]byte*/;

partial struct MyInts /*[3]nint*/;

partial struct MyBytes /*[]byte*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object b1Freshˢ = (@string)"B1 fresh:"u8;
private static readonly object lenˢ = (@string)"len:"u8;
private static readonly object b2ZeroLengthˢ = (@string)"B2 zero-length:"u8;
private static readonly object b3Intsˢ = (@string)"B3 ints:"u8;
private static readonly object b4Independentˢ = (@string)"B4 independent:"u8;
private static readonly object c1Nilˢ = (@string)"C1 nil:"u8;
private static readonly object c2Compositeˢ = (@string)"C2 composite:"u8;
private static readonly object c3NamedSlicePtrAliasesˢ = (@string)"C3 named-slice ptr aliases original:"u8;
private static readonly object c4UnderlyingSlicePtrˢ = (@string)"C4 underlying-slice ptr aliases named:"u8;

internal static void Main() {
    var q = Ꮡ(new MyArray(new array<byte>(4)));
    q.Value[0] = 7;
    q.Value[3] = 8;
    fmt.Println(b1Freshˢ, q.Value, lenˢ, len(q.Value));
    var z = Ꮡ(new MyEmptyArray(new array<byte>(0)));
    fmt.Println(b2ZeroLengthˢ, z.Value, lenˢ, len(z.Value));
    var n = Ꮡ(new MyInts(new array<nint>(3)));
    n.Value[1] = 5;
    fmt.Println(b3Intsˢ, n.Value, lenˢ, len(n.Value));
    var a = Ꮡ(new MyArray(new array<byte>(4)));
    var b = Ꮡ(new MyArray(new array<byte>(4)));
    a.Value[0] = 1;
    fmt.Println(b4Independentˢ, a.Value, b.Value);
    ж<MyEmptyArray> nilp = ж<MyEmptyArray>.NilBoxOfDims(0L);
    fmt.Println(c1Nilˢ, nilp == nil);
    var c = Ꮡ(new MyArray(new byte[]{1, 2, 3, 4}.array()));
    c.Value[1] = 9;
    fmt.Println(c2Compositeˢ, c.Value);
    ref var sraw = ref heap<slice<byte>>(out var Ꮡsraw);
    sraw = new byte[]{1, 2, 3}.slice();
    var sp = Ꮡsraw.Reinterpret<slice<byte>, MyBytes>();
    sp.ValueSlot = append(sp.ValueSlot, (byte)(4));
    fmt.Println(c3NamedSlicePtrAliasesˢ, sraw);
    ref var mb = ref heap<MyBytes>(out var Ꮡmb);

    mb = new MyBytes(new byte[]{9, 8}.slice());
    var bp = Ꮡmb.of(MyBytes.Ꮡm_value);
    bp.ValueSlot = append(bp.ValueSlot, (byte)(7));
    fmt.Println(c4UnderlyingSlicePtrˢ, mb);
}

} // end main_package

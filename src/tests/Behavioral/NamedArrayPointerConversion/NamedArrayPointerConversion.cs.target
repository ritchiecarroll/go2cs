namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct MyArray /*[4]byte*/;

partial struct MyEmptyArray /*[0]byte*/;

partial struct MyInts /*[3]nint*/;

partial struct MyBytes /*[]byte*/;

internal static void Main() {
    var q = Ꮡ(new MyArray(new array<byte>(4)));
    q.Value[0] = 7;
    q.Value[3] = 8;
    fmt.Println("B1 fresh:", q.Value, "len:", len(q.Value));

    var z = Ꮡ(new MyEmptyArray(new array<byte>(0)));
    fmt.Println("B2 zero-length:", z.Value, "len:", len(z.Value));

    var n = Ꮡ(new MyInts(new array<nint>(3)));
    n.Value[1] = 5;
    fmt.Println("B3 ints:", n.Value, "len:", len(n.Value));

    var a = Ꮡ(new MyArray(new array<byte>(4)));
    var b = Ꮡ(new MyArray(new array<byte>(4)));
    a.Value[0] = 1;
    fmt.Println("B4 independent:", a.Value, b.Value);

    ж<MyEmptyArray> nilp = ж<MyEmptyArray>.NilBoxOfDims(0L);
    fmt.Println("C1 nil:", nilp == nil);

    var c = Ꮡ(new MyArray(new byte[]{1, 2, 3, 4}.array()));
    c.Value[1] = 9;
    fmt.Println("C2 composite:", c.Value);

    ref var sraw = ref heap<slice<byte>>(out var Ꮡsraw);
    sraw = new byte[]{1, 2, 3}.slice();
    var sp = Ꮡsraw.Reinterpret<slice<byte>, MyBytes>();
    sp.ValueSlot = append(sp.ValueSlot, (byte)(4));
    fmt.Println("C3 named-slice ptr aliases original:", sraw);

    ref var mb = ref heap<MyBytes>(out var Ꮡmb);

    mb = new MyBytes(new byte[]{9, 8}.slice());
    var bp = Ꮡmb.of(MyBytes.Ꮡm_value);
    bp.ValueSlot = append(bp.ValueSlot, (byte)(7));
    fmt.Println("C4 underlying-slice ptr aliases named:", mb);
}

} // end main_package

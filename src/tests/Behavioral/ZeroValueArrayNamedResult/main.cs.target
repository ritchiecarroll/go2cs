namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct ticket {
    internal array<byte> aesKey = new(16);
    internal array<byte> hmacKey = new(16);
    internal nint seq;
}

partial struct box {
    internal nint id;
    internal ticket t;
}

internal static partial array<byte> /*a16*/ as16(uint64 hi, uint64 lo) {
    array<byte> a16 = new(16);

    putUint64(a16[..8], hi);
    putUint64(a16[8..], lo);
    return a16.Clone();
}

internal static void putUint64(slice<byte> b, uint64 v) {
    for (nint i = 0; i < 8; i++) {
        b[i] = (byte)(v.Rsh((int64)((56 - 8 * i))));
    }
}

internal static partial ticket /*key*/ ticketFromBytes(/*[32]*/ array<byte> b) {
    ticket key = new();

    b = b.Clone();
    copy(key.aesKey[..], b[..16]);
    copy(key.hmacKey[..], b[16..]);
    key.seq = 7;
    return key.ΔClone();
}

internal static partial box /*bx*/ makeBox() {
    box bx = new();

    bx.id = 3;
    bx.t.aesKey[0] = 9;
    bx.t.hmacKey[15] = 4;
    return bx.ΔClone();
}

internal static partial (array<byte> a4, error err) withDefer() {
    array<byte> a4 = new(4);
    error err = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            a4[0]++;
        }, ref ᒐ);
        a4[1] = 2;
        (a4, err) = (a4.Clone(), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (a4, err);
}

internal static Func<array<byte>> literalAs3 = () => {
    array<byte> a3 = new(3);
    a3[2] = 5;
    return a3.Clone();
};

internal static partial (nint n, @string s) scalars() {
    nint n = default!;
    @string s = default!;

    n = 5;
    return (n, s);
}

internal static void Main() {
    var a = as16(0x0102030405060708UL, 0x090a0b0c0d0e0f10UL);
    fmt.Println(len(a), a[0], a[7], a[8], a[15]);
    array<byte> raw = new(32);
    foreach (var (i, _) in raw) {
        raw[i] = (byte)(i + 1);
    }
    var k = ticketFromBytes(raw);
    fmt.Println(len(k.aesKey), len(k.hmacKey), k.aesKey[0], k.aesKey[15], k.hmacKey[0], k.hmacKey[15], k.seq);
    var bx = makeBox();
    fmt.Println(bx.id, len(bx.t.aesKey), bx.t.aesKey[0], bx.t.hmacKey[15]);
    var (d, err) = withDefer();
    fmt.Println(len(d), d[0], d[1], err == default!);
    var l = literalAs3();
    fmt.Println(len(l), l[2]);
    var (n, s) = scalars();
    fmt.Println(n, len(s));
}

} // end main_package

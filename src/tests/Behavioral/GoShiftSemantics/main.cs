namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct word /*num:nuint*/;

partial struct halfword /*num:uint32*/;

partial struct signedword /*num:int64*/;

internal static void Main() {
    var c = slice<nuint>([0, 1, 63, 64, 65, 200]);
    uint64 u = 0x8000000000000001UL;

    foreach (var (_, k) in c) {
        fmt.Println(u.Rsh(k), u.Lsh(k));
    }

    int64 neg = -8;
    int64 pos = 1099511627776L;
    fmt.Println(neg.Rsh(c[3]), neg.Rsh(c[5]), pos.Rsh(c[3]));

    uint32 u32 = 0xDEADBEEFU;
    foreach (var (_, k) in slice<nuint>([31, 32, 40])) {
        fmt.Println(u32.Rsh(k));
    }

    fmt.Println(u >> (int)(c[3] & 63));
    fmt.Println(u >> (int)(c[4] % 64));

    word w = (nuint)0x8000000000000001UL;
    const nuint W = 64;
    nuint h = c[0];
    fmt.Println((nuint)(w >> (W - h)), (nuint)(w << (W - h)));
    foreach (var (_, k) in c) {
        fmt.Println((nuint)(w >> k), (nuint)(w << k));
    }

    halfword hw = 0xDEADBEEFU;
    foreach (var (_, k) in slice<nuint>([31, 32, 40])) {
        fmt.Println((uint32)(hw >> k));
    }

    signedword sw = -8;
    signedword swp = (signedword)(1099511627776L);
    fmt.Println((int64)(sw >> c[3]), (int64)(sw >> c[5]), (int64)(swp >> c[3]));

    var far = slice<nuint>([(nuint)1 << 32, (nuint)(4294967296L + 1)]);
    foreach (var (_, k) in far) {
        fmt.Println((uint32)(hw << k), (uint32)(hw >> k));
    }
    var hc = hw;
    hc <<= far[1];
    fmt.Println((uint32)hc);
    fmt.Println(namedNegativeShift(hw, -1));
}

internal static @string /*msg*/ namedNegativeShift(halfword x, nint n) {
    @string msg = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var e = recover(); if (e != nil) {
                    msg = fmt.Sprint(e);
                }
            }
        }, ref ᒐ);
        msg = fmt.Sprint((uint32)(x << (int64)(n)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

} // end main_package

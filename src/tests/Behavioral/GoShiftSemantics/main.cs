namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct word /*num:nuint*/;

partial struct halfword /*num:uint32*/;

partial struct signedword /*num:int64*/;

internal static void Main() {
    var c = new nuint[]{0, 1, 63, 64, 65, 200}.slice();
    uint64 u = 0x8000000000000001UL;
    foreach (var (_, k) in c) {
        fmt.Println(u.Rsh(k), u.Lsh(k));
    }
    int64 neg = -8;
    int64 pos = 1099511627776L;
    fmt.Println(neg.Rsh(c[3]), neg.Rsh(c[5]), pos.Rsh(c[3]));
    uint32 u32 = 0xDEADBEEFU;
    foreach (var (_, k) in new nuint[]{31, 32, 40}.slice()) {
        fmt.Println(u32.Rsh(k));
    }
    fmt.Println((u >> (int)(((nuint)(c[3] & 63)))));
    fmt.Println((u >> (int)((c[4] % 64))));
    word w = (nuint)0x8000000000000001UL;
    const nuint W = 64;
    nuint h = c[0];
    fmt.Println((nuint)((w >> (int)((W - h)))), (nuint)((w << (int)((W - h)))));
    foreach (var (_, k) in c) {
        fmt.Println((nuint)((w >> (int)(k))), (nuint)((w << (int)(k))));
    }
    halfword hw = 0xDEADBEEFU;
    foreach (var (_, k) in new nuint[]{31, 32, 40}.slice()) {
        fmt.Println((uint32)((hw >> (int)(k))));
    }
    signedword sw = -8;
    signedword swp = (signedword)(1099511627776L);
    fmt.Println((int64)((sw >> (int)(c[3]))), (int64)((sw >> (int)(c[5]))), (int64)((swp >> (int)(c[3]))));
}

} // end main_package

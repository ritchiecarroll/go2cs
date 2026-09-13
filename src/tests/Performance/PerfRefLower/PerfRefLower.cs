namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

[GoType] partial struct felem {
    internal array<uint64> x = new(4);
}

internal static void cmov(ref uint64 @out, uint64 cond, uint64 a, uint64 b) {
    var m = (uint64)0 - ((uint64)(cond & 1));
    @out = (uint64)(((uint64)(m & b)) | ((uint64)(~m & a)));
}

internal static void norm(ref array<uint64> @out, uint64 k) {
    uint64 c0 = default!;
    uint64 c1 = default!;
    cmov(ref c0, k, (uint64)(@out[0] ^ k), @out[0] + k);
    cmov(ref c1, (uint64)(@out[1] & 1), @out[1] + c0, (uint64)(@out[1] ^ c0));
    @out[0] = c0;
    @out[1] += c1;
    @out[2] ^= (uint64)(c0);
    @out[3] += (uint64)(c1 ^ c0);
}

internal static void mix(ref felem e, ref felem t, uint64 k) {
    norm(ref nonnil(ref e).x, k);
    norm(ref nonnil(ref t).x, k + 1);
    for (nint i = 0; i < 4; i++) {
        uint64 s = default!;
        cmov(ref s, (uint64)(t.x[i] & 1), e.x[i] + t.x[i], (uint64)(e.x[i] ^ t.x[i]));
        e.x[i] = s;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object checksumˢ = (@string)"checksum:"u8;
private static readonly object elapsedNsˢ = (@string)"elapsed_ns:"u8;

internal static void Main() {
    var start = time.Now().UnixNano();
    felem e = new();
    felem t = new();
    e.x = new uint64[]{0x9e3779b97f4a7c15UL, 0xbf58476d1ce4e5b9UL, 0x94d049bb133111ebUL, 0x2545f4914f6cdd1dUL}.array();
    t.x = new uint64[]{1, 2, 3, 4}.array();
    for (nint r = 0; r < 20000000; r++) {
        mix(ref e, ref t, (uint64)r);
    }
    var sum = (uint64)((uint64)((uint64)((uint64)((uint64)(e.x[0] ^ e.x[1]) ^ e.x[2]) ^ e.x[3]) ^ t.x[0]) ^ t.x[3]);
    var elapsed = time.Now().UnixNano() - start;
    fmt.Println(checksumˢ, sum);
    fmt.Println(elapsedNsˢ, elapsed);
}

} // end main_package

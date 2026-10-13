namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object uintptrWordRoundTripsˢ = (@string)"uintptr word round-trips:"u8;
private static readonly object liveChannelWordIsNonNilˢ = (@string)"live channel word is non-nil:"u8;
private static readonly object reReadAgreesˢ = (@string)"re-read agrees:"u8;

internal static void Main() {
    ref var u = ref heap<uintptr>(out var Ꮡu);
    u = (uintptr)0xC0FFEE;
    @unsafe.Pointer q = ~Ꮡ(new @unsafe.Pointer(~Ꮡu));
    fmt.Println(uintptrWordRoundTripsˢ, (uintptr)q == 0xC0FFEE);

    ref var c = ref heap<channel<nint>>(out var Ꮡc);
    c = new channel<nint>(1);
    @unsafe.Pointer p = ~Ꮡ(new @unsafe.Pointer((uintptr)Ꮡc));
    fmt.Println(liveChannelWordIsNonNilˢ, p != nil);

    @unsafe.Pointer p2 = ~Ꮡ(new @unsafe.Pointer((uintptr)Ꮡc));
    fmt.Println(reReadAgreesˢ, (uintptr)p == (uintptr)p2);
}

} // end main_package

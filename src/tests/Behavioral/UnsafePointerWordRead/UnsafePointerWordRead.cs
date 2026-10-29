namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    ref var u = ref heap<uintptr>((uintptr)0xC0FFEE, out var Ꮡu);
    @unsafe.Pointer q = ~Ꮡ(new @unsafe.Pointer(~Ꮡu));
    fmt.Println("uintptr word round-trips:", (uintptr)q == 0xC0FFEE);

    ref var c = ref heap<channel<nint>>(new channel<nint>(1), out var Ꮡc);
    @unsafe.Pointer p = ~Ꮡ(new @unsafe.Pointer((uintptr)Ꮡc));
    fmt.Println("live channel word is non-nil:", p != nil);

    @unsafe.Pointer p2 = ~Ꮡ(new @unsafe.Pointer((uintptr)Ꮡc));
    fmt.Println("re-read agrees:", (uintptr)p == (uintptr)p2);
}

} // end main_package

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    ref var a = ref heap<array<nint>>(new nint[]{10, 20, 30}.array(), out var Ꮡa);
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡa);
    nint sum = 0;
    foreach (var (i, x) in ((ж<array<nint>>)(uintptr)(p)).Value) {
        sum += i + x;
    }
    fmt.Println(sum);
}

} // end main_package

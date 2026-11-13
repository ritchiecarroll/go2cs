namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void main() {
    ref var x = ref heap(new int32(), out var Ꮡx);
    x = 0x01020304;
    @unsafe.Pointer up = @unsafe.Pointer.FromPinnedBox(Ꮡx);

    var q = ((ж<int32>)(uintptr)(up)).Reinterpret<int32, int8>();
    fmt.Println(q.Value);

    q.Value = 9;
    fmt.Println(x == 0x01020309);

    var w = ((ж<int32>)(uintptr)(up)).Reinterpret<int32, uint16>();
    fmt.Println(w.Value);
}

} // end main_package

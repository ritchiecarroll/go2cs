namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void swap(uintptr p, uintptr q) {
    (((ж<uint64>)(uintptr)((@unsafe.Pointer)p)).Value, ((ж<uint64>)(uintptr)((@unsafe.Pointer)q)).Value) = (~(ж<uint64>)(uintptr)((@unsafe.Pointer)q), ~(ж<uint64>)(uintptr)((@unsafe.Pointer)p));
}

internal static void rotate(@unsafe.Pointer a, @unsafe.Pointer b, @unsafe.Pointer c) {
    (((ж<int32>)(uintptr)(a)).Value, ((ж<int32>)(uintptr)(b)).Value, ((ж<int32>)(uintptr)(c)).Value) = (~(ж<int32>)(uintptr)(b), ~(ж<int32>)(uintptr)(c), ~(ж<int32>)(uintptr)(a));
}

internal static void main() {
    var u = slice<uint64>([1, 2]);
    swap((uintptr)Ꮡ(u, 0), (uintptr)Ꮡ(u, 1));
    fmt.Println(u);

    ref var v = ref heap<array<int32>>(new int32[]{7, 8, 9}.array(), out var Ꮡv);
    rotate(@unsafe.Pointer.FromPinnedBox(Ꮡv.at<int32>(0)), @unsafe.Pointer.FromPinnedBox(Ꮡv.at<int32>(1)), @unsafe.Pointer.FromPinnedBox(Ꮡv.at<int32>(2)));
    fmt.Println(v);

    ref var w = ref heap<uint64>((uint64)5, out var Ꮡw);
    swap((uintptr)Ꮡw, (uintptr)Ꮡw);
    fmt.Println(w);
}

} // end main_package

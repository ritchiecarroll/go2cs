namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void swap(uintptr p, uintptr q) {
    ((@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value, (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)q)).Value) = (~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)q), ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p));
}

internal static void rotate(@unsafe.Pointer a, @unsafe.Pointer b, @unsafe.Pointer c) {
    ((@unsafe.Pointer.To<int32>(a)).Value, (@unsafe.Pointer.To<int32>(b)).Value, (@unsafe.Pointer.To<int32>(c)).Value) = (~@unsafe.Pointer.To<int32>(b), ~@unsafe.Pointer.To<int32>(c), ~@unsafe.Pointer.To<int32>(a));
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

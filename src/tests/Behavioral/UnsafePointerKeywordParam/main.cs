namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct holder {
    internal @unsafe.Pointer p;
}

internal static bool sameAs(@unsafe.Pointer old, @unsafe.Pointer @new) {
    return @new == old;
}

internal static bool notNil(@unsafe.Pointer @new) {
    return @new != nil;
}

internal static uintptr asUintptr(@unsafe.Pointer @new) {
    return (uintptr)@new;
}

internal static int32 deref(@unsafe.Pointer @new) {
    return ~(ж<int32>)(uintptr)(@new);
}

internal static void store(ref holder h, @unsafe.Pointer @new) {
    h.p = @new;
}

internal static @unsafe.Pointer offset(@unsafe.Pointer @new, uintptr d) {
    return (@unsafe.Pointer)((uintptr)@new + d);
}

internal static uintptr pass(@unsafe.Pointer @new) {
    return asUintptr(@new);
}

internal static void Main() {
    ref var x = ref heap(new int32(), out var Ꮡx);
    x = 7;
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 1;
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 2;
    @unsafe.Pointer pa = @unsafe.Pointer.FromPinnedBox(Ꮡa);
    @unsafe.Pointer pb = @unsafe.Pointer.FromPinnedBox(Ꮡb);
    @unsafe.Pointer px = @unsafe.Pointer.FromPinnedBox(Ꮡx);
    fmt.Println(sameAs(pa, pa));
    fmt.Println(sameAs(pa, pb));
    fmt.Println(notNil(pa));
    fmt.Println(notNil(nil));
    fmt.Println(asUintptr(px) != 0);
    fmt.Println(deref(px));
    holder h = default!;
    store(ref h, px);
    fmt.Println(h.p == px);
    fmt.Println((uintptr)offset(px, 0) == px);
    fmt.Println(pass(px) == asUintptr(px));
}

} // end main_package

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static @unsafe.Pointer rebind(@unsafe.Pointer p, @unsafe.Pointer q) {
    p = q;
    return p;
}

internal static @unsafe.Pointer advance(@unsafe.Pointer p, uintptr n) {
    p = (@unsafe.Pointer)((uintptr)p + n);
    return p;
}

internal static @unsafe.Pointer step(@unsafe.Pointer p, nint n) {
    p = (uintptr)@unsafe.Add(p, n);
    return p;
}

internal static bool clear(@unsafe.Pointer p) {
    p = default!;
    return p == nil;
}

internal static @unsafe.Pointer local(@unsafe.Pointer q) {
    @unsafe.Pointer p = default!;
    p = q;
    return p;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object rebindˢ = (@string)"rebind:"u8;
private static readonly object callerSPaStillReadsˢ = (@string)"caller's pa still reads"u8;
private static readonly object advanceˢ = (@string)"advance:"u8;
private static readonly object callerSP0StillReadsˢ = (@string)"caller's p0 still reads"u8;
private static readonly object stepˢ = (@string)"step:"u8;
private static readonly object clearˢ = (@string)"clear:"u8;
private static readonly object localˢ = (@string)"local:"u8;

internal static void Main() {
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 1;
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 2;
    @unsafe.Pointer pa = @unsafe.Pointer.FromPinnedBox(Ꮡa);
    @unsafe.Pointer pb = @unsafe.Pointer.FromPinnedBox(Ꮡb);
    @unsafe.Pointer r = (uintptr)rebind(pa, pb);
    fmt.Println(rebindˢ, ~(ж<nint>)(uintptr)(r), callerSPaStillReadsˢ, ~(ж<nint>)(uintptr)(pa));
    ref var arr = ref heap<array<int32>>(out var Ꮡarr);
    arr = new int32[]{10, 20, 30, 40}.array();
    @unsafe.Pointer p0 = @unsafe.Pointer.FromPinnedBox(Ꮡarr.at<int32>(0));
    @unsafe.Pointer s = (uintptr)advance(p0, /* unsafe.Sizeof(arr[0]) */ (uintptr)4);
    fmt.Println(advanceˢ, ~(ж<int32>)(uintptr)(s), callerSP0StillReadsˢ, ~(ж<int32>)(uintptr)(p0));
    @unsafe.Pointer t = (uintptr)step(p0, 2 * (nint)/* unsafe.Sizeof(arr[0]) */ (uintptr)4);
    fmt.Println(stepˢ, ~(ж<int32>)(uintptr)(t), callerSP0StillReadsˢ, ~(ж<int32>)(uintptr)(p0));
    fmt.Println(clearˢ, clear(pa), callerSPaStillReadsˢ, ~(ж<nint>)(uintptr)(pa));
    fmt.Println(localˢ, ~(ж<nint>)(uintptr)(local(pb)), a, b);
}

} // end main_package

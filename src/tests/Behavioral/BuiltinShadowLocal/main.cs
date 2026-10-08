namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint sumWithLenLocal(slice<nint> buf) {
    if (len(buf) == 0) {
        return 0;
    }
    nint lenΔ1 = len(buf);
    nint total = 0;
    for (nint i = 0; i < lenΔ1; i++) {
        total += buf[i];
    }
    return total + lenΔ1;
}

internal static nint capPlusOne(slice<nint> s) {
    nint capΔ1 = cap(s);
    return capΔ1 + 1;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sigkillˢ = "SIGKILL"u8;
private static readonly @string sigˢ = "SIG?"u8;

internal static @string signame(nint sig) {
    if (sig == 9) {
        return sigkillˢ;
    }
    return sigˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noneˢ = "none"u8;

internal static @string describeSignal(nint sig) {
    @string signameΔ1 = signame(sig);
    if (signameΔ1 != ""u8) {
        return "["u8 + signameΔ1 + "]"u8;
    }
    return noneˢ;
}

partial struct arr /*[3]nint*/;

partial struct counter {
    internal nint n;
}

internal static nint sumHeap(slice<nint> heap) {
    ref var c = ref builtin.heap(new counter(), out var Ꮡc);
    var p = Ꮡc;
    foreach (var (_, v) in heap) {
        p.Value.n += v;
    }
    return c.n;
}

internal static nint scaleHeap(nint heapʗp, nint factor) {
    ref var heap = ref builtin.heap(heapʗp, out var Ꮡheap);

    var p = Ꮡheap;
    p.Value *= factor;
    return heap;
}

internal static (nint, nint) unshadowed() {
    var s = new slice<nint>(2, 5);
    return (len(s), cap(s));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object shadowedˢ = (@string)"shadowed"u8;

internal static void shadowedCalls() {
    ref var a = ref heap<arr>(out var Ꮡa);
    a = new arr(new nint[]{1, 2, 3}.array());
    nint make(nint n) => n * 2;
    nint @new(nint n) => n * 3;
    nint panic(nint n) => n * 4;
    nint print(nint n) => n * 5;
    nint println(nint n) => n * 6;
    nint len(ж<arr> p) => p.Value[0] + 100;
    nint cap(ж<arr> p) => p.Value[1] + 200;
    fmt.Println(shadowedˢ, make(21), @new(7), panic(5));
    fmt.Println(shadowedˢ, print(4), println(3), len(Ꮡa), cap(Ꮡa));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object heapˢ = (@string)"heap"u8;
private static readonly object builtinˢ = (@string)"builtin"u8;

internal static void Main() {
    fmt.Println(sumWithLenLocal(new nint[]{10, 20, 30}.slice()));
    fmt.Println(sumWithLenLocal(default!));
    fmt.Println(capPlusOne(new slice<nint>(2, 5)));
    fmt.Println(describeSignal(9));
    fmt.Println(describeSignal(1));
    shadowedCalls();
    fmt.Println(heapˢ, sumHeap(new nint[]{4, 5, 6}.slice()), scaleHeap(7, 6));
    var (l, c) = unshadowed();
    fmt.Println(builtinˢ, l, c);
}

} // end main_package

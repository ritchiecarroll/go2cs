namespace go;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using @unsafe = unsafe_package;

partial class main_package {

internal static bool blankImportInfersUnsafe() {
    ref var @base = ref heap(new int64(), out var Ꮡbase);
    @base = 7;
    @unsafe.Pointer p = (uintptr)ptrOf(Ꮡbase);
    return p == nil;
}

internal static (nint, bool) pair() { return (21, true); }

internal static nint discardInBlankImportFile() {
    var (n, _) = pair();
    nint x = 0;
    (x, _) = pair();
    return n + x;
}

} // end main_package

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    ref var @base = ref heap(new int64(), out var Ꮡbase);
    @base = 100;
    @unsafe.Pointer p = (uintptr)ptrOf(Ꮡbase);

    fmt.Println(p == nil);
    fmt.Println(p != nil);
    fmt.Println(isNil(p));
    fmt.Println(compositeLen());
    fmt.Println(blankImportInfersUnsafe());
    fmt.Println(discardInBlankImportFile());
}

} // end main_package

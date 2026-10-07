namespace go;

using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial @unsafe.Pointer ptrOf(ж<int64> Ꮡx) {
    return @unsafe.Pointer.FromPinnedBox(Ꮡx);
}

internal static partial bool isNil(@unsafe.Pointer p) {
    return p == nil;
}

internal static partial slice<@unsafe.Pointer> makePtrs(ж<int64> Ꮡx) {
    return new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡx)}.slice();
}

} // end main_package

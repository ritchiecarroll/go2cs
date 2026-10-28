namespace go;

using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial @unsafe.Pointer ptrOf(ж<int64> x) {
    return @unsafe.Pointer.FromPinnedBox(x);
}

internal static partial bool isNil(@unsafe.Pointer p) {
    return p == nil;
}

internal static partial slice<@unsafe.Pointer> makePtrs(ж<int64> x) {
    return new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(x)}.slice();
}

} // end main_package

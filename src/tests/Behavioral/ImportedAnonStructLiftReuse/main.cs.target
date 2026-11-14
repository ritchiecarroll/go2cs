global using TLARGE_INTEGER = go.ImportedAnonStructLiftReuse.lib_package.largeIntegerᴛ1;

namespace go;

using fmt = fmt_package;
using lib = ImportedAnonStructLiftReuse.lib_package;
using ImportedAnonStructLiftReuse;

partial class main_package {

internal static void main() {
    TLARGE_INTEGER distance = default;
    distance.QuadPart = 20;
    ref var moved = ref heap(new TLARGE_INTEGER(), out var Ꮡmoved);

    fmt.Println(lib.SetFilePointerEx(distance, Ꮡmoved), moved.QuadPart);

    Func<TLARGE_INTEGER, ж<TLARGE_INTEGER>, int64> seek = lib.SetFilePointerEx;
    fmt.Println(seek(new TLARGE_INTEGER(QuadPart: 5), Ꮡmoved), moved.QuadPart);
    fmt.Printf("%T\n"u8, distance);
}

} // end main_package

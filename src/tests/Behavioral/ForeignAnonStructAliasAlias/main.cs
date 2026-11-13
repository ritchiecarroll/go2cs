global using cIndexConstraint = go.main_package/struct{FiColumn int32; Fop uint8};

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using lib = ForeignAnonStructAliasAlias.lib_package;
using ForeignAnonStructAliasAlias;

partial class main_package {

internal static void main() {
    var c = new cIndexConstraint(FiColumn: 7, Fop: 2);
    fmt.Println(c.FiColumn, c.Fop, /* unsafe.Sizeof(c) */ (uintptr)8);
}

} // end main_package

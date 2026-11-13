namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using stdio = ForeignAliasElementAddress.stdio_package;
using ForeignAliasElementAddress;

partial class main_package {

public static readonly ж<array<stdioꓸFILE>> ᏑX_iob = new StaticBox<array<stdioꓸFILE>>(new array<stdioꓸFILE>(3));
public static ref array<stdioꓸFILE> X_iob => ref ((StaticBox<array<stdioꓸFILE>>)ᏑX_iob).ValueSlot;

internal static void main() {
    var m = new map<uintptr, int32>{};
    foreach (var i in range(len(X_iob))) {
        m[(uintptr)ᏑX_iob.at<stdio.FILE>(i)] = (int32)i;
    }
    X_iob[1].Fcnt = 4;
    fmt.Println(len(m), X_iob[1].Fcnt, /* unsafe.Sizeof(X_iob[0]) */ (uintptr)16);
}

} // end main_package

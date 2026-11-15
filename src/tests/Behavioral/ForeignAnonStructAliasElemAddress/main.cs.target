namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using stdio = ForeignAnonStructAliasElemAddress.stdio_package;
using ForeignAnonStructAliasElemAddress;

partial class main_package {

public static readonly ж<array<stdioꓸFILE>> ᏑX_iob = new StaticBox<array<stdioꓸFILE>>(new array<stdioꓸFILE>(3, () => new()));
public static ref array<stdioꓸFILE> X_iob => ref ((StaticBox<array<stdioꓸFILE>>)ᏑX_iob).ValueSlot;

internal static map<uintptr, int32> iobMap = new map<uintptr, int32>{};

[GoInit] internal static void init() {
    foreach (var i in range(len(X_iob))) {
        iobMap[(uintptr)ᏑX_iob.at<stdioꓸFILE>(i)] = (int32)i;
    }
}

internal static void main() {
    foreach (var i in range(len(X_iob))) {
        var p = ᏑX_iob.at<stdioꓸFILE>(i);
        p.Value.F_cnt = (int32)(10 * (i + 1));
        p.Value.F__ccgo_pad1[i] = (byte)(i + 1);
    }

    var (first, last) = ((uintptr)ᏑX_iob.at<stdioꓸFILE>(0), (uintptr)ᏑX_iob.at<stdioꓸFILE>(2));

    fmt.Println(len(iobMap), iobMap[first], iobMap[last], last - first == 2 * /* unsafe.Sizeof(X_iob[0]) */ (uintptr)24);
    fmt.Println(X_iob[0].F_cnt, X_iob[1].F_cnt, X_iob[2].F_cnt, X_iob[1].F__ccgo_pad1, X_iob[2].F__ccgo_pad1);
}

} // end main_package

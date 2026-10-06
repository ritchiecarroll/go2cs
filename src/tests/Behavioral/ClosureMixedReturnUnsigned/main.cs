namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void run() {
    var pcs = new uintptr[]{10, 20, 30}.slice();
    var pcsʗ1 = pcs;
    uintptr casePC(nint casi) {
        if (pcsʗ1 == default!) {
            return (uintptr)(0);
        }
        return pcsʗ1[casi];
    }
    fmt.Println(casePC(0), casePC(2));
    var vals = new uint64[]{100, 200}.slice();
    var valsʗ1 = vals;
    uint64 getU64(nint i) {
        if (i < 0) {
            return (uint64)(0);
        }
        return valsʗ1[i];
    }
    fmt.Println(getU64(1), getU64(-1));
    var tab = new uint32[]{7, 8, 9}.slice();
    var tabʗ1 = tab;
    uint32 getU32(nint i) {
        if (i >= len(tabʗ1)) {
            return (uint32)(0);
        }
        return tabʗ1[i];
    }
    fmt.Println(getU32(2), getU32(5));
    var sizes = new nuint[]{4, 16, 64}.slice();
    var sizesʗ1 = sizes;
    nuint getU(nint i) {
        if (i < 0) {
            return (nuint)(0);
        }
        return sizesʗ1[i];
    }
    fmt.Println(getU(0), getU(-1));
    nint pick(bool b) {
        if (b) {
            return -1;
        }
        return 42;
    }
    fmt.Println(pick(true), pick(false));
}

internal static void Main() {
    run();
}

} // end main_package

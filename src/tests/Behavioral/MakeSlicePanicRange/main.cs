namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial any /*r*/ tryMake(nint length, nint capacity) {
    any r = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            r = recover();
        }, ref ᒐ);
        slice<byte> b = default!;
        if (capacity < 0){
            b = new slice<byte>(length);
        } else {
            b = new slice<byte>(length, capacity);
        }
        _ = b;
        r = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void Main() {
    fmt.Println(tryMake(4, -1));
    fmt.Println(tryMake(0, -1));
    fmt.Println(tryMake(-1, -1));
    fmt.Println(tryMake(unchecked((nint)(4611686018427387904L)), -1));
    fmt.Println(tryMake(1, unchecked((nint)(4611686018427387904L))));
}

} // end main_package

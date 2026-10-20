namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;

partial class main_package {

partial struct count /*num:int8*/;

internal static nint one = 1;
internal static int64 big = 1099511627776L;
internal static uint32 u32 = 0xF0;
internal static int8 i8 = -128;
internal static nint neg = -1;
internal static int8 neg8 = -2;
internal static int64 neg64 = -3;
internal static nint three = 3;
internal static nint wide = 70;
internal static nint y = -3;
internal static count c = -1;
internal static nuint huge = ((nuint)1 << 63);

internal static void arm(@string @class, Func<@string> f) {
    GoFrame ᒐ = default;
    try {
        if (len(os.Args) > 1 && os.Args[1] != @class) {
            return;
        }

        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    var (_, isRuntimeError) = r._<runtimeꓸError>(ᐧ);
                    fmt.Println(@class + ": panic:", r, isRuntimeError);
                }
            }
        }, ref ᒐ);

        fmt.Println(@class + ":", f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    arm("left int", () => fmt.Sprint(one.Lsh((int64)(neg))));
    arm("right int64 by int8", () => fmt.Sprint(big.Rsh((int64)(neg8))));
    arm("left uint32 by int64", () => fmt.Sprint(u32.Lsh((int64)(neg64))));
    arm("right int8", () => fmt.Sprint(i8.Rsh((int64)(neg))));
    arm("named count", () => fmt.Sprint(big.Lsh((int64)(int8)(c))));
    arm("compound left", () => {
        var x = big;
        x.LshAssign((int64)(neg));
        return fmt.Sprint(x);
    });
    arm("compound right", () => {
        var x = u32;
        x.RshAssign((int64)(neg8));
        return fmt.Sprint(x);
    });
    arm("modulo count", () => fmt.Sprint(big.Lsh((int64)((y % 8)))));
    arm("non-negative signed", () => fmt.Sprint(one.Lsh((int64)(three)), " ", big.Rsh((int64)(three)), " ", u32.Lsh((int64)(wide)), " ", i8.Rsh((int64)(wide))));
    arm("unsigned huge", () => fmt.Sprint(big.Lsh(huge), " ", i8.Rsh(huge)));
    arm("masked signed", () => fmt.Sprint(big << (int)((y & 7))));
}

} // end main_package

namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;

partial class main_package {

[GoType("num:int8")] partial struct count;

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
internal static nuint huge = ((nuint)1 << (int)(63));

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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string leftIntˢ = "left int"u8;
private static readonly @string rightInt64ByInt8ˢ = "right int64 by int8"u8;
private static readonly @string leftUint32ByInt64ˢ = "left uint32 by int64"u8;
private static readonly @string rightInt8ˢ = "right int8"u8;
private static readonly @string namedCountˢ = "named count"u8;
private static readonly @string compoundLeftˢ = "compound left"u8;
private static readonly @string compoundRightˢ = "compound right"u8;
private static readonly @string moduloCountˢ = "modulo count"u8;
private static readonly @string nonNegativeSignedˢ = "non-negative signed"u8;
private static readonly @string unsignedHugeˢ = "unsigned huge"u8;
private static readonly @string maskedSignedˢ = "masked signed"u8;

internal static void Main() {
    arm(leftIntˢ, () => fmt.Sprint(one.Lsh((int64)(neg))));
    arm(rightInt64ByInt8ˢ, () => fmt.Sprint(big.Rsh((int64)(neg8))));
    arm(leftUint32ByInt64ˢ, () => fmt.Sprint(u32.Lsh((int64)(neg64))));
    arm(rightInt8ˢ, () => fmt.Sprint(i8.Rsh((int64)(neg))));
    arm(namedCountˢ, () => fmt.Sprint(big.Lsh((int64)(int8)(c))));
    arm(compoundLeftˢ, () => {
        var x = big;
        x.LshAssign((int64)(neg));
        return fmt.Sprint(x);
    });
    arm(compoundRightˢ, () => {
        var x = u32;
        x.RshAssign((int64)(neg8));
        return fmt.Sprint(x);
    });
    arm(moduloCountˢ, () => fmt.Sprint(big.Lsh((int64)((y % 8)))));
    arm(nonNegativeSignedˢ, () => fmt.Sprint(one.Lsh((int64)(three)), (@string)" "u8, big.Rsh((int64)(three)), (@string)" "u8, u32.Lsh((int64)(wide)), (@string)" "u8, i8.Rsh((int64)(wide))));
    arm(unsignedHugeˢ, () => fmt.Sprint(big.Lsh(huge), (@string)" "u8, i8.Rsh(huge)));
    arm(maskedSignedˢ, () => fmt.Sprint((big << (int)(((nint)(y & 7))))));
}

} // end main_package

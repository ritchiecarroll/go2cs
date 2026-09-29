namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct S {
    internal array<nint> a = new(3);
}

internal static void @try(@string name, Func<ж<nint>> f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Printf("%s: %v\n"u8, name, r);
                }
            }
        }, ref ᒐ);
        f().Value = 7;
        fmt.Printf("%s: wrote\n"u8, name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string pointerToArrayUint64Maxˢ = "pointer-to-array uint64 max"u8;
private static readonly @string pointerToArrayUintPast2ˢ = "pointer-to-array uint past 2^63"u8;
private static readonly @string pointerToArrayUintptrˢ = "pointer-to-array uintptr past 2^63"u8;
private static readonly @string pointerToArrayUint32Maxˢ = "pointer-to-array uint32 max"u8;
private static readonly @string pointerToArrayInRangeˢ = "pointer-to-array in range"u8;
private static readonly @string fieldArrayUint64Maxˢ = "field array uint64 max"u8;
private static readonly @string fieldArrayUintPast263ˢ = "field array uint past 2^63"u8;
private static readonly @string fieldArrayInRangeˢ = "field array in range"u8;

internal static void Main() {
    ref var arr = ref heap(new array<nint>(3), out var Ꮡarr);
    var p = Ꮡarr;
    var ps = Ꮡ(new S(nil));
    uint64 u64 = 18446744073709551615UL;
    nuint u = unchecked((nuint)(9223372036854775810UL));
    uintptr up = unchecked((nuint)(9223372036854775811UL));
    uint32 u32 = unchecked((uint32)(4294967295UL));
    uint64 ok = 2;
    var pʗ1 = p;
    @try(pointerToArrayUint64Maxˢ, () => pʗ1.at<nint>((ulong)(u64)));
    var pʗ2 = p;
    @try(pointerToArrayUintPast2ˢ, () => pʗ2.at<nint>((ulong)(u)));
    var pʗ3 = p;
    @try(pointerToArrayUintptrˢ, () => pʗ3.at<nint>((ulong)(up)));
    var pʗ4 = p;
    @try(pointerToArrayUint32Maxˢ, () => pʗ4.at<nint>((ulong)(u32)));
    var pʗ5 = p;
    @try(pointerToArrayInRangeˢ, () => pʗ5.at<nint>((ulong)(ok)));
    var psʗ1 = ps;
    @try(fieldArrayUint64Maxˢ, () => psʗ1.at(S.Ꮡa, (ulong)(u64)));
    var psʗ2 = ps;
    @try(fieldArrayUintPast263ˢ, () => psʗ2.at(S.Ꮡa, (ulong)(u)));
    var psʗ3 = ps;
    @try(fieldArrayInRangeˢ, () => psʗ3.at(S.Ꮡa, (ulong)(ok)));
    fmt.Println(arr, (~ps).a);
}

} // end main_package

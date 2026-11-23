namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct S {
    internal array<nint> a = new(3);
}

internal static void @try(@string name, Func<ж<nint>> f) {
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("try.func1")] () => {
            var r = recover(); if (r != nil) {
                fmt.Printf("%s: %v\n"u8, name, r);
            }
        }, ref ᒐ);
        f().Value = 7;
        fmt.Printf("%s: wrote\n"u8, name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void main() {
    ref var arr = ref heap(new array<nint>(3), out var Ꮡarr);
    var p = Ꮡarr;
    var ps = Ꮡ(new S());

    uint64 u64 = 18446744073709551615UL;
    nuint u = unchecked((nuint)(9223372036854775810UL));
    uintptr up = unchecked((nuint)(9223372036854775811UL));
    uint32 u32 = unchecked((uint32)(4294967295UL));
    uint64 ok = 2;

    var pʗ1 = p;
    @try("pointer-to-array uint64 max", [GoFunc("main.func1")] () => pʗ1.at<nint>((ulong)(u64)));
    var pʗ2 = p;
    @try("pointer-to-array uint past 2^63", [GoFunc("main.func2")] () => pʗ2.at<nint>((ulong)(u)));
    var pʗ3 = p;
    @try("pointer-to-array uintptr past 2^63", [GoFunc("main.func3")] () => pʗ3.at<nint>((ulong)(up)));
    var pʗ4 = p;
    @try("pointer-to-array uint32 max", [GoFunc("main.func4")] () => pʗ4.at<nint>((ulong)(u32)));
    var pʗ5 = p;
    @try("pointer-to-array in range", [GoFunc("main.func5")] () => pʗ5.at<nint>((ulong)(ok)));
    var psʗ1 = ps;
    @try("field array uint64 max", [GoFunc("main.func6")] () => psʗ1.at(S.Ꮡa, (ulong)(u64)));
    var psʗ2 = ps;
    @try("field array uint past 2^63", [GoFunc("main.func7")] () => psʗ2.at(S.Ꮡa, (ulong)(u)));
    var psʗ3 = ps;
    @try("field array in range", [GoFunc("main.func8")] () => psʗ3.at(S.Ꮡa, (ulong)(ok)));

    fmt.Println(arr, ps.a);
}

} // end main_package

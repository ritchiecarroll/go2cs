namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object recoveredˢ = (@string)"recovered:"u8;
private static readonly object noPanicˢ = (@string)"no panic"u8;

internal static void arm(@string name, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println(name, recoveredˢ, r);
                }
            }
        }, ref ᒐ);
        f();
        fmt.Println(name, noPanicˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string addressUint64ˢ = "address uint64:"u8;
private static readonly @string addressInt64ˢ = "address int64:"u8;
private static readonly @string addressIntˢ = "address int:"u8;
private static readonly @string addressPastLengthˢ = "address past length:"u8;
private static readonly object s65ˢ = (@string)"s[:6][5] ="u8;
private static readonly @string indexBoundˢ = "3-index bound:"u8;
private static readonly object lenˢ = (@string)"len"u8;
private static readonly object capˢ = (@string)"cap"u8;
private static readonly @string elementMethodˢ = "element method:"u8;
private static readonly @string literalIndexˢ = "literal index:"u8;
private static readonly object byteˢ = (@string)"byte"u8;
private static readonly @string literalNegativeIndexˢ = "literal negative index:"u8;
private static readonly @string stringPastEndˢ = "string past end:"u8;
private static readonly @string abcˢ = "abc"u8;
private static readonly @string stringLowPastEndˢ = "string low past end:"u8;

internal static void Main() {
    uint64 wideU = (uint64)(4294967296L + 5);
    int64 wideI = 4294967301L;
    nint wideInt = unchecked((nint)(4294967301L));
    arm(addressUint64ˢ, () => {
        var a = new slice<nint>(10);
        var p = Ꮡ(a, wideU);
        p.Value = 7;
        fmt.Println((@string)"a[5] ="u8, a[5]);
    });
    arm(addressInt64ˢ, () => {
        var a = new slice<nint>(10);
        var p = Ꮡ(a, (nint)(wideI));
        p.Value = 7;
        fmt.Println((@string)"a[5] ="u8, a[5]);
    });
    arm(addressIntˢ, () => {
        var a = new slice<nint>(10);
        var p = Ꮡ(a, wideInt);
        p.Value = 7;
        fmt.Println((@string)"a[5] ="u8, a[5]);
    });
    arm(addressPastLengthˢ, () => {
        var s = new slice<nint>(3, 10);
        nint i = 5;
        var p = Ꮡ(s, i);
        p.Value = 7;
        fmt.Println(s65ˢ, s[..6][5]);
    });
    arm(indexBoundˢ, () => {
        var s = new slice<nint>(10);
        var t = s.slice(0, (nint)(wideU), (nint)(wideU));
        fmt.Println(lenˢ, len(t), capˢ, cap(t));
    });
    arm(elementMethodˢ, () => {
        var a = new slice<atomic.Uint64>(10);
        Ꮡ(a, wideU).Add(1);
        fmt.Println((@string)"a[5] ="u8, Ꮡ(a, 5).Load());
    });
    nint past = 5;
    arm(literalIndexˢ, () => {
        fmt.Println(byteˢ, LiteralByteAt("abc"u8, past));
    });
    nint neg = -1;
    arm(literalNegativeIndexˢ, () => {
        fmt.Println(byteˢ, LiteralByteAt("abc"u8, neg));
    });
    arm(stringPastEndˢ, () => {
        @string s = abcˢ;
        fmt.Println(s.slice(1, past));
    });
    nint low = 4;
    arm(stringLowPastEndˢ, () => {
        @string s = abcˢ;
        fmt.Println(s.slice(low));
    });
}

} // end main_package

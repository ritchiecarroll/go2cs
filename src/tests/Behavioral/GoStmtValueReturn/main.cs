namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static channel<nint> gch = new channel<nint>(1);

internal static nint sum(channel<nint> @out, nint a, nint b) {
    nint r = a + b;
    @out.ᐸꟷ(r);
    return r;
}

internal static (nint, error) pair(channel<nint> @out, nint n) {
    @out.ᐸꟷ(n * n);
    return (n * n, default!);
}

internal static nint nib() {
    gch.ᐸꟷ(5);
    return 5;
}

internal static void emit(channel<nint> @out) {
    @out.ᐸꟷ(8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sumˢ = (@string)"sum:"u8;
private static readonly object pairˢ = (@string)"pair:"u8;
private static readonly object nibˢ = (@string)"nib:"u8;
private static readonly object litˢ = (@string)"lit:"u8;
private static readonly object emitˢ = (@string)"emit:"u8;
private static readonly object litvalˢ = (@string)"litval:"u8;
private static readonly object litmultiˢ = (@string)"litmulti:"u8;

internal static partial void Main() {
    var @out = new channel<nint>(0);
    goǃ((ᴛ1, ᴛ2, ᴛ3) => sum(ᴛ1, ᴛ2, ᴛ3), @out, 3, 4);
    fmt.Println(sumˢ, ᐸꟷ(@out));
    goǃ((ᴛ1, ᴛ2) => pair(ᴛ1, ᴛ2), @out, 6);
    fmt.Println(pairˢ, ᐸꟷ(@out));
    goǃ(() => nib());
    fmt.Println(nibˢ, ᐸꟷ(gch));
    var outʗ1 = @out;
    goǃ(() => {
        outʗ1.ᐸꟷ(1);
    });
    fmt.Println(litˢ, ᐸꟷ(@out));
    goǃ(emit, @out);
    fmt.Println(emitˢ, ᐸꟷ(@out));
    goǃ((channel<nint> o) => {
        error retErr = default!;
        GoFrame ᒐ = default;
        try {
            var oʗ1 = o;
            defer(() => {
                oʗ1.ᐸꟷ(11);
            }, ref ᒐ);
            retErr = default!; goto ᒐdone;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        ᒐdone: return retErr;
    }, @out);
    fmt.Println(litvalˢ, ᐸꟷ(@out));
    goǃ(error (channel<nint> o, nint n) => {
        o.ᐸꟷ(n * 2);
        if (n < 0) {
            return fmt.Errorf("negative %d"u8, n);
        }
        return default!;
    }, @out, (nint)(6));
    fmt.Println(litmultiˢ, ᐸꟷ(@out));
}

} // end main_package

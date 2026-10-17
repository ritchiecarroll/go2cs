namespace go;

using fmt = fmt_package;
using runtime = runtime_package;

partial class main_package {

partial struct T {
    internal ж<byte> p;
    internal nint f;
}

internal static void @try(@string name, Action register) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Printf("%s: panic: %v\n"u8, name, r);
                }
            }
        }, ref ᒐ);
        register();
        fmt.Printf("%s: registered\n"u8, name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void nothing(nint _) {
}

internal static void release(this ref T t, nint _) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string capturesPtrˢ = "captures ptr"u8;
private static readonly @string capturesFieldPointerˢ = "captures field pointer"u8;
private static readonly @string capturesOtherObjectˢ = "captures other object"u8;
private static readonly @string capturesValueˢ = "captures value"u8;
private static readonly @string capturesNothingˢ = "captures nothing"u8;
private static readonly @string siblingCapturesPtrˢ = "sibling captures ptr"u8;
private static readonly object siblingReadsˢ = (@string)"sibling reads"u8;
private static readonly @string methodValueOnPtrˢ = "method value on ptr"u8;
private static readonly @string reassignedAfterCaptureˢ = "reassigned after capture"u8;
private static readonly object reassignedIsNilˢ = (@string)"reassigned is nil:"u8;
private static readonly @string argIsPtrˢ = "arg is ptr"u8;

internal static void Main() {
    @try(capturesPtrˢ, () => {
        var v = Ꮡ(new T(f: 1));
        var vʗ1 = v;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = vʗ1.Value.f;
        }, 0, new object?[] { v });
    });

    @try(capturesFieldPointerˢ, () => {
        var v = Ꮡ(new T(f: 2));
        var fp = v.of(T.Ꮡf);
        var fpʗ1 = fp;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = fpʗ1.Value;
        }, 0, new object?[] { fp });
    });

    @try(capturesOtherObjectˢ, () => {
        var v = Ꮡ(new T(f: 3));
        var w = Ꮡ(new T(f: 4));
        var wʗ1 = w;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = wʗ1.Value.f;
        }, 0, new object?[] { w });
        runtime.KeepAlive(w.OrTypedNil());
    });

    @try(capturesValueˢ, () => {
        var v = Ꮡ(new T(f: 5));
        nint n = v.Value.f;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = n;
        }, 0);
    });

    @try(capturesNothingˢ, () => {
        var v = Ꮡ(new T(f: 6));
        runtime.AddCleanup<T, nint>(v, nothing, 0);
    });

    @try(siblingCapturesPtrˢ, () => {
        var v = Ꮡ(new T(f: 7));
        var w = Ꮡ(new T(f: 8));
        var vʗ2 = v;
        nint sibling() => (~vʗ2).f;
        var wʗ2 = w;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = wʗ2.Value.f;
        }, 0, new object?[] { w });
        fmt.Println(siblingReadsˢ, sibling());
        runtime.KeepAlive(w.OrTypedNil());
    });

    @try(methodValueOnPtrˢ, () => {
        var v = Ꮡ(new T(f: 10));
        runtime.AddCleanup<T, nint>(v, v.release, 0, new object?[] { v });
    });

    @try(reassignedAfterCaptureˢ, () => {
        ref var v = ref heap<ж<T>>(out var Ꮡv);
        Ꮡv.ValueSlot = Ꮡ(new T(f: 11));
        runtime.AddCleanup(Ꮡv.ValueSlot, (nint _Δp0) => {
            _ = Ꮡv.ValueSlot;
        }, 0);
        Ꮡv.ValueSlot = default!;
        fmt.Println(reassignedIsNilˢ, Ꮡv.ValueSlot == nil);
    });

    @try(argIsPtrˢ, () => {
        var v = Ꮡ(new T(f: 9));
        runtime.AddCleanup(v, (ж<T> _Δp0) => {
        }, v);
    });
}

} // end main_package

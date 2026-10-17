namespace go;

using fmt = fmt_package;
using runtime = runtime_package;

partial class main_package {

partial struct T {
    internal ж<byte> p;
    internal nint f;
}

partial struct pair {
    internal ж<T> p;
    internal nint n;
}

partial struct outer {
    internal pair @in;
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
private static readonly @string funcVariableCapturesPtrˢ = "func variable captures ptr"u8;
private static readonly @string funcVariableCapturesˢ = "func variable captures other object"u8;
private static readonly @string funcVariableReassignedToˢ = "func variable reassigned to method value"u8;
private static readonly @string structCaptureHoldsPtrˢ = "struct capture holds ptr"u8;
private static readonly @string arrayCaptureHoldsPtrˢ = "array capture holds ptr"u8;
private static readonly @string nestedStructCaptureHoldsˢ = "nested struct capture holds ptr"u8;
private static readonly @string structCaptureHoldsOtherˢ = "struct capture holds other object"u8;
private static readonly @string interfaceHoldingAStructˢ = "interface holding a struct that holds ptr"u8;
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

    @try(funcVariableCapturesPtrˢ, () => {
        var v = Ꮡ(new T(f: 12));
        var vʗ3 = v;
        var cleanup = (nint _Δp0) => {
            _ = vʗ3.Value.f;
        };
        runtime.AddCleanup(v, cleanup, 0, new object?[] { v });
    });

    @try(funcVariableCapturesˢ, () => {
        var v = Ꮡ(new T(f: 13));
        var w = Ꮡ(new T(f: 14));
        var wʗ3 = w;
        var cleanup = (nint _Δp0) => {
            _ = wʗ3.Value.f;
        };
        runtime.AddCleanup(v, cleanup, 0, new object?[] { w });
        runtime.KeepAlive(w.OrTypedNil());
    });

    @try(funcVariableReassignedToˢ, () => {
        var v = Ꮡ(new T(f: 15));
        var cleanup = (nint _Δp0) => {
        };
        var vʗ4 = v;
                cleanup = (nint p1) => vʗ4.release(p1);
        runtime.AddCleanup(v, cleanup, 0, new object?[] { v });
    });

    @try(structCaptureHoldsPtrˢ, () => {
        var v = Ꮡ(new T(f: 16));
        ref var p = ref heap<pair>(out var Ꮡp);
        p = new pair(v, 1);
        var pʗ1 = p;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = pʗ1;
        }, 0);
    });

    @try(arrayCaptureHoldsPtrˢ, () => {
        var v = Ꮡ(new T(f: 17));
        ref var a = ref heap<array<ж<T>>>(out var Ꮡa);
        a = new ж<T>[]{default!, v}.array();
        var aʗ1 = a;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = aʗ1;
        }, 0);
    });

    @try(nestedStructCaptureHoldsˢ, () => {
        var v = Ꮡ(new T(f: 18));
        ref var o = ref heap<outer>(out var Ꮡo);
        o = new outer(new pair(v, 2));
        var oʗ1 = o;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = oʗ1;
        }, 0);
    });

    @try(structCaptureHoldsOtherˢ, () => {
        var v = Ꮡ(new T(f: 19));
        var w = Ꮡ(new T(f: 20));
        ref var p = ref heap<pair>(out var Ꮡp);
        p = new pair(w, 3);
        var pʗ2 = p;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = pʗ2;
        }, 0);
        runtime.KeepAlive(w.OrTypedNil());
    });

    @try(interfaceHoldingAStructˢ, () => {
        var v = Ꮡ(new T(f: 21));
        any i = new pair(v, 4);
        var iʗ1 = i;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = iʗ1;
        }, 0, new object?[] { i });
    });

    @try(argIsPtrˢ, () => {
        var v = Ꮡ(new T(f: 9));
        runtime.AddCleanup(v, (ж<T> _Δp0) => {
        }, v);
    });
}

} // end main_package

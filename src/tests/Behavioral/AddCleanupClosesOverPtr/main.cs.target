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

partial struct large {
    internal ж<T> p;
    internal array<int64> pad = new(16);
}

internal static void @try(@string name, Action register) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
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

internal static void Main() {
    @try("captures ptr", () => {
        var v = Ꮡ(new T(f: 1));
        var vʗ1 = v;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = vʗ1.f;
        }, 0, new object?[] { v });
    });

    @try("captures field pointer", () => {
        var v = Ꮡ(new T(f: 2));
        var fp = v.of(T.Ꮡf);
        var fpʗ1 = fp;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = fpʗ1.Value;
        }, 0, new object?[] { fp });
    });

    @try("captures other object", () => {
        var v = Ꮡ(new T(f: 3));
        var w = Ꮡ(new T(f: 4));
        var wʗ1 = w;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = wʗ1.f;
        }, 0, new object?[] { w });
        runtime.KeepAlive(w.OrTypedNil());
    });

    @try("captures value", () => {
        var v = Ꮡ(new T(f: 5));
        nint n = v.f;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = n;
        }, 0);
    });

    @try("captures nothing", () => {
        var v = Ꮡ(new T(f: 6));
        runtime.AddCleanup<T, nint>(v, nothing, 0);
    });

    @try("sibling captures ptr", () => {
        var v = Ꮡ(new T(f: 7));
        var w = Ꮡ(new T(f: 8));
        var vʗ2 = v;
        nint sibling() => vʗ2.f;
        var wʗ2 = w;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = wʗ2.f;
        }, 0, new object?[] { w });
        fmt.Println("sibling reads", sibling());
        runtime.KeepAlive(w.OrTypedNil());
    });

    @try("method value on ptr", () => {
        var v = Ꮡ(new T(f: 10));
        runtime.AddCleanup<T, nint>(v, v.release, 0, new object?[] { v });
    });

    @try("reassigned after capture", () => {
        ref var v = ref heap<ж<T>>(Ꮡ(new T(f: 11)), out var Ꮡv);
        runtime.AddCleanup(Ꮡv.ValueSlot, (nint _Δp0) => {
            _ = Ꮡv.ValueSlot;
        }, 0);
        Ꮡv.ValueSlot = default;
        fmt.Println("reassigned is nil:", Ꮡv.ValueSlot == nil);
    });

    @try("func variable captures ptr", () => {
        var v = Ꮡ(new T(f: 12));
        var vʗ3 = v;
        var cleanup = (nint _Δp0) => {
            _ = vʗ3.f;
        };
        runtime.AddCleanup(v, cleanup, 0, new object?[] { v });
    });

    @try("func variable captures other object", () => {
        var v = Ꮡ(new T(f: 13));
        var w = Ꮡ(new T(f: 14));
        var wʗ3 = w;
        var cleanup = (nint _Δp0) => {
            _ = wʗ3.f;
        };
        runtime.AddCleanup(v, cleanup, 0, new object?[] { w });
        runtime.KeepAlive(w.OrTypedNil());
    });

    @try("func variable reassigned to method value", () => {
        var v = Ꮡ(new T(f: 15));
        var cleanup = (nint _Δp0) => {
        };
        var vʗ4 = v;
                cleanup = (nint p1) => vʗ4.release(p1);
        runtime.AddCleanup(v, cleanup, 0, new object?[] { v });
    });

    @try("struct capture holds ptr", () => {
        var v = Ꮡ(new T(f: 16));
        ref var p = ref heap<pair>(new pair(v, 1), out var Ꮡp);
        var pʗ1 = p;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = pʗ1;
        }, 0, null, new object?[] { p });
    });

    @try("array capture holds ptr", () => {
        var v = Ꮡ(new T(f: 17));
        ref var a = ref heap<array<ж<T>>>(new ж<T>[]{default, v}.array(), out var Ꮡa);
        var aʗ1 = a;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = aʗ1;
        }, 0, null, new object?[] { a });
    });

    @try("nested struct capture holds ptr", () => {
        var v = Ꮡ(new T(f: 18));
        ref var o = ref heap<outer>(new outer(new pair(v, 2)), out var Ꮡo);
        var oʗ1 = o;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = oʗ1;
        }, 0, null, new object?[] { o });
    });

    @try("struct capture holds other object", () => {
        var v = Ꮡ(new T(f: 19));
        var w = Ꮡ(new T(f: 20));
        ref var p = ref heap<pair>(new pair(w, 3), out var Ꮡp);
        var pʗ2 = p;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = pʗ2;
        }, 0, null, new object?[] { p });
        runtime.KeepAlive(w.OrTypedNil());
    });

    @try("struct over 128 bytes holds ptr", () => {
        var v = Ꮡ(new T(f: 22));
        ref var l = ref heap<large>(new large(p: v), out var Ꮡl);
        var lʗ1 = l;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = lʗ1;
        }, 0);
    });

    @try("interface holding a struct that holds ptr", () => {
        var v = Ꮡ(new T(f: 21));
        any i = new pair(v, 4);
        var iʗ1 = i;
        runtime.AddCleanup(v, (nint _Δp0) => {
            _ = iʗ1;
        }, 0, new object?[] { i });
    });

    @try("arg is ptr", () => {
        var v = Ꮡ(new T(f: 9));
        runtime.AddCleanup(v, (ж<T> _Δp0) => {
        }, v);
    });
}

} // end main_package

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
        defer([GoFunc("try.func1")] () => {
            var r = recover(); if (r != nil) {
                fmt.Printf("%s: panic: %v\n"u8, name, r);
            }
        }, ref ᒐ);
        register();
        fmt.Printf("%s: registered\n"u8, name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void nothing(nint _) { }

internal static void release(this ref T t, nint _) { }

internal static void main() {
    @try("captures ptr", [GoFunc("main.func1")] () => {
        var v = Ꮡ(new T(f: 1));
        var vʗ1 = v;
        runtime.AddCleanup(v, [GoFunc("main.func1.1")] (nint _Δp0) => {
            _ = vʗ1.f;
        }, 0, new object?[] { v });
    });

    @try("captures field pointer", [GoFunc("main.func2")] () => {
        var v = Ꮡ(new T(f: 2));
        var fp = v.Ꮡf;
        var fpʗ1 = fp;
        runtime.AddCleanup(v, [GoFunc("main.func2.1")] (nint _Δp0) => {
            _ = fpʗ1.Value;
        }, 0, new object?[] { fp });
    });

    @try("captures other object", [GoFunc("main.func3")] () => {
        var v = Ꮡ(new T(f: 3));
        var w = Ꮡ(new T(f: 4));
        var wʗ1 = w;
        runtime.AddCleanup(v, [GoFunc("main.func3.1")] (nint _Δp0) => {
            _ = wʗ1.f;
        }, 0, new object?[] { w });
        runtime.KeepAlive(w.OrTypedNil());
    });

    @try("captures value", [GoFunc("main.func4")] () => {
        var v = Ꮡ(new T(f: 5));
        nint n = v.f;
        runtime.AddCleanup(v, [GoFunc("main.func4.1")] (nint _Δp0) => {
            _ = n;
        }, 0);
    });

    @try("captures nothing", [GoFunc("main.func5")] () => {
        var v = Ꮡ(new T(f: 6));
        runtime.AddCleanup<T, nint>(v, nothing, 0);
    });

    @try("sibling captures ptr", [GoFunc("main.func6")] () => {
        var v = Ꮡ(new T(f: 7));
        var w = Ꮡ(new T(f: 8));
        var vʗ2 = v;
        [GoFunc("main.func6.1")] nint sibling() => vʗ2.f;
        var wʗ2 = w;
        runtime.AddCleanup(v, [GoFunc("main.func6.2")] (nint _Δp0) => {
            _ = wʗ2.f;
        }, 0, new object?[] { w });
        fmt.Println("sibling reads", sibling());
        runtime.KeepAlive(w.OrTypedNil());
    });

    @try("method value on ptr", [GoFunc("main.func7")] () => {
        var v = Ꮡ(new T(f: 10));
        runtime.AddCleanup<T, nint>(v, v.release, 0, new object?[] { v });
    });

    @try("reassigned after capture", [GoFunc("main.func8")] () => {
        ref var v = ref heap<ж<T>>(Ꮡ(new T(f: 11)), out var Ꮡv);
        runtime.AddCleanup(Ꮡv.ValueSlot, [GoFunc("main.func8.1")] (nint _Δp0) => {
            _ = Ꮡv.ValueSlot;
        }, 0);
        Ꮡv.ValueSlot = default;
        fmt.Println("reassigned is nil:", Ꮡv.ValueSlot == nil);
    });

    @try("func variable captures ptr", [GoFunc("main.func9")] () => {
        var v = Ꮡ(new T(f: 12));
        var vʗ3 = v;
        var cleanup = [GoFunc("main.func9.1")] (nint _Δp0) => {
            _ = vʗ3.f;
        };
        runtime.AddCleanup(v, cleanup, 0, new object?[] { v });
    });

    @try("func variable captures other object", [GoFunc("main.func10")] () => {
        var v = Ꮡ(new T(f: 13));
        var w = Ꮡ(new T(f: 14));
        var wʗ3 = w;
        var cleanup = [GoFunc("main.func10.1")] (nint _Δp0) => {
            _ = wʗ3.f;
        };
        runtime.AddCleanup(v, cleanup, 0, new object?[] { w });
        runtime.KeepAlive(w.OrTypedNil());
    });

    @try("func variable reassigned to method value", [GoFunc("main.func11")] () => {
        var v = Ꮡ(new T(f: 15));
        var cleanup = [GoFunc("main.func11.1")] (nint _Δp0) => {
        };
        var vʗ4 = v;
                cleanup = (nint p1) => vʗ4.release(p1);
        runtime.AddCleanup(v, cleanup, 0, new object?[] { v });
    });

    @try("struct capture holds ptr", [GoFunc("main.func12")] () => {
        var v = Ꮡ(new T(f: 16));
        ref var p = ref heap<pair>(new pair(v, 1), out var Ꮡp);
        var pʗ1 = p;
        runtime.AddCleanup(v, [GoFunc("main.func12.1")] (nint _Δp0) => {
            _ = pʗ1;
        }, 0, null, new object?[] { p });
    });

    @try("array capture holds ptr", [GoFunc("main.func13")] () => {
        var v = Ꮡ(new T(f: 17));
        ref var a = ref heap<array<ж<T>>>(array<ж<T>>([default, v]), out var Ꮡa);
        var aʗ1 = a;
        runtime.AddCleanup(v, [GoFunc("main.func13.1")] (nint _Δp0) => {
            _ = aʗ1;
        }, 0, null, new object?[] { a });
    });

    @try("nested struct capture holds ptr", [GoFunc("main.func14")] () => {
        var v = Ꮡ(new T(f: 18));
        ref var o = ref heap<outer>(new outer(new pair(v, 2)), out var Ꮡo);
        var oʗ1 = o;
        runtime.AddCleanup(v, [GoFunc("main.func14.1")] (nint _Δp0) => {
            _ = oʗ1;
        }, 0, null, new object?[] { o });
    });

    @try("struct capture holds other object", [GoFunc("main.func15")] () => {
        var v = Ꮡ(new T(f: 19));
        var w = Ꮡ(new T(f: 20));
        ref var p = ref heap<pair>(new pair(w, 3), out var Ꮡp);
        var pʗ2 = p;
        runtime.AddCleanup(v, [GoFunc("main.func15.1")] (nint _Δp0) => {
            _ = pʗ2;
        }, 0, null, new object?[] { p });
        runtime.KeepAlive(w.OrTypedNil());
    });

    @try("struct over 128 bytes holds ptr", [GoFunc("main.func16")] () => {
        var v = Ꮡ(new T(f: 22));
        ref var l = ref heap<large>(new large(p: v), out var Ꮡl);
        var lʗ1 = l;
        runtime.AddCleanup(v, [GoFunc("main.func16.1")] (nint _Δp0) => {
            _ = lʗ1;
        }, 0);
    });

    @try("interface holding a struct that holds ptr", [GoFunc("main.func17")] () => {
        var v = Ꮡ(new T(f: 21));
        any i = new pair(v, 4);
        var iʗ1 = i;
        runtime.AddCleanup(v, [GoFunc("main.func17.1")] (nint _Δp0) => {
            _ = iʗ1;
        }, 0, new object?[] { i });
    });

    @try("arg is ptr", [GoFunc("main.func18")] () => {
        var v = Ꮡ(new T(f: 9));
        runtime.AddCleanup(v, [GoFunc("main.func18.1")] (ж<T> _Δp0) => {
        }, v);
    });
}

} // end main_package

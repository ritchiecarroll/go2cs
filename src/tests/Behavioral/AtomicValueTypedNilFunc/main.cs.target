namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

internal static void recovered(@string label, Action fn) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Printf("%s: PANIC %v\n"u8, label, r);
                    return;
                }
            }
            fmt.Printf("%s: ok\n"u8, label);
        }, ref ᒐ);
        fn();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    ref var v1 = ref heap(new atomic.Value(), out var Ꮡv1);
    Ꮡv1.Store((nint fd) => {
        _ = fd;
    });
    recovered("live then typed-nil, same type", () => {
        Ꮡv1.Store(((Action<nint>)(default!)).OrTypedNilFunc());
    });

    ref var v2 = ref heap(new atomic.Value(), out var Ꮡv2);
    Ꮡv2.Store(((Action<nint>)(default!)).OrTypedNilFunc());
    recovered("typed-nil then live, same type", () => {
        Ꮡv2.Store((nint fd) => {
            _ = fd;
        });
    });

    ref var v3 = ref heap(new atomic.Value(), out var Ꮡv3);
    Ꮡv3.Store(((Action<nint>)(default!)).OrTypedNilFunc());
    recovered("typed-nil then typed-nil, DIFFERENT type", () => {
        Ꮡv3.Store(((Action<@string>)(default!)).OrTypedNilFunc());
    });

    ref var v4 = ref heap(new atomic.Value(), out var Ꮡv4);
    Ꮡv4.Store((nint fd) => {
        _ = fd;
    });
    recovered("live then live, same type", () => {
        Ꮡv4.Store((nint fd) => {
            _ = fd;
        });
    });

    ref var v5 = ref heap(new atomic.Value(), out var Ꮡv5);
    Ꮡv5.Store((nint)1);
    recovered("int then string, different type", () => {
        Ꮡv5.Store("x");
    });

    ref var v6 = ref heap(new atomic.Value(), out var Ꮡv6);
    Ꮡv6.Store(nint (nint fd) => fd * 2);
    {
        var (f, ok) = Ꮡv6.Load()._<Func<nint, nint>>(ᐧ); if (ok) {
            fmt.Println("loaded and called:", f(21));
        }
    }
}

} // end main_package

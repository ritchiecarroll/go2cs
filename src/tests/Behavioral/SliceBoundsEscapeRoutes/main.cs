namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

internal static void arm(@string name, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    fmt.Println(name, "recovered:", r);
                }
            }
        }, ref ᒐ);
        f();
        fmt.Println(name, "no panic");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    uint64 wideU = (uint64)(4294967296L + 5);
    int64 wideI = 4294967301L;
    nint wideInt = unchecked((nint)(4294967301L));

    arm("address uint64:", () => {
        var a = new slice<nint>(10);
        var p = Ꮡ(a, wideU);
        p.Value = 7;
        fmt.Println("a[5] =", a[5]);
    });
    arm("address int64:", () => {
        var a = new slice<nint>(10);
        var p = Ꮡ(a, (nint)(wideI));
        p.Value = 7;
        fmt.Println("a[5] =", a[5]);
    });
    arm("address int:", () => {
        var a = new slice<nint>(10);
        var p = Ꮡ(a, wideInt);
        p.Value = 7;
        fmt.Println("a[5] =", a[5]);
    });
    arm("address past length:", () => {
        var s = new slice<nint>(3, 10);
        nint i = 5;
        var p = Ꮡ(s, i);
        p.Value = 7;
        fmt.Println("s[:6][5] =", s[..6][5]);
    });
    arm("3-index bound:", () => {
        var s = new slice<nint>(10);
        var t = s.slice(0, (nint)(wideU), (nint)(wideU));
        fmt.Println("len", len(t), "cap", cap(t));
    });
    arm("element method:", () => {
        var a = new slice<atomic.Uint64>(10);
        Ꮡ(a, wideU).Add(1);
        fmt.Println("a[5] =", Ꮡ(a, 5).Load());
    });
    nint past = 5;
    arm("literal index:", () => {
        fmt.Println("byte", LiteralByteAt("abc"u8, past));
    });
    nint neg = -1;
    arm("literal negative index:", () => {
        fmt.Println("byte", LiteralByteAt("abc"u8, neg));
    });
    arm("string past end:", () => {
        @string s = "abc";
        fmt.Println(s.slice(1, past));
    });
    nint low = 4;
    arm("string low past end:", () => {
        @string s = "abc";
        fmt.Println(s.slice(low));
    });
}

} // end main_package

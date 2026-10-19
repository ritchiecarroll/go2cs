namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using Δio = io_package;
using strings = strings_package;
using mid = TypeAssertionSource.mid_package;
using TypeAssertionSource;
using lib = TypeAssertionSource.lib_package;

partial class main_package {

partial interface namer {
    @string name();
}

partial struct a {
}

partial struct b {
}

partial struct pipe {
}

internal static (nint, error) Read(this ref pipe _, slice<byte> p) {
    return (0, Δio.EOF);
}

internal static (nint, error) Write(this ref pipe _, slice<byte> p) {
    return (len(p), default!);
}

internal static error Close(this ref pipe _) {
    return default!;
}

internal static @string name(this ref a _) {
    return "a";
}

internal static @string name(this ref b _) {
    return "b";
}

internal static void @catch(@string what, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!){
                    fmt.Printf("%s: %T: %v\n"u8, what, r, r);
                } else {
                    fmt.Printf("%s: NO PANIC\n"u8, what);
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial interface main_anon /*dyn*/ {
    @string name();
}

internal static void Main() {
    namer n = new aжnamer(Ꮡ(new a(nil)));
    var nʗ1 = n;
    @catch("namer-to-*b", () => {
        _ = nʗ1._<namer, ж<b>>();
    });

    Δio.Reader r = new strings_ReaderжReader(strings.NewReader("x"));
    var rʗ1 = r;
    @catch("io.Reader-to-*bytes.Buffer", () => {
        _ = rʗ1._<ж<bytes.Buffer>>();
    });

    Δio.ReadWriter rw = new bytes_BufferжReadWriter(Ꮡ(new bytes.Buffer(nil)));
    var rwʗ1 = rw;
    @catch("io.ReadWriter-to-*pipe", () => {
        _ = rwʗ1._<ж<pipe>>();
    });

    Δio.ReadCloser rc = Δio.NopCloser(new strings_ReaderжReader(strings.NewReader("x")));
    var rcʗ1 = rc;
    @catch("io.ReadCloser-to-*pipe", () => {
        _ = rcʗ1._<ж<pipe>>();
    });

    main_anon anon = new aжmain_anon(Ꮡ(new a(nil)));
    var anonʗ1 = anon;
    @catch("anonymous-to-*b", () => {
        _ = anonʗ1._<ж<b>>();
    });

    var nʗ2 = n;
    @catch("namer-to-*a", () => {
        _ = nʗ2._<namer, ж<a>>();
    });
    var rʗ2 = r;
    @catch("io.Reader-to-*strings.Reader", () => {
        _ = rʗ2._<ж<strings.Reader>>();
    });

    var x = mid.Get();
    var xʗ1 = x;
    @catch("lib.RW-to-*mid.Other", () => {
        _ = xʗ1._<lib.RW, ж<mid.Other>>();
    });
}

} // end main_package

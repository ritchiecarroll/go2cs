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
    return "a"u8;
}

internal static @string name(this ref b _) {
    return "b"u8;
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string namerToBˢ = "namer-to-*b"u8;
private static readonly @string ioReaderToBytesBufferˢ = "io.Reader-to-*bytes.Buffer"u8;
private static readonly @string ioReadWriterToPipeˢ = "io.ReadWriter-to-*pipe"u8;
private static readonly @string ioReadCloserToPipeˢ = "io.ReadCloser-to-*pipe"u8;
private static readonly @string anonymousToBˢ = "anonymous-to-*b"u8;
private static readonly @string namerToAˢ = "namer-to-*a"u8;
private static readonly @string ioReaderToStringsReaderˢ = "io.Reader-to-*strings.Reader"u8;
private static readonly @string libRwToMidOtherˢ = "lib.RW-to-*mid.Other"u8;

internal partial interface main_anon /*dyn*/ {
    @string name();
}

internal static void Main() {
    namer n = new aжnamer(Ꮡ(new a(nil)));
    var nʗ1 = n;
    @catch(namerToBˢ, () => {
        _ = nʗ1._<namer, ж<b>>();
    });

    Δio.Reader r = new strings_ReaderжReader(strings.NewReader("x"u8));
    var rʗ1 = r;
    @catch(ioReaderToBytesBufferˢ, () => {
        _ = rʗ1._<ж<bytes.Buffer>>();
    });

    Δio.ReadWriter rw = new bytes_BufferжReadWriter(Ꮡ(new bytes.Buffer(nil)));
    var rwʗ1 = rw;
    @catch(ioReadWriterToPipeˢ, () => {
        _ = rwʗ1._<ж<pipe>>();
    });

    Δio.ReadCloser rc = Δio.NopCloser(new strings_ReaderжReader(strings.NewReader("x"u8)));
    var rcʗ1 = rc;
    @catch(ioReadCloserToPipeˢ, () => {
        _ = rcʗ1._<ж<pipe>>();
    });

    main_anon anon = new aжmain_anon(Ꮡ(new a(nil)));
    var anonʗ1 = anon;
    @catch(anonymousToBˢ, () => {
        _ = anonʗ1._<ж<b>>();
    });

    var nʗ2 = n;
    @catch(namerToAˢ, () => {
        _ = nʗ2._<namer, ж<a>>();
    });
    var rʗ2 = r;
    @catch(ioReaderToStringsReaderˢ, () => {
        _ = rʗ2._<ж<strings.Reader>>();
    });

    var x = mid.Get();
    var xʗ1 = x;
    @catch(libRwToMidOtherˢ, () => {
        _ = xʗ1._<lib.RW, ж<mid.Other>>();
    });
}

} // end main_package

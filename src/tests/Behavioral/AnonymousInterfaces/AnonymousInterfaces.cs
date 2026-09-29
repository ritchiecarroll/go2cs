namespace go;

using fmt = fmt_package;
using Δio = io_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object typeSwitchUnwrapˢ = (@string)"TypeSwitch: Unwrap ="u8;
private static readonly object typeSwitchNoMatchˢ = (@string)"TypeSwitch: No match"u8;

[GoType("dyn")] internal partial interface testTypeSwitch_type {
    error Unwrap();
}

internal static void testTypeSwitch(error err) {
    switch (err.type()) {
    case {} Δx when Δx._<testTypeSwitch_type>(out var x): {
        fmt.Println(typeSwitchUnwrapˢ, x.Unwrap());
        break;
    }
    default: {
        var x = err;
        fmt.Println(typeSwitchNoMatchˢ);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object typeAssertionIsNilˢ = (@string)"TypeAssertion: Is(nil) ="u8;
private static readonly object typeAssertionNoMatchˢ = (@string)"TypeAssertion: No match"u8;

[GoType("dyn")] internal partial interface testTypeAssertion_type {
    bool Is(error _);
}

internal static void testTypeAssertion(error err) {
    {
        var (x, ok) = err._<testTypeAssertion_type>(ᐧ); if (ok){
            fmt.Println(typeAssertionIsNilˢ, x.Is(default!));
        } else {
            fmt.Println(typeAssertionNoMatchˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object funcParamReadˢ = (@string)"FuncParam: Read ="u8;

[GoType("dyn")] internal partial interface takesReader_r {
    (nint, error) Read(slice<byte> _Δp0);
}

internal static void takesReader(takesReader_r r) {
    var buf = new slice<byte>(4);
    var (n, _) = r.Read(buf);
    fmt.Println(funcParamReadˢ, ((@string)(buf.slice(0, n))));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object compositeLiteralReadˢ = (@string)"CompositeLiteral: Read ="u8;

internal static void testCompositeLiteral() {
    var readers = new takesReader_r[]{new fakeReader(nil)}.slice();
    var buf = new slice<byte>(4);
    var (n, _) = readers[0].Read(buf);
    fmt.Println(compositeLiteralReadˢ, ((@string)(buf.slice(0, n))));
}

[GoType("dyn")] partial interface WithInlineField_R {
    (nint, error) Read(slice<byte> _Δp0);
}

[GoType] partial struct WithInlineField {
    public WithInlineField_R R;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object inlineFieldReadˢ = (@string)"InlineField: Read ="u8;

internal static void testInlineField() {
    var s = new WithInlineField(R: new fakeReader(nil));
    var buf = new slice<byte>(4);
    var (n, _) = s.R.Read(buf);
    fmt.Println(inlineFieldReadˢ, ((@string)(buf.slice(0, n))));
}

[GoType("dyn")] partial interface Δtype {
    error Close();
}

[GoType] partial interface InlineEmbed :
    Δtype
{    error Flush();
}

[GoType] partial struct embeddedImpl {
}

internal static error Close(this embeddedImpl _) {
    return default!;
}

internal static error Flush(this embeddedImpl _) {
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object interfaceEmbedCloseAndˢ = (@string)"InterfaceEmbed: Close and Flush OK"u8;

internal static void testInterfaceEmbedding(InlineEmbed x) {
    _ = x.Close();
    _ = x.Flush();
    fmt.Println(interfaceEmbedCloseAndˢ);
}

[GoType] partial struct fakeReader {
}

internal static (nint, error) Read(this fakeReader _, slice<byte> b) {
    copy(b, "DATA"u8);
    return (4, default!);
}

[GoType] partial struct fakeError {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string fakeErrorˢ = "fake error"u8;

internal static @string Error(this fakeError _) {
    return fakeErrorˢ;
}

internal static error Unwrap(this fakeError _) {
    return Δio.EOF;
}

internal static bool Is(this fakeError _, error err) {
    return AreEqual(err, Δio.EOF);
}

[GoType] partial struct tally {
    internal nint total;
}

[GoRecv] internal static (nint, error) Write(this ref tally t, slice<byte> p) {
    t.total += len(p);
    return (len(p), default!);
}

[GoType("dyn")] internal partial struct fill_dst {
    public io_package.Writer Writer;
}

internal static (int64, error) fill(this ж<tally> Ꮡt, Δio.Reader r) {
    return Δio.Copy(new fill_dst(new tallyжWriter(Ꮡt)), r);
}

[GoType] partial struct byteRepeat {
    internal nint left;
}

[GoRecv] internal static (nint, error) Read(this ref byteRepeat b, slice<byte> p) {
    if (b.left == 0) {
        return (0, Δio.EOF);
    }
    nint n = 0;
    foreach (var (i, _) in p) {
        if (b.left == 0) {
            break;
        }
        p[i] = (rune)'x';
        b.left--;
        n++;
    }
    return (n, default!);
}

[GoType("[4]byte")] partial struct quad;

[GoType] partial struct frame {
    internal quad data;
}

internal static nint checksum(ref frame f) {
    nint s = 0;
    foreach (var (_, v) in f.data[..]) {
        s += (nint)v;
    }
    return s;
}

internal static void Main() {
    testTypeSwitch(new fakeError(nil));
    testTypeAssertion(new fakeError(nil));
    takesReader(new fakeReader(nil));
    testCompositeLiteral();
    testInlineField();
    testInterfaceEmbedding(new embeddedImpl(nil));
    var tl = Ꮡ(new tally(nil));
    var (n, err) = tl.fill(new byteRepeatжReader(Ꮡ(new byteRepeat(left: 10))));
    fmt.Println(n, err == default!, (~tl).total);
    var fr = new frame(data: new quad(new byte[]{1, 2, 3, 4}.array()));
    fmt.Println(checksum(ref fr));
}

} // end main_package

namespace go;

using fmt = fmt_package;
using Δio = io_package;

partial class main_package {

internal partial interface testTypeSwitch_type /*dyn*/ {
    error Unwrap();
}

internal static void testTypeSwitch(error err) {
    switch (err.type()) {
    case {} Δx when Δx._<testTypeSwitch_type>(out var x): {
        fmt.Println("TypeSwitch: Unwrap =", x.Unwrap());
        break;
    }
    default: {
        var x = err;
        fmt.Println("TypeSwitch: No match");
        break;
    }}
}

internal partial interface testTypeAssertion_type /*dyn*/ {
    bool Is(error _);
}

internal static void testTypeAssertion(error err) {
    {
        var (x, ok) = err._<testTypeAssertion_type>(ᐧ); if (ok) {
            fmt.Println("TypeAssertion: Is(nil) =", x.Is(default));
        } else {
            fmt.Println("TypeAssertion: No match");
        }
    }
}

internal partial interface takesReader_r /*dyn*/ {
    (nint, error) Read(slice<byte> _Δp0);
}

internal static void takesReader(takesReader_r r) {
    var buf = new slice<byte>(4);
    var (n, _) = r.Read(buf);
    fmt.Println("FuncParam: Read =", (@string)(buf.slice(0, n)));
}

internal static void testCompositeLiteral() {
    var readers = slice<takesReader_r>([new fakeReader()]);
    var buf = new slice<byte>(4);
    var (n, _) = readers[0].Read(buf);
    fmt.Println("CompositeLiteral: Read =", (@string)(buf.slice(0, n)));
}

partial interface WithInlineField_R /*dyn*/ {
    (nint, error) Read(slice<byte> _Δp0);
}

partial struct WithInlineField {
    public WithInlineField_R R;
}

internal static void testInlineField() {
    var s = new WithInlineField(R: new fakeReader());
    var buf = new slice<byte>(4);
    var (n, _) = s.R.Read(buf);
    fmt.Println("InlineField: Read =", (@string)(buf.slice(0, n)));
}

partial interface Δtype /*dyn*/ {
    error Close();
}

partial interface InlineEmbed :
    Δtype
{    error Flush();
}

partial struct embeddedImpl {
}

internal static error Close(this embeddedImpl _) {
    return default;
}

internal static error Flush(this embeddedImpl _) {
    return default;
}

internal static void testInterfaceEmbedding(InlineEmbed x) {
    _ = x.Close();
    _ = x.Flush();
    fmt.Println("InterfaceEmbed: Close and Flush OK");
}

partial struct fakeReader {
}

internal static (nint, error) Read(this fakeReader _, slice<byte> b) {
    copy(b, "DATA"u8);
    return (4, default);
}

partial struct fakeError {
}

internal static @string Error(this fakeError _) {
    return "fake error";
}

internal static error Unwrap(this fakeError _) {
    return Δio.EOF;
}

internal static bool Is(this fakeError _, error err) {
    return AreEqual(err, Δio.EOF);
}

partial struct tally {
    internal nint total;
}

internal static (nint, error) Write(this ref tally t, slice<byte> p) {
    t.total += len(p);
    return (len(p), default);
}

internal partial struct fill_dst /*dyn*/ {
    /*embed*/ public io_package.Writer Writer;
}

internal static (int64, error) fill(this ж<tally> Ꮡt, Δio.Reader r) {
    return Δio.Copy(new fill_dst(new tallyжWriter(Ꮡt)), r);
}

partial struct byteRepeat {
    internal nint left;
}

internal static (nint, error) Read(this ref byteRepeat b, slice<byte> p) {
    if (b.left == 0) {
        return (0, Δio.EOF);
    }
    nint n = 0;
    foreach (var i in range(p)) {
        if (b.left == 0) {
            break;
        }
        p[i] = (byte)'x';
        b.left--;
        n++;
    }
    return (n, default);
}

partial struct quad /*[4]byte*/;

partial struct frame {
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
    testTypeSwitch(new fakeError());
    testTypeAssertion(new fakeError());
    takesReader(new fakeReader());
    testCompositeLiteral();
    testInlineField();
    testInterfaceEmbedding(new embeddedImpl());

    var tl = Ꮡ(new tally());
    var (n, err) = tl.fill(new byteRepeatжReader(Ꮡ(new byteRepeat(left: 10))));
    fmt.Println(n, err == nil, (~tl).total);

    var fr = new frame(data: new quad(new byte[]{1, 2, 3, 4}.array()));
    fmt.Println(checksum(ref fr));
}

} // end main_package

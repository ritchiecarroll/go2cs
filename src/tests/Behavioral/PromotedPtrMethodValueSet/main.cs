namespace go;

using bufio = bufio_package;
using fmt = fmt_package;
using Δio = io_package;
using reflect = reflect_package;
using strings = strings_package;

partial class main_package {

partial struct Inner {
    internal nint n;
}

public static nint Bump(this ref Inner i) {
    i.n++;
    return i.n;
}

public static nint Get(this Inner i) {
    return i.n;
}

partial struct SameP {
    public partial ref ж<Inner> Inner { get; }
}

partial struct SameV {
    public partial ref Inner Inner { get; }
}

partial struct MidP {
    public partial ref ж<Inner> Inner { get; }
}

partial struct OuterVP {
    public partial ref MidP MidP { get; }
}

partial struct MidV {
    public partial ref Inner Inner { get; }
}

partial struct OuterPV {
    public partial ref ж<MidV> MidV { get; }
}

partial struct OuterVV {
    public partial ref MidV MidV { get; }
}

partial interface bumper {
    nint Bump();
}

internal static @string names(reflectꓸType t) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    for (nint i = 0; i < t.NumMethod(); i++) {
        if (i > 0) {
            Ꮡsb.WriteString(","u8);
        }
        Ꮡsb.WriteString(t.Method(i).Name);
    }
    return sb.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string bumpˢ = "Bump"u8;

internal static void describe(@string label, any v) {
    var t = reflect.TypeOf(v);
    fmt.Printf("%s: NumMethod=%d [%s]\n"u8, label, t.NumMethod(), names(t));
    var rv = reflect.ValueOf(v);
    var m = rv.MethodByName(bumpˢ);
    fmt.Printf("%s: MethodByName(Bump) valid=%v\n"u8, label, m.IsValid());
    if (m.IsValid()) {
        var @out = m.Call(default!);
        fmt.Printf("%s: Bump via MethodByName = %d\n"u8, label, @out[0].Int());
    }
    for (nint i = 0; i < t.NumMethod(); i++) {
        if (t.Method(i).Name == "Bump"u8) {
            var @out = rv.Method(i).Call(default!);
            fmt.Printf("%s: Bump via Method(%d).Call = %d\n"u8, label, i, @out[0].Int());
        }
    }
    var (b, ok) = v._<bumper>(ᐧ);
    fmt.Printf("%s: assert bumper ok=%v\n"u8, label, ok);
    if (ok) {
        fmt.Printf("%s: Bump via interface = %d\n"u8, label, b.Bump());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string samePˢ = "SameP"u8;
private static readonly @string samePˢ2 = "*SameP"u8;
private static readonly object samePInnerAfterˢ = (@string)"SameP inner after:"u8;
private static readonly @string sameVˢ = "SameV"u8;
private static readonly @string sameVˢ2 = "*SameV"u8;
private static readonly object sameVInnerAfterˢ = (@string)"SameV inner after:"u8;
private static readonly @string outerVPˢ = "OuterVP"u8;
private static readonly @string outerVPˢ2 = "*OuterVP"u8;
private static readonly object outerVPInnerAfterˢ = (@string)"OuterVP inner after:"u8;
private static readonly @string outerPVˢ = "OuterPV"u8;
private static readonly @string outerPVˢ2 = "*OuterPV"u8;
private static readonly object outerPVInnerAfterˢ = (@string)"OuterPV inner after:"u8;
private static readonly @string outerVVˢ = "OuterVV"u8;
private static readonly @string outerVVˢ2 = "*OuterVV"u8;
private static readonly object outerVVInnerAfterˢ = (@string)"OuterVV inner after:"u8;
private static readonly @string firstSecondˢ = "first\nsecond\n"u8;
private static readonly @string readStringˢ = "ReadString"u8;
private static readonly object bufioReadWriterValueHasˢ = (@string)"bufio.ReadWriter value has ReadString:"u8;
private static readonly object bufioReadWriterValueˢ = (@string)"bufio.ReadWriter value asserts io.Reader:"u8;

internal static void Main() {
    ref var p = ref heap<SameP>(out var Ꮡp);
    p = new SameP(Ꮡ(new Inner(10)));
    describe(samePˢ, p);
    describe(samePˢ2, Ꮡp);
    fmt.Println(samePInnerAfterˢ, p.n);
    ref var v = ref heap<SameV>(out var Ꮡv);
    v = new SameV(new Inner(20));
    describe(sameVˢ, v);
    describe(sameVˢ2, Ꮡv);
    fmt.Println(sameVInnerAfterˢ, v.n);
    ref var vp = ref heap<OuterVP>(out var Ꮡvp);
    vp = new OuterVP(new MidP(Ꮡ(new Inner(30))));
    describe(outerVPˢ, vp);
    describe(outerVPˢ2, Ꮡvp);
    fmt.Println(outerVPInnerAfterˢ, vp.n);
    ref var pv = ref heap<OuterPV>(out var Ꮡpv);
    pv = new OuterPV(Ꮡ(new MidV(new Inner(40))));
    describe(outerPVˢ, pv);
    describe(outerPVˢ2, Ꮡpv);
    fmt.Println(outerPVInnerAfterˢ, pv.n);
    ref var vv = ref heap<OuterVV>(out var Ꮡvv);
    vv = new OuterVV(new MidV(new Inner(50)));
    describe(outerVVˢ, vv);
    describe(outerVVˢ2, Ꮡvv);
    fmt.Println(outerVVInnerAfterˢ, vv.n);
    var rw = new bufio.ReadWriter(Reader: bufio.NewReader(new strings_ReaderжReader(strings.NewReader(firstSecondˢ))));
    var rt = reflect.TypeOf(rw);
    var (_, hasReadString) = rt.MethodByName(readStringˢ);
    fmt.Println(bufioReadWriterValueHasˢ, hasReadString);
    var @out = reflect.ValueOf(rw).MethodByName(readStringˢ).Call(new reflectꓸValue[]{reflect.ValueOf((byte)(rune)'\n')}.slice());
    fmt.Printf("ReadString via reflect = %q\n"u8, @out[0].String());
    any x = rw;
    var (r, ok) = x._<Δio.Reader>(ᐧ);
    fmt.Println(bufioReadWriterValueˢ, ok);
    if (ok) {
        var buf = new slice<byte>(6);
        var (n, _) = r.Read(buf);
        fmt.Printf("Read via io.Reader = %q\n"u8, ((@string)(buf.slice(0, n))));
    }
}

} // end main_package

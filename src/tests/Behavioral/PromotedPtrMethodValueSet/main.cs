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
    /*embed*/ public ж<Inner> Inner;
}

partial struct SameV {
    /*embed*/ public Inner Inner;
}

partial struct MidP {
    /*embed*/ public ж<Inner> Inner;
}

partial struct OuterVP {
    /*embed*/ public MidP MidP;
}

partial struct MidV {
    /*embed*/ public Inner Inner;
}

partial struct OuterPV {
    /*embed*/ public ж<MidV> MidV;
}

partial struct OuterVV {
    /*embed*/ public MidV MidV;
}

partial interface bumper {
    nint Bump();
}

internal static @string names(reflectꓸType t) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);

    for (nint i = 0; i < t.NumMethod(); i++) {
        if (i > 0) {
            Ꮡsb.WriteString(",");
        }

        Ꮡsb.WriteString(t.Method(i).Name);
    }

    return sb.String();
}

internal static void describe(@string label, any v) {
    var t = reflect.TypeOf(v);
    fmt.Printf("%s: NumMethod=%d [%s]\n"u8, label, t.NumMethod(), names(t));

    var rv = reflect.ValueOf(v);
    var m = rv.MethodByName("Bump");
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
private static readonly object samePInnerAfterˢ = (@string)"SameP inner after:"u8;
private static readonly object sameVInnerAfterˢ = (@string)"SameV inner after:"u8;
private static readonly object outerVPInnerAfterˢ = (@string)"OuterVP inner after:"u8;
private static readonly object outerPVInnerAfterˢ = (@string)"OuterPV inner after:"u8;
private static readonly object outerVVInnerAfterˢ = (@string)"OuterVV inner after:"u8;
private static readonly object bufioReadWriterValueHasˢ = (@string)"bufio.ReadWriter value has ReadString:"u8;
private static readonly object bufioReadWriterValueˢ = (@string)"bufio.ReadWriter value asserts io.Reader:"u8;

internal static void Main() {
    ref var p = ref heap<SameP>(out var Ꮡp);
    p = new SameP(Ꮡ(new Inner(10)));
    describe("SameP", p);
    describe("*SameP", Ꮡp);
    fmt.Println(samePInnerAfterˢ, p.n);

    ref var v = ref heap<SameV>(out var Ꮡv);
    v = new SameV(new Inner(20));
    describe("SameV", v);
    describe("*SameV", Ꮡv);
    fmt.Println(sameVInnerAfterˢ, v.n);

    ref var vp = ref heap<OuterVP>(out var Ꮡvp);
    vp = new OuterVP(new MidP(Ꮡ(new Inner(30))));
    describe("OuterVP", vp);
    describe("*OuterVP", Ꮡvp);
    fmt.Println(outerVPInnerAfterˢ, vp.n);

    ref var pv = ref heap<OuterPV>(out var Ꮡpv);
    pv = new OuterPV(Ꮡ(new MidV(new Inner(40))));
    describe("OuterPV", pv);
    describe("*OuterPV", Ꮡpv);
    fmt.Println(outerPVInnerAfterˢ, pv.n);

    ref var vv = ref heap<OuterVV>(out var Ꮡvv);
    vv = new OuterVV(new MidV(new Inner(50)));
    describe("OuterVV", vv);
    describe("*OuterVV", Ꮡvv);
    fmt.Println(outerVVInnerAfterˢ, vv.n);

    var rw = new bufio.ReadWriter(Reader: bufio.NewReader(new strings_ReaderжReader(strings.NewReader("first\nsecond\n"))));
    var rt = reflect.TypeOf(rw);
    var (_, hasReadString) = rt.MethodByName("ReadString");
    fmt.Println(bufioReadWriterValueHasˢ, hasReadString);

    var @out = reflect.ValueOf(rw).MethodByName("ReadString").Call(new reflectꓸValue[]{reflect.ValueOf((byte)(rune)'\n')}.slice());
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

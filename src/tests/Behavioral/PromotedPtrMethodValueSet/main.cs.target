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
        var @out = m.Call(default);
        fmt.Printf("%s: Bump via MethodByName = %d\n"u8, label, @out[0].Int());
    }

    for (nint i = 0; i < t.NumMethod(); i++) {
        if (t.Method(i).Name == "Bump"u8) {
            var @out = rv.Method(i).Call(default);
            fmt.Printf("%s: Bump via Method(%d).Call = %d\n"u8, label, i, @out[0].Int());
        }
    }

    var (b, ok) = v._<bumper>(ᐧ);
    fmt.Printf("%s: assert bumper ok=%v\n"u8, label, ok);

    if (ok) {
        fmt.Printf("%s: Bump via interface = %d\n"u8, label, b.Bump());
    }
}

internal static void Main() {
    ref var p = ref heap<SameP>(new SameP(Ꮡ(new Inner(10))), out var Ꮡp);
    describe("SameP", p);
    describe("*SameP", Ꮡp);
    fmt.Println("SameP inner after:", p.n);

    ref var v = ref heap<SameV>(new SameV(new Inner(20)), out var Ꮡv);
    describe("SameV", v);
    describe("*SameV", Ꮡv);
    fmt.Println("SameV inner after:", v.n);

    ref var vp = ref heap<OuterVP>(new OuterVP(new MidP(Ꮡ(new Inner(30)))), out var Ꮡvp);
    describe("OuterVP", vp);
    describe("*OuterVP", Ꮡvp);
    fmt.Println("OuterVP inner after:", vp.n);

    ref var pv = ref heap<OuterPV>(new OuterPV(Ꮡ(new MidV(new Inner(40)))), out var Ꮡpv);
    describe("OuterPV", pv);
    describe("*OuterPV", Ꮡpv);
    fmt.Println("OuterPV inner after:", pv.n);

    ref var vv = ref heap<OuterVV>(new OuterVV(new MidV(new Inner(50))), out var Ꮡvv);
    describe("OuterVV", vv);
    describe("*OuterVV", Ꮡvv);
    fmt.Println("OuterVV inner after:", vv.n);

    var rw = new bufio.ReadWriter(Reader: bufio.NewReader(new strings_ReaderжReader(strings.NewReader("first\nsecond\n"))));
    var rt = reflect.TypeOf(rw);
    var (_, hasReadString) = rt.MethodByName("ReadString");
    fmt.Println("bufio.ReadWriter value has ReadString:", hasReadString);

    var @out = reflect.ValueOf(rw).MethodByName("ReadString").Call(new reflectꓸValue[]{reflect.ValueOf((byte)'\n')}.slice());
    fmt.Printf("ReadString via reflect = %q\n"u8, @out[0].String());

    any x = rw;
    var (r, ok) = x._<Δio.Reader>(ᐧ);
    fmt.Println("bufio.ReadWriter value asserts io.Reader:", ok);

    if (ok) {
        var buf = new slice<byte>(6);
        var (n, _) = r.Read(buf);
        fmt.Printf("Read via io.Reader = %q\n"u8, (@string)(buf.slice(0, n)));
    }
}

} // end main_package

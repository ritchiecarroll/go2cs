namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using strings = strings_package;

partial class main_package {

partial struct myType {
    /*embed*/ public bytes_package.Buffer Buffer;
}

partial struct inner {
    internal nint n;
}

internal static nint Add(this ref inner i, nint d) {
    i.n += d;
    return i.n;
}

internal static nint Get(this inner i) {
    return i.n;
}

partial struct outer {
    /*embed*/ internal inner inner;
}

partial struct own {
    internal @string s;
}

internal static @string Say(this own o, @string p) {
    return p + o.s;
}

internal static void Main() {
    var write = ((Func<ж<myType>, slice<byte>, (nint, error)>)([GoWrapper("(*myType).Write")] (p0, p1) => wrapperRecv(p0).of(myType.ᏑBuffer).Write(p1)));
    ref var m = ref heap(new myType(), out var Ꮡm);
    var (n, err) = write(Ꮡm, slice<byte>("expr"u8));
    fmt.Println("(*myType).Write:", n, err, Ꮡm.of(myType.ᏑBuffer).String());

    var m2 = Ꮡ(new myType());
    
    var m2ʗ1 = m2;
    var wv = (slice<byte> p1) => m2ʗ1.Write(p1);
    (n, err) = wv(slice<byte>("value"u8));
    fmt.Println("(&myType{}).Write:", n, err, m2.of(myType.ᏑBuffer).String());

    ref var m3 = ref heap(new myType(), out var Ꮡm3);
    Ꮡm3.of(myType.ᏑBuffer).WriteString("abc");
    var lv = Ꮡm3.Len;
    fmt.Println("m3.Len:", lv());
    
    var add = ((Func<ж<outer>, nint, nint>)(Add));
    ref var o = ref heap(new outer(), out var Ꮡo);
    fmt.Println("(*outer).Add:", add(Ꮡo, 2), add(Ꮡo, 3));
    
    var get = ((Func<outer, nint>)(Get));
    fmt.Println("outer.Get:", get(o));
    
    var oʗ1 = o;
    var gv = () => oʗ1.Get();
    fmt.Println("o.Get:", gv());
    
    var say = ((Func<own, @string, @string>)(Say));
    fmt.Println("own.Say:", say(new own("!"), "hi"));
    var recvʗ1 = new own("?");
    
    var sv = (@string p1) => recvʗ1.Say(p1);
    fmt.Println("own{}.Say:", sv("why"));

    fmt.Println("as value:", strings.ToUpper(fmt.Sprint(((Func<Func<ж<myType>, slice<byte>, (nint, error)>, nint>)(f => {
        ref var t = ref heap(new myType(), out var Ꮡt);
        var (k, _) = f(Ꮡt, slice<byte>("xyz"u8));
        return k + Ꮡt.of(myType.ᏑBuffer).Len();
    }))(((Func<ж<myType>, slice<byte>, (nint, error)>)([GoWrapper("(*myType).Write")] (p0, p1) => wrapperRecv(p0).of(myType.ᏑBuffer).Write(p1)))))));
}

} // end main_package

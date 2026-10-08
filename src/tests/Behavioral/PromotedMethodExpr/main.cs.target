namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using strings = strings_package;

partial class main_package {

partial struct myType {
    public partial ref bytes_package.Buffer Buffer { get; }
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
    internal partial ref inner inner { get; }
}

partial struct own {
    internal @string s;
}

internal static @string Say(this own o, @string p) {
    return p + o.s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object myTypeWriteˢ = (@string)"(*myType).Write:"u8;
private static readonly object myTypeWriteˢ2 = (@string)"(&myType{}).Write:"u8;
private static readonly @string abcˢ = "abc"u8;
private static readonly object m3Lenˢ = (@string)"m3.Len:"u8;
private static readonly object outerAddˢ = (@string)"(*outer).Add:"u8;
private static readonly object outerGetˢ = (@string)"outer.Get:"u8;
private static readonly object oGetˢ = (@string)"o.Get:"u8;
private static readonly object ownSayˢ = (@string)"own.Say:"u8;
private static readonly object ownSayˢ2 = (@string)"own{}.Say:"u8;
private static readonly @string whyˢ = "why"u8;
private static readonly object asValueˢ = (@string)"as value:"u8;

internal static void Main() {
    var write = ((Func<ж<myType>, slice<byte>, (nint, error)>)([GoWrapper("(*myType).Write")] (p0, p1) => wrapperRecv(p0).of(myType.ᏑBuffer).Write(p1)));
    ref var m = ref heap(new myType(), out var Ꮡm);
    var (n, err) = write(Ꮡm, slice<byte>("expr"u8));
    fmt.Println(myTypeWriteˢ, n, err, Ꮡm.of(myType.ᏑBuffer).String());
    var m2 = Ꮡ(new myType(nil));
    
    var m2ʗ1 = m2;
    var wv = (slice<byte> p1) => m2ʗ1.Write(p1);
    (n, err) = wv(slice<byte>("value"u8));
    fmt.Println(myTypeWriteˢ2, n, err, m2.of(myType.ᏑBuffer).String());
    ref var m3 = ref heap(new myType(), out var Ꮡm3);
    Ꮡm3.of(myType.ᏑBuffer).WriteString(abcˢ);
    var lv = Ꮡm3.Len;
    fmt.Println(m3Lenˢ, lv());
    
    var add = ((Func<ж<outer>, nint, nint>)(Add));
    ref var o = ref heap(new outer(), out var Ꮡo);
    fmt.Println(outerAddˢ, add(Ꮡo, 2), add(Ꮡo, 3));
    
    var get = ((Func<outer, nint>)(Get));
    fmt.Println(outerGetˢ, get(o));
    
    var oʗ1 = o;
    var gv = () => oʗ1.Get();
    fmt.Println(oGetˢ, gv());
    
    var say = ((Func<own, @string, @string>)(Say));
    fmt.Println(ownSayˢ, say(new own("!"u8), "hi"u8));
    var recvʗ1 = new own("?"u8);
    
    var sv = (@string p1) => recvʗ1.Say(p1);
    fmt.Println(ownSayˢ2, sv(whyˢ));
    fmt.Println(asValueˢ, strings.ToUpper(fmt.Sprint(((Func<Func<ж<myType>, slice<byte>, (nint, error)>, nint>)(f => {
        ref var t = ref heap(new myType(), out var Ꮡt);
        var (k, _) = f(Ꮡt, slice<byte>("xyz"u8));
        return k + Ꮡt.of(myType.ᏑBuffer).Len();
    }))(((Func<ж<myType>, slice<byte>, (nint, error)>)([GoWrapper("(*myType).Write")] (p0, p1) => wrapperRecv(p0).of(myType.ᏑBuffer).Write(p1)))))));
}

} // end main_package

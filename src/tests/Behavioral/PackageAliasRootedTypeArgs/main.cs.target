global using names = go.slice<go.@string>;
global using grid = go.slice<go.slice<go.@string>>;
global using errs = go.slice<go.error>;
global using cplx = go.slice<go.complex64>;
global using chans = go.channel<go.@string>;
global using arr = go.array<go.@string>;
global using sizes = go.map<go.@string, long>;
global using u8s = go.slice<byte>;
global using ifs = go.slice<object>;
global using cplx2 = go.slice<System.Numerics.Complex>;
global using hdrs = go.slice<go.main_package.Header>;
global using hmap = go.map<go.@string, go.main_package.Header>;
global using nested = go.map<go.@string, go.slice<go.main_package.Header>>;
global using ptrs = go.slice<go.ж<go.main_package.Header>>;
global using iface = go.slice<go.main_package.Stringish>;
global using sends = go.channel/*<-*/<go.main_package.Header>;
global using recvs = go./*<-*/channel<go.main_package.Header>;
global using rdrs = go.slice<go.io_package.Reader>;
global using fn = System.Func<go.@string, nint>;
global using fn2 = System.Func<go.main_package.Header, (go.@string, go.error)>;
global using fn0 = System.Action;
global using anon = go.main_package.anonᴛ1;
global using anonI = go.main_package.anonIᴛ1;
global using direct = go.main_package.Header;
global using aliasOfAlias = go.slice<go.main_package.Header>;

namespace go;

using fmt = fmt_package;
using Δio = io_package;

partial class main_package {

partial struct Header {
    public @string Name;
    public int64 Size;
}

public static @string String(this Header h) {
    return fmt.Sprint(h.Name, (@string)"/"u8, h.Size);
}

partial interface Stringish {
    @string String();
}

partial struct anonᴛ1 /*dyn*/ {
    public nint A;
}

partial interface anonIᴛ1 /*dyn*/ {
    nint Zed();
}

partial struct zed {
    internal nint v;
}

internal static nint Zed(this zed z) {
    return z.v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fn0Ranˢ = (@string)"fn0 ran"u8;
private static readonly object namesˢ = (@string)"names:"u8;
private static readonly object sizesˢ = (@string)"sizes:"u8;
private static readonly object gridˢ = (@string)"grid:"u8;
private static readonly object errsˢ = (@string)"errs:"u8;
private static readonly object cplxˢ = (@string)"cplx:"u8;
private static readonly object cplx2ˢ = (@string)"cplx2:"u8;
private static readonly object chansˢ = (@string)"chans:"u8;
private static readonly object arrˢ = (@string)"arr:"u8;
private static readonly object hdrsˢ = (@string)"hdrs:"u8;
private static readonly object rdrsˢ = (@string)"rdrs:"u8;
private static readonly object ptrsˢ = (@string)"ptrs:"u8;
private static readonly @string abcdˢ = "abcd"u8;
private static readonly object fn2ˢ = (@string)"fn2:"u8;
private static readonly object nestedˢ = (@string)"nested:"u8;
private static readonly object hmapˢ = (@string)"hmap:"u8;
private static readonly object u8sˢ = (@string)"u8s:"u8;
private static readonly object ifsˢ = (@string)"ifs:"u8;
private static readonly object directˢ = (@string)"direct:"u8;
private static readonly object aliasOfAliasˢ = (@string)"aliasOfAlias:"u8;
private static readonly object anonˢ = (@string)"anon:"u8;
private static readonly object ifaceˢ = (@string)"iface:"u8;
private static readonly object anonIˢ = (@string)"anonI:"u8;
private static readonly object sendsˢ = (@string)"sends:"u8;
private static readonly object recvsˢ = (@string)"recvs:"u8;

internal static void Main() {
    names a = new @string[]{"x"u8, "y"u8}.slice();
    sizes b = new sizes{["k"u8] = 7};
    grid c = new slice<@string>[]{new @string[]{"g"u8}.slice()}.slice();
    errs d = new error[]{default!}.slice();
    cplx e = new complex64[]{complex(1F, 2F)}.slice();
    cplx2 e2 = new complex128[]{complex(3D, 4D)}.slice();
    chans f = new chans(1);
    arr g = new @string[]{"a"u8, "b"u8, "c"u8}.array();
    hdrs h = new Header[]{new(Name: "n"u8, Size: 1)}.slice();
    rdrs i = new Δio.Reader[]{default!}.slice();
    ptrs j = new ж<Header>[]{default!}.slice();
    fn k = (@string sΔ1) => len(sΔ1);
    fn2 k2 = (Header x) => (x.Name, default!);
    fn0 k0 = () => {
        fmt.Println(fn0Ranˢ);
    };
    nested l = new nested{["z"u8] = new Header[]{new(Name: "q"u8, Size: 2)}.slice()};
    hmap m = new hmap{["z"u8] = new(Name: "hm"u8, Size: 3)};
    u8s n = new uint8[]{7, 8}.slice();
    ifs o = new any[]{(nint)(1), (@string)"two"u8}.slice();
    direct p = new direct(Name: "dir"u8, Size: 4);
    aliasOfAlias q = new Header[]{new(Name: "aoa"u8, Size: 5)}.slice();
    anon r = new anon(A: 9);
    iface s = new Stringish[]{new Header(Name: "if"u8, Size: 6)}.slice();
    anonI t = new zed(v: 11);
    var sendCh = new channel<Header>(1);
    sends send = sendCh.WithDirection(GoChanDir.Send);
    var recvCh = new channel<Header>(1);
    recvCh.ᐸꟷ(new Header(Name: "recv"u8, Size: 12));
    recvs recv = recvCh.WithDirection(GoChanDir.Recv);
    f.ᐸꟷ("ch"u8);
    send.ᐸꟷ(new Header(Name: "send"u8, Size: 13));
    k0();
    var (name, err) = k2(new Header(Name: "k2"u8, Size: 14));
    fmt.Println(namesˢ, a, sizesˢ, b, gridˢ, c, errsˢ, d);
    fmt.Println(cplxˢ, e, cplx2ˢ, e2, chansˢ, ᐸꟷ(f), arrˢ, g);
    fmt.Println(hdrsˢ, h, rdrsˢ, i, ptrsˢ, j, (@string)"fn:"u8, k(abcdˢ));
    fmt.Println(fn2ˢ, name, err, nestedˢ, l, hmapˢ, m);
    fmt.Println(u8sˢ, n, ifsˢ, o, directˢ, p, aliasOfAliasˢ, q);
    fmt.Println(anonˢ, r, ifaceˢ, s, anonIˢ, t.Zed());
    fmt.Println(sendsˢ, ᐸꟷ(sendCh), recvsˢ, ᐸꟷ(recv));
}

} // end main_package

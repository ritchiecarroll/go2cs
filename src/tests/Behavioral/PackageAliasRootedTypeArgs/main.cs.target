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
    return fmt.Sprint(h.Name, "/", h.Size);
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

internal static void Main() {
    names a = slice<@string>(["x", "y"]);
    sizes b = new sizes{["k"] = 7};
    grid c = slice<slice<@string>>([slice<@string>(["g"])]);
    errs d = slice<error>([default]);
    cplx e = slice<complex64>([complex(1F, 2F)]);
    cplx2 e2 = slice<complex128>([complex(3D, 4D)]);
    chans f = new chans(1);
    arr g = new @string[]{"a", "b", "c"}.array();
    hdrs h = slice<Header>([new(Name: "n", Size: 1)]);
    rdrs i = slice<Δio.Reader>([default]);
    ptrs j = slice<ж<Header>>([default]);
    fn k = (@string sΔ1) => len(sΔ1);
    fn2 k2 = (Header x) => (x.Name, default);
    fn0 k0 = () => {
        fmt.Println("fn0 ran");
    };
    nested l = new nested{["z"] = slice<Header>([new(Name: "q", Size: 2)])};
    hmap m = new hmap{["z"] = new(Name: "hm", Size: 3)};
    u8s n = slice<uint8>([7, 8]);
    ifs o = slice<any>([(nint)1, (@string)"two"u8]);
    direct p = new direct(Name: "dir", Size: 4);
    aliasOfAlias q = slice<Header>([new(Name: "aoa", Size: 5)]);
    anon r = new anon(A: 9);
    iface s = slice<Stringish>([new Header(Name: "if", Size: 6)]);
    anonI t = new zed(v: 11);

    var sendCh = new channel<Header>(1);
    sends send = sendCh.AsSendOnly();
    var recvCh = new channel<Header>(1);
    recvCh.ᐸꟷ(new Header(Name: "recv", Size: 12));
    recvs recv = recvCh.AsRecvOnly();

    f.ᐸꟷ("ch");
    send.ᐸꟷ(new Header(Name: "send", Size: 13));

    k0();
    var (name, err) = k2(new Header(Name: "k2", Size: 14));

    fmt.Println("names:", a, "sizes:", b, "grid:", c, "errs:", d);
    fmt.Println("cplx:", e, "cplx2:", e2, "chans:", ᐸꟷ(f), "arr:", g);
    fmt.Println("hdrs:", h, "rdrs:", i, "ptrs:", j, "fn:", k("abcd"));
    fmt.Println("fn2:", name, err, "nested:", l, "hmap:", m);
    fmt.Println("u8s:", n, "ifs:", o, "direct:", p, "aliasOfAlias:", q);
    fmt.Println("anon:", r, "iface:", s, "anonI:", t.Zed());
    fmt.Println("sends:", ᐸꟷ(sendCh), "recvs:", ᐸꟷ(recv));
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface reader {
    @string Read();
}

partial interface readCloser {
    @string Read();
    @string Close();
}

partial struct conn {
    internal @string data;
}

internal static @string Read(this ref conn c) {
    return "read:"u8 + c.data;
}

internal static @string Close(this ref conn c) {
    return "closed:"u8 + c.data;
}

partial struct valConn {
    internal @string tag;
}

internal static @string Read(this valConn v) {
    return "vread:"u8 + v.tag;
}

internal static @string Close(this valConn v) {
    return "vclosed:"u8 + v.tag;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string mutatedˢ = "mutated"u8;
private static readonly object chainAssertOkˢ = (@string)"chain assert ok:"u8;
private static readonly object chainAssertMissedˢ = (@string)"chain assert MISSED"u8;
private static readonly object throughChainˢ = (@string)"through chain:"u8;
private static readonly object throughSourceˢ = (@string)"through source:"u8;
private static readonly object switchConnˢ = (@string)"switch *conn:"u8;
private static readonly object switchDefaultˢ = (@string)"switch default"u8;
private static readonly object valueChainWronglyˢ = (@string)"value chain WRONGLY asserted *conn"u8;
private static readonly object valueChainMissesConnˢ = (@string)"value chain misses *conn:"u8;
private static readonly object valueChainAssertsValConnˢ = (@string)"value chain asserts valConn:"u8;
private static readonly object valueChainMissedValConnˢ = (@string)"value chain MISSED valConn"u8;
private static readonly object valueSwitchWronglyˢ = (@string)"value switch WRONGLY matched *conn:"u8;
private static readonly object valueSwitchValConnˢ = (@string)"value switch valConn:"u8;
private static readonly object valueSwitchDefaultˢ = (@string)"value switch default"u8;
private static readonly object nilInterfaceWronglyˢ = (@string)"nil interface WRONGLY asserted"u8;
private static readonly object nilInterfaceMissesˢ = (@string)"nil interface misses"u8;

internal static void Main() {
    readCloser rc = new connжreadCloser(Ꮡ(new conn(data: "init"u8)));
    reader r = new readCloserᴠreader(rc);
    {
        var (p, ok) = r._<ж<conn>>(ᐧ); if (ok){
            p.Value.data = mutatedˢ;
            fmt.Println(chainAssertOkˢ, p.Read());
        } else {
            fmt.Println(chainAssertMissedˢ);
        }
    }
    fmt.Println(throughChainˢ, r.Read());
    fmt.Println(throughSourceˢ, rc.Close());
    switch (r.type()) {
    case ж<conn> v: {
        fmt.Println(switchConnˢ, (~v).data);
        break;
    }
    default: {
        var v = r;
        fmt.Println(switchDefaultˢ);
        break;
    }}
    readCloser vrc = new valConn(tag: "v1"u8);
    reader vr = new readCloserᴠreader(vrc);
    {
        var (_, ok) = vr._<ж<conn>>(ᐧ); if (ok){
            fmt.Println(valueChainWronglyˢ);
        } else {
            fmt.Println(valueChainMissesConnˢ, vr.Read());
        }
    }
    {
        var (v, ok) = vr._<valConn>(ᐧ); if (ok){
            fmt.Println(valueChainAssertsValConnˢ, v.tag);
        } else {
            fmt.Println(valueChainMissedValConnˢ);
        }
    }
    switch (vr.type()) {
    case ж<conn> v: {
        fmt.Println(valueSwitchWronglyˢ, (~v).data);
        break;
    }
    case valConn v: {
        fmt.Println(valueSwitchValConnˢ, v.tag);
        break;
    }
    default: {
        var v = vr;
        fmt.Println(valueSwitchDefaultˢ);
        break;
    }}
    reader empty = default!;
    {
        var (_, ok) = empty._<ж<conn>>(ᐧ); if (ok){
            fmt.Println(nilInterfaceWronglyˢ);
        } else {
            fmt.Println(nilInterfaceMissesˢ);
        }
    }
}

} // end main_package

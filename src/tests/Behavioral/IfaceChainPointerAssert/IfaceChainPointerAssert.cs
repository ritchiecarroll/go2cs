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

internal static void Main() {
    readCloser rc = new connжreadCloser(Ꮡ(new conn(data: "init")));

    reader r = new readCloserᴠreader(rc);

    {
        var (p, ok) = r._<ж<conn>>(ᐧ); if (ok) {
            p.Value.data = "mutated";
            fmt.Println("chain assert ok:", p.Read());
        } else {
            fmt.Println("chain assert MISSED");
        }
    }

    fmt.Println("through chain:", r.Read());
    fmt.Println("through source:", rc.Close());

    switch (r.type()) {
    case ж<conn> v: {
        fmt.Println("switch *conn:", (~v).data);
        break;
    }
    default: {
        var v = r;
        fmt.Println("switch default");
        break;
    }}

    readCloser vrc = new valConn(tag: "v1");
    reader vr = new readCloserᴠreader(vrc);

    {
        var (_, ok) = vr._<ж<conn>>(ᐧ); if (ok) {
            fmt.Println("value chain WRONGLY asserted *conn");
        } else {
            fmt.Println("value chain misses *conn:", vr.Read());
        }
    }

    {
        var (v, ok) = vr._<valConn>(ᐧ); if (ok) {
            fmt.Println("value chain asserts valConn:", v.tag);
        } else {
            fmt.Println("value chain MISSED valConn");
        }
    }

    switch (vr.type()) {
    case ж<conn> v: {
        fmt.Println("value switch WRONGLY matched *conn:", (~v).data);
        break;
    }
    case valConn v: {
        fmt.Println("value switch valConn:", v.tag);
        break;
    }
    default: {
        var v = vr;
        fmt.Println("value switch default");
        break;
    }}

    reader empty = default!;
    {
        var (_, ok) = empty._<ж<conn>>(ᐧ); if (ok) {
            fmt.Println("nil interface WRONGLY asserted");
        } else {
            fmt.Println("nil interface misses");
        }
    }
}

} // end main_package

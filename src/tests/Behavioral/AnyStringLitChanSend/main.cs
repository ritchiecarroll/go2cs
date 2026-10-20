namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface speaker {
    @string speak();
}

partial struct dog {
    internal @string name;
}

internal static @string speak(this dog d) {
    return "woof:"u8 + d.name;
}

partial struct cat {
    internal @string name;
}

internal static @string speak(this ref cat c) {
    return "meow:"u8 + c.name;
}

internal static void describe(@string prefix, any v) {
    switch (v.type()) {
    case @string s: {
        fmt.Println(prefix, "string:", s);
        break;
    }
    case nint s: {
        fmt.Println(prefix, "int:", s);
        break;
    }
    case int32 s: {
        fmt.Println(prefix, "int:", s);
        break;
    }
    default: {
        var s = v;
        fmt.Println(prefix, "other:", s);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object textˢ = (@string)"text"u8;
private static readonly object selˢ = (@string)"sel"u8;
private static readonly object assertˢ = (@string)"assert"u8;

internal static void Main() {
    var ch = new channel<any>(2);
    ch.ᐸꟷ(textˢ);
    ch.ᐸꟷ((nint)42);
    describe("send", ᐸꟷ(ch));
    describe("send", ᐸꟷ(ch));

    var sel = new channel<any>(1);
    var selᴛ1 = sel.ᐸꟷ(selˢ, ꓸꓸꓸ);
    switch (select(selᴛ1)) {
    case 0: {
        describe("select", ᐸꟷ(sel));
        break;
    }}

    var sc = new channel<@string>(1);
    sc.ᐸꟷ("plain");
    fmt.Println("string chan:", ᐸꟷ(sc));

    var vs = new channel<speaker>(1);
    vs.ᐸꟷ(new dog(name: "rex"));
    fmt.Println((ᐸꟷ(vs)).speak());

    var ps = new channel<speaker>(1);
    var c = Ꮡ(new cat(name: "tom"));
    ps.ᐸꟷ(new catжspeaker(c));
    fmt.Println((ᐸꟷ(ps)).speak());

    var rt = new channel<any>(1);
    rt.ᐸꟷ(assertˢ);
    var got = ᐸꟷ(rt);
    {
        var (s, ok) = got._<@string>(ᐧ); if (ok){
            fmt.Println("assert string:", s);
        } else {
            fmt.Println("assert missed");
        }
    }
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object matchˢ = (@string)"match"u8;
private static readonly object copySafeˢ = (@string)"copy-safe"u8;
private static readonly object taggedˢ = (@string)"tagged"u8;
private static readonly object wantedˢ = (@string)"wanted"u8;
private static readonly object classifyˢ = (@string)"classify:"u8;
private static readonly object pickˢ = (@string)"pick:"u8;
private static readonly object prefixˢ = (@string)"prefix:"u8;
private static readonly object matchVarˢ = (@string)"matchVar:"u8;
private static readonly @string go2csˢ = "go2cs"u8;
private static readonly object matchFieldˢ = (@string)"matchField:"u8;
private static readonly object twoConvˢ = (@string)"twoConv:"u8;
private static readonly object callOperandˢ = (@string)"callOperand:"u8;
private static readonly object switchTagˢ = (@string)"switchTag:"u8;
private static readonly object switchLocalˢ = (@string)"switchLocal:"u8;
private static readonly object switchMagicˢ = (@string)"switchMagic:"u8;
private static readonly object switchCallˢ = (@string)"switchCall:"u8;
private static readonly object concatLocalˢ = (@string)"concatLocal:"u8;
private static readonly object concatLitˢ = (@string)"concatLit:"u8;
private static readonly object concatVarˢ = (@string)"concatVar:"u8;
private static readonly object concatTwoˢ = (@string)"concatTwo:"u8;
private static readonly object concatCallˢ = (@string)"concatCall:"u8;
private static readonly object concatObjˢ = (@string)"concatObj:"u8;

internal static void Main() {
    var name = slice<byte>("go2cs"u8);
    sstring s = ((sstring)name);
    if (s == "go2cs"u8) {
        fmt.Println(matchˢ);
    }
    var digits = slice<byte>("2468"u8);
    sstring d = ((sstring)digits);
    fmt.Println((nint)d[0] + (nint)d[3] + len(d));
    var scratch = slice<byte>("AB"u8);
    @string t = ((@string)scratch);
    scratch[0] = (rune)'X';
    if (t == "AB"u8) {
        fmt.Println(copySafeˢ);
    }
    @string u = ((@string)slice<byte>("printed"u8));
    fmt.Println(u);
    fmt.Println(returnedString());
    var tag = slice<byte>("v2"u8);
    sstring tagᴛ1 = ((sstring)tag);
    if (tagᴛ1 == "v2"u8) {
        fmt.Println(taggedˢ);
    }
    @string want = "v2"u8;
    if (tagᴛ1 == want) {
        fmt.Println(wantedˢ);
    }
    fmt.Println(classifyˢ, classify(slice<byte>("rust"u8)));
    fmt.Println(pickˢ, pick(slice<byte>("b"u8)));
    fmt.Println(prefixˢ, prefix(slice<byte>("GET /x"u8)));
    fmt.Println(matchVarˢ, matchVar(slice<byte>("go2cs"u8), go2csˢ));
    fmt.Println(matchFieldˢ, matchField(slice<byte>("prod"u8), new config(name: "prod"u8)));
    fmt.Println(twoConvˢ, matchTwoConversions(slice<byte>("abc"u8), slice<byte>("abc"u8)));
    fmt.Println(callOperandˢ, staysHeapCallOperand(slice<byte>("y"u8), () => "y"u8));
    fmt.Println(switchTagˢ, switchTag(slice<byte>("put"u8)));
    fmt.Println(switchLocalˢ, switchLocal(slice<byte>("off"u8)));
    fmt.Println(switchMagicˢ, switchMagic(slice<byte>("PK"u8)));
    fmt.Println(switchCallˢ, switchCall(slice<byte>("q"u8)));
    fmt.Println(concatLocalˢ, concatLocal(slice<byte>("go"u8), "2cs"u8));
    fmt.Println(concatLitˢ, concatLit(slice<byte>("v"u8)));
    fmt.Println(concatVarˢ, concatVar(slice<byte>("k"u8), "v"u8));
    fmt.Println(concatTwoˢ, concatTwo(slice<byte>("x"u8), slice<byte>("y"u8)));
    fmt.Println(concatCallˢ, concatCall(slice<byte>("q"u8), () => "z"u8));
    fmt.Println(concatObjˢ, concatObj(slice<byte>("x"u8)));
}

internal static @string concatObj(slice<byte> b) {
    return fmt.Sprint("v=" + ((sstring)b));
}

internal static @string concatLocal(slice<byte> b, @string suffix) {
    sstring s = ((sstring)b);
    return s + suffix;
}

internal static @string concatLit(slice<byte> b) {
    return ((sstring)b) + "!"u8;
}

internal static @string concatVar(slice<byte> b, @string suffix) {
    return ((sstring)b) + suffix;
}

internal static @string concatTwo(slice<byte> a, slice<byte> b) {
    return ((sstring)a) + ((sstring)b);
}

internal static @string concatCall(slice<byte> b, Func<@string> f) {
    return ((@string)b) + f();
}

internal static nint switchTag(slice<byte> b) {
    var exprᴛ1 = ((sstring)b);
    if (exprᴛ1 == "get"u8) {
        return 1;
    }
    if (exprᴛ1 == "put"u8) {
        return 2;
    }

    return 0;
}

internal static nint switchLocal(slice<byte> b) {
    sstring s = ((sstring)b);
    var exprᴛ1 = s;
    if (exprᴛ1 == "on"u8) {
        return 1;
    }
    if (exprᴛ1 == "off"u8) {
        return 2;
    }

    return -1;
}

internal static readonly @string zipMagic = "PK"u8;

internal static readonly @string gzMagic = ((@string)(new byte[]{0x1f, 0x8b}));

internal static nint switchMagic(slice<byte> b) {
    var exprᴛ1 = ((sstring)b);
    if (exprᴛ1 == zipMagic) {
        return 1;
    }
    if (exprᴛ1 == gzMagic) {
        return 2;
    }

    return 0;
}

internal static nint switchCall(slice<byte> b) {
    var exprᴛ1 = ((@string)b);
    if (exprᴛ1 == labelValue()) {
        return 1;
    }

    return 0;
}

internal static @string labelValue() {
    return "q"u8;
}

partial struct config {
    internal @string name;
}

internal static bool matchVar(slice<byte> b, @string want) {
    sstring s = ((sstring)b);
    return s == want;
}

internal static bool matchField(slice<byte> b, config cfg) {
    sstring s = ((sstring)b);
    return s == cfg.name;
}

internal static bool matchTwoConversions(slice<byte> a, slice<byte> b) {
    return ((sstring)a) == ((sstring)b);
}

internal static bool staysHeapCallOperand(slice<byte> a, Func<@string> next) {
    return ((@string)a) == next();
}

internal static @string returnedString() {
    var b = slice<byte>("returned"u8);
    @string r = ((@string)b);
    return r;
}

internal static nint classify(slice<byte> word) {
    nint total = 0;
    sstring wordᴛ1 = ((sstring)word);
    for (nint i = 0; i < 2; i++) {
        if (wordᴛ1 == "go"u8) {
            total++;
        }
        if (wordᴛ1 == "rust"u8) {
            total += 10;
        }
        if (wordᴛ1 == "c"u8) {
            total += 100;
        }
    }
    return total;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string alphaˢ = "alpha"u8;
private static readonly @string bravoˢ = "bravo"u8;
private static readonly @string otherˢ = "other"u8;

internal static @string pick(slice<byte> tag) {
    sstring tagᴛ1 = ((sstring)tag);
    if (tagᴛ1 == "a"u8) {
        return alphaˢ;
    }
    if (tagᴛ1 == "b"u8) {
        return bravoˢ;
    }
    return otherˢ;
}

internal static bool prefix(slice<byte> buf) {
    if (len(buf) < 6) {
        return false;
    }
    if (((sstring)(buf[..3])) != "GET"u8) {
        return false;
    }
    return ((sstring)(buf[3..6])) == " /x"u8;
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var name = slice<byte>("go2cs"u8);
    sstring s = (sstring)name;
    if (s == "go2cs"u8) {
        fmt.Println("match");
    }

    var digits = slice<byte>("2468"u8);
    sstring d = (sstring)digits;
    fmt.Println((nint)d[0] + (nint)d[3] + len(d));

    var scratch = slice<byte>("AB"u8);
    @string t = (@string)scratch;
    scratch[0] = (byte)'X';
    if (t == "AB"u8) {
        fmt.Println("copy-safe");
    }

    @string u = (@string)slice<byte>("printed"u8);
    fmt.Println(u);

    fmt.Println(returnedString());

    var tag = slice<byte>("v2"u8);
    sstring tagᴛ1 = ((sstring)tag);
    if (tagᴛ1 == "v2"u8) {
        fmt.Println("tagged");
    }

    @string want = "v2";
    if (tagᴛ1 == want) {
        fmt.Println("wanted");
    }

    fmt.Println("classify:", classify(slice<byte>("rust"u8)));

    fmt.Println("pick:", pick(slice<byte>("b"u8)));

    fmt.Println("prefix:", prefix(slice<byte>("GET /x"u8)));

    fmt.Println("matchVar:", matchVar(slice<byte>("go2cs"u8), "go2cs"));

    fmt.Println("matchField:", matchField(slice<byte>("prod"u8), new config(name: "prod")));

    fmt.Println("twoConv:", matchTwoConversions(slice<byte>("abc"u8), slice<byte>("abc"u8)));

    fmt.Println("callOperand:", staysHeapCallOperand(slice<byte>("y"u8), () => "y"));

    fmt.Println("switchTag:", switchTag(slice<byte>("put"u8)));

    fmt.Println("switchLocal:", switchLocal(slice<byte>("off"u8)));

    fmt.Println("switchMagic:", switchMagic(slice<byte>("PK"u8)));

    fmt.Println("switchCall:", switchCall(slice<byte>("q"u8)));

    fmt.Println("concatLocal:", concatLocal(slice<byte>("go"u8), "2cs"));

    fmt.Println("concatLit:", concatLit(slice<byte>("v"u8)));

    fmt.Println("concatVar:", concatVar(slice<byte>("k"u8), "v"));

    fmt.Println("concatTwo:", concatTwo(slice<byte>("x"u8), slice<byte>("y"u8)));

    fmt.Println("concatCall:", concatCall(slice<byte>("q"u8), () => "z"));

    fmt.Println("concatObj:", concatObj(slice<byte>("x"u8)));
}

internal static @string concatObj(slice<byte> b) {
    return fmt.Sprint("v=" + ((sstring)b));
}

internal static @string concatLocal(slice<byte> b, @string suffix) {
    sstring s = (sstring)b;
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
    sstring s = (sstring)b;
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
    return "q";
}

partial struct config {
    internal @string name;
}

internal static bool matchVar(slice<byte> b, @string want) {
    sstring s = (sstring)b;
    return s == want;
}

internal static bool matchField(slice<byte> b, config cfg) {
    sstring s = (sstring)b;
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
    @string r = (@string)b;
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

internal static @string pick(slice<byte> tag) {
    sstring tagᴛ1 = ((sstring)tag);
    if (tagᴛ1 == "a"u8) {
        return "alpha";
    }
    if (tagᴛ1 == "b"u8) {
        return "bravo";
    }
    return "other";
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

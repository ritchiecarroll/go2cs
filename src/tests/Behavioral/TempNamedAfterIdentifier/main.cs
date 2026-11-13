namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using strings = strings_package;

partial class main_package {

internal static @string mode = "fast";

internal static readonly @string kind = "b"u8;

internal static void goos() {
    @string os = runtime.GOOS;
    var exprᴛ1 = os;
    switch (ᐧ) {
    case {} when exprᴛ1 == "darwin"u8: {
        fmt.Println("macOS");
        break;
    }
    case {} when exprᴛ1 == "linux"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "freebsd"u8: {
        fmt.Println("known");
        break;
    }
    default: {
        fmt.Println("other");
        break;
    }}

}

internal static void shapes(@string s, nint n) {
    var exprᴛ1 = n;
    var matchᴛ1 = false;
    if (exprᴛ1 is 1) { matchᴛ1 = true;
        n = 3;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 is 2) {
        fmt.Println("two or fell", n);
    }
    else if (exprᴛ1 is 3) { matchᴛ1 = true;
        fmt.Println("three", n);
    }

    switch (n) {
    case 3: {
        n = 4;
        break;
    }
    case 4: {
        fmt.Println("never", n);
        break;
    }
    default: {
        fmt.Println("default", n);
        break;
    }}

    var exprᴛ2 = s;
    var matchᴛ2 = false;
    var matchᴛ3 = exprᴛ2 == "x"u8 || exprᴛ2 == "y"u8;
    if (!matchᴛ3) { /* default: */
        fmt.Println("default first", s);
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ2 && exprᴛ2 == "x"u8) {
        fmt.Println("x or after default", s);
    }
    else if (exprᴛ2 == "y"u8) { matchᴛ2 = true;
        fmt.Println("y", s);
    }

    var exprᴛ3 = s;
    switch (ᐧ) {
    case {} when exprᴛ3 == "a"u8 || exprᴛ3 == "b"u8: {
        fmt.Println("a or b");
        break;
    }
    case {} when exprᴛ3 == "c"u8: {
        fmt.Println("c");
        break;
    }}

}

internal static void refused(@string sʗp) {
    ref var s = ref heap(sʗp, out var Ꮡs);

    var exprᴛ1 = s;
    switch (ᐧ) {
    case {} when exprᴛ1 == strings.ToLower("A"): {
        fmt.Println("lower a");
        break;
    }
    case {} when exprᴛ1 == "b"u8: {
        fmt.Println("b");
        break;
    }}

    @string f() => Ꮡs.Value;

    var exprᴛ2 = s;
    switch (ᐧ) {
    case {} when exprᴛ2 == "b"u8: {
        fmt.Println("b again", f());
        break;
    }
    case {} when exprᴛ2 == "c"u8: {
        fmt.Println("c");
        break;
    }}

    var p = Ꮡs;

    var exprᴛ3 = s;
    switch (ᐧ) {
    case {} when exprᴛ3 == "b"u8: {
        p.Value = "changed";
        fmt.Println("b via pointer", s);
        break;
    }
    case {} when exprᴛ3 == "changed"u8: {
        fmt.Println("never");
        break;
    }}

    var exprᴛ4 = mode;
    switch (ᐧ) {
    case {} when exprᴛ4 == "fast"u8: {
        fmt.Println("fast");
        break;
    }
    case {} when exprᴛ4 == "slow"u8: {
        fmt.Println("slow");
        break;
    }}

    var exprᴛ5 = kind;
    switch (ᐧ) {
    case {} when exprᴛ5 == "a"u8: {
        fmt.Println("kind a");
        break;
    }
    case {} when exprᴛ5 == "b"u8: {
        fmt.Println("kind b");
        break;
    }}

}

internal static void keywordNamed(@string @base) {
    nint n = 0;
    if (len(@base) > 1) {
        n = 2;
    }

    var exprᴛ1 = @base;
    var matchᴛ1 = false;
    if (exprᴛ1 == "x"u8) {
        fmt.Println("x");
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == "base"u8) { matchᴛ1 = true;
        fmt.Println("base", n);
    }

}

internal static void selects() {
    var c = new channel<nint>(1);
    var quit = new channel<nint>(0);
    var done = new channel<bool>(0);

    var selᴛ1 = c.ᐸꟷ(1, ꓸꓸꓸ);
    var selᴛ2 = quit;
    switch (select(selᴛ1, ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0: {
        fmt.Println("sent");
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out var v): {
        fmt.Println("quit", v);
        break;
    }}

    var selᴛ3 = c;
    var selᴛ4 = c;
    var selᴛ5 = done.ᐸꟷ(true, ꓸꓸꓸ);
    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ), selᴛ5)) {
    case 0 when selᴛ3.ꟷᐳ(out var v): {
        fmt.Println("received", v);
        break;
    }
    case 1 when selᴛ4.ꟷᐳ(out var v, out var ok): {
        if (ok) {
            fmt.Println("received", v);
        }
        break;
    }
    case 2: {
        fmt.Println("done");
        break;
    }}
}

internal static void main() {
    goos();
    shapes("y", 1);
    shapes("z", 2);
    refused("b");
    keywordNamed("base");
    keywordNamed("x");
    selects();
}

} // end main_package

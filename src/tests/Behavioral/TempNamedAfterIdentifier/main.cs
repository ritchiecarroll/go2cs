namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using strings = strings_package;

partial class main_package {

internal static @string mode = "fast";

internal static readonly @string kind = "b"u8;

internal static void goos() {
    @string os = runtime.GOOS;
    switch (ᐧ) {
    case {} when os == "darwin"u8: {
        fmt.Println("macOS");
        break;
    }
    case {} when os == "linux"u8 || os == "windows"u8 || os == "freebsd"u8: {
        fmt.Println("known");
        break;
    }
    default: {
        fmt.Println("other");
        break;
    }}

}

internal static void shapes(@string s, nint n) {
    var nᴛ1 = n;
    var matchᴛ1 = false;
    if (nᴛ1 is 1) { matchᴛ1 = true;
        n = 3;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && nᴛ1 is 2) {
        fmt.Println("two or fell", n);
    }
    else if (nᴛ1 is 3) { matchᴛ1 = true;
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

    var matchᴛ2 = false;
    var matchᴛ3 = s == "x"u8 || s == "y"u8;
    if (!matchᴛ3) { /* default: */
        fmt.Println("default first", s);
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ2 && s == "x"u8) {
        fmt.Println("x or after default", s);
    }
    else if (s == "y"u8) { matchᴛ2 = true;
        fmt.Println("y", s);
    }

    switch (ᐧ) {
    case {} when s == "a"u8 || s == "b"u8: {
        fmt.Println("a or b");
        break;
    }
    case {} when s == "c"u8: {
        fmt.Println("c");
        break;
    }}

}

internal static void refused(@string sʗp) {
    ref var s = ref heap(sʗp, out var Ꮡs);

    var sᴛ1 = s;
    switch (ᐧ) {
    case {} when sᴛ1 == strings.ToLower("A"): {
        fmt.Println("lower a");
        break;
    }
    case {} when sᴛ1 == "b"u8: {
        fmt.Println("b");
        break;
    }}

    @string f() => Ꮡs.Value;

    var sᴛ2 = s;
    switch (ᐧ) {
    case {} when sᴛ2 == "b"u8: {
        fmt.Println("b again", f());
        break;
    }
    case {} when sᴛ2 == "c"u8: {
        fmt.Println("c");
        break;
    }}

    var p = Ꮡs;

    var sᴛ3 = s;
    switch (ᐧ) {
    case {} when sᴛ3 == "b"u8: {
        p.Value = "changed";
        fmt.Println("b via pointer", s);
        break;
    }
    case {} when sᴛ3 == "changed"u8: {
        fmt.Println("never");
        break;
    }}

    var modeᴛ1 = mode;
    switch (ᐧ) {
    case {} when modeᴛ1 == "fast"u8: {
        fmt.Println("fast");
        break;
    }
    case {} when modeᴛ1 == "slow"u8: {
        fmt.Println("slow");
        break;
    }}

    var kindᴛ1 = kind;
    switch (ᐧ) {
    case {} when kindᴛ1 == "a"u8: {
        fmt.Println("kind a");
        break;
    }
    case {} when kindᴛ1 == "b"u8: {
        fmt.Println("kind b");
        break;
    }}

}

internal static void keywordNamed(@string @base) {
    nint n = 0;
    if (len(@base) > 1) {
        n = 2;
    }

    var matchᴛ1 = false;
    if (@base == "x"u8) {
        fmt.Println("x");
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && @base == "base"u8) { matchᴛ1 = true;
        fmt.Println("base", n);
    }

}

internal static void selects() {
    var c = new channel<nint>(1);
    var quit = new channel<nint>(0);
    var done = new channel<bool>(0);

    var cᴛ1 = c.ᐸꟷ(1, ꓸꓸꓸ);
    var quitᴛ1 = quit;
    switch (select(cᴛ1, ᐸꟷ(quitᴛ1, ꓸꓸꓸ))) {
    case 0: {
        fmt.Println("sent");
        break;
    }
    case 1 when quitᴛ1.ꟷᐳ(out var v): {
        fmt.Println("quit", v);
        break;
    }}

    var cᴛ2 = c;
    var cᴛ3 = c;
    var doneᴛ1 = done.ᐸꟷ(true, ꓸꓸꓸ);
    switch (select(ᐸꟷ(cᴛ2, ꓸꓸꓸ), ᐸꟷ(cᴛ3, ꓸꓸꓸ), doneᴛ1)) {
    case 0 when cᴛ2.ꟷᐳ(out var v): {
        fmt.Println("received", v);
        break;
    }
    case 1 when cᴛ3.ꟷᐳ(out var v, out var ok): {
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

namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = variadic<any>;

partial class main_package {

internal static @string escaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp;

    return fmt.Sprint(args.ꓸꓸꓸ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object plainˢ = (@string)"plain"u8;

internal static void main() {
    Actionꓸꓸꓸ<@string, any> fn = [GoFunc("main.func1")] (@string format, params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp;
        fmt.Printf(format + "\n"u8, args.ꓸꓸꓸ);
    };
    any logf = (((Actionꓸꓸꓸ<@string, any>)(fn))).OrTypedNilFunc();

    if (logf._ᶠ<Actionꓸꓸꓸ<@string, any>>(out var fnΔ1)) {
        fnΔ1("value=%v flag=%v", (nint)42, true);
    } else {
        fmt.Println("no match");
    }

    any notFn = plainˢ;
    if (notFn._ᶠ<Actionꓸꓸꓸ<@string, any>>(out _)) {
        fmt.Println("unexpected match");
    } else {
        fmt.Println("no match for string");
    }

    any plain = [GoFunc("main.func2")] @string (@string s) => s + "!"u8;
    if (plain._ᶠ<Func<@string, @string>>(out var fnΔ2)) {
        fmt.Println(fnΔ2("ok"));
    }

    any direct = ((Funcꓸꓸꓸ<any, @string>)([GoFunc("main.func3")] @string (params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp;
        return fmt.Sprint(args.ꓸꓸꓸ);
    }));
    {
        var (f, ok) = direct._ᶠ<Funcꓸꓸꓸ<any, @string>>(ᐧ); if (ok) {
            fmt.Println("direct:", f((nint)1, (nint)2));
        } else {
            fmt.Println("direct: no match");
        }
    }

    var funcMap = new map<@string, any>{["esc"] = ((Funcꓸꓸꓸ<any, @string>)(escaper))};
    {
        var (f, ok) = funcMap["esc"]._ᶠ<Funcꓸꓸꓸ<any, @string>>(ᐧ); if (ok) {
            fmt.Println("mapped:", f((@string)"a", (@string)"b"));
        } else {
            fmt.Println("mapped: no match");
        }
    }

    any assigned = ((Funcꓸꓸꓸ<any, @string>)(escaper));
    {
        var (f, ok) = assigned._ᶠ<Funcꓸꓸꓸ<any, @string>>(ᐧ); if (ok) {
            fmt.Println("assigned:", f((@string)"x"));
        } else {
            fmt.Println("assigned: no match");
        }
    }

    var slots = slice<any>([(Funcꓸꓸꓸ<any, @string>)(escaper)]);
    {
        var (f, ok) = slots[0]._ᶠ<Funcꓸꓸꓸ<any, @string>>(ᐧ); if (ok) {
            fmt.Println("element:", f((@string)"y"));
        } else {
            fmt.Println("element: no match");
        }
    }

    any directPlain = [GoFunc("main.func4")] @string (@string s) => s + "?"u8;
    {
        var (f, ok) = directPlain._ᶠ<Func<@string, @string>>(ᐧ); if (ok) {
            fmt.Println("direct plain:", f("z"));
        } else {
            fmt.Println("direct plain: no match");
        }
    }
}

} // end main_package

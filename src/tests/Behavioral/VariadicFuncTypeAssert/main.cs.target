namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

internal static @string escaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return fmt.Sprint(args.ꓸꓸꓸ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object plainˢ = (@string)"plain"u8;

internal static void Main() {
    Actionꓸꓸꓸ<@string, any> fn = (@string format, params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp.sslice();
        fmt.Printf(format + "\n"u8, args.ꓸꓸꓸ);
    };
    any logf = (((Actionꓸꓸꓸ<@string, any>)(fn))).OrTypedNilFunc();

    {
        var (fnΔ1, ok) = logf._<Actionꓸꓸꓸ<@string, any>>(ᐧ); if (ok) {
            fnΔ1("value=%v flag=%v", (nint)42, true);
        } else {
            fmt.Println("no match");
        }
    }

    any notFn = plainˢ;
    {
        var (_, ok) = notFn._<Actionꓸꓸꓸ<@string, any>>(ᐧ); if (ok) {
            fmt.Println("unexpected match");
        } else {
            fmt.Println("no match for string");
        }
    }

    any plain = @string (@string s) => s + "!"u8;
    {
        var (fnΔ2, ok) = plain._<Func<@string, @string>>(ᐧ); if (ok) {
            fmt.Println(fnΔ2("ok"));
        }
    }

    any direct = ((Funcꓸꓸꓸ<any, @string>)(@string (params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp.sslice();
        return fmt.Sprint(args.ꓸꓸꓸ);
    }));
    {
        var (f, ok) = direct._<Funcꓸꓸꓸ<any, @string>>(ᐧ); if (ok) {
            fmt.Println("direct:", f((nint)1, (nint)2));
        } else {
            fmt.Println("direct: no match");
        }
    }

    var funcMap = new map<@string, any>{["esc"] = ((Funcꓸꓸꓸ<any, @string>)(escaper))};
    {
        var (f, ok) = funcMap["esc"]._<Funcꓸꓸꓸ<any, @string>>(ᐧ); if (ok) {
            fmt.Println("mapped:", f((@string)"a"u8, (@string)"b"u8));
        } else {
            fmt.Println("mapped: no match");
        }
    }

    any assigned = ((Funcꓸꓸꓸ<any, @string>)(escaper));
    {
        var (f, ok) = assigned._<Funcꓸꓸꓸ<any, @string>>(ᐧ); if (ok) {
            fmt.Println("assigned:", f((@string)"x"u8));
        } else {
            fmt.Println("assigned: no match");
        }
    }

    var slots = new any[]{(Funcꓸꓸꓸ<any, @string>)(escaper)}.slice();
    {
        var (f, ok) = slots[0]._<Funcꓸꓸꓸ<any, @string>>(ᐧ); if (ok) {
            fmt.Println("element:", f((@string)"y"u8));
        } else {
            fmt.Println("element: no match");
        }
    }

    any directPlain = @string (@string s) => s + "?"u8;
    {
        var (f, ok) = directPlain._<Func<@string, @string>>(ᐧ); if (ok) {
            fmt.Println("direct plain:", f("z"));
        } else {
            fmt.Println("direct plain: no match");
        }
    }
}

} // end main_package

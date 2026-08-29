namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸfmt() {
    builtin.initPackage(typeof(fmt_package));
}

internal static @string escaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    return fmt.Sprint(args.ꓸꓸꓸ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string valueVFlagVˢ = "value=%v flag=%v"u8;
private static readonly object noMatchˢ = (@string)"no match"u8;
private static readonly object plainˢ = (@string)"plain"u8;
private static readonly object unexpectedMatchˢ = (@string)"unexpected match"u8;
private static readonly object noMatchForStringˢ = (@string)"no match for string"u8;
private static readonly object directˢ = (@string)"direct:"u8;
private static readonly object directNoMatchˢ = (@string)"direct: no match"u8;
private static readonly @string escˢ = "esc"u8;
private static readonly object mappedˢ = (@string)"mapped:"u8;
private static readonly object mappedNoMatchˢ = (@string)"mapped: no match"u8;
private static readonly object assignedˢ = (@string)"assigned:"u8;
private static readonly object assignedNoMatchˢ = (@string)"assigned: no match"u8;
private static readonly object elementˢ = (@string)"element:"u8;
private static readonly object elementNoMatchˢ = (@string)"element: no match"u8;
private static readonly object directPlainˢ = (@string)"direct plain:"u8;
private static readonly object directPlainNoMatchˢ = (@string)"direct plain: no match"u8;

internal static void Main() {
    Actionꓸꓸꓸ<@string, any> fn = (@string format, params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp.slice();
        fmt.Printf(format + "\n"u8, args.ꓸꓸꓸ);
    };
    any logf = ((Actionꓸꓸꓸ<@string, any>)(fn)).OrTypedNilFunc();
    {
        var (fnΔ1, ok) = logf._<Actionꓸꓸꓸ<@string, any>>(ᐧ); if (ok){
            fnΔ1(valueVFlagVˢ, (nint)(42), true);
        } else {
            fmt.Println(noMatchˢ);
        }
    }
    any notFn = plainˢ;
    {
        var (_, ok) = notFn._<Actionꓸꓸꓸ<@string, any>>(ᐧ); if (ok){
            fmt.Println(unexpectedMatchˢ);
        } else {
            fmt.Println(noMatchForStringˢ);
        }
    }
    any plain = (@string s) => s + "!"u8;
    {
        var (fnΔ2, ok) = plain._<Func<@string, @string>>(ᐧ); if (ok) {
            fmt.Println(fnΔ2("ok"u8));
        }
    }
    any direct = ((Funcꓸꓸꓸ<any, @string>)((params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp.slice();
        return fmt.Sprint(args.ꓸꓸꓸ);
    }));
    {
        var (f, ok) = direct._<Funcꓸꓸꓸ<any, @string>>(ᐧ); if (ok){
            fmt.Println(directˢ, f((nint)(1), (nint)(2)));
        } else {
            fmt.Println(directNoMatchˢ);
        }
    }
    var funcMap = new map<@string, any>{["esc"u8] = ((Funcꓸꓸꓸ<any, @string>)(escaper))};
    {
        var (f, ok) = funcMap[escˢ]._<Funcꓸꓸꓸ<any, @string>>(ᐧ); if (ok){
            fmt.Println(mappedˢ, f((@string)"a"u8, (@string)"b"u8));
        } else {
            fmt.Println(mappedNoMatchˢ);
        }
    }
    any assigned = ((Funcꓸꓸꓸ<any, @string>)(escaper));
    {
        var (f, ok) = assigned._<Funcꓸꓸꓸ<any, @string>>(ᐧ); if (ok){
            fmt.Println(assignedˢ, f((@string)"x"u8));
        } else {
            fmt.Println(assignedNoMatchˢ);
        }
    }
    var slots = new any[]{(Funcꓸꓸꓸ<any, @string>)(escaper)}.slice();
    {
        var (f, ok) = slots[0]._<Funcꓸꓸꓸ<any, @string>>(ᐧ); if (ok){
            fmt.Println(elementˢ, f((@string)"y"u8));
        } else {
            fmt.Println(elementNoMatchˢ);
        }
    }
    any directPlain = (@string s) => s + "?"u8;
    {
        var (f, ok) = directPlain._<Func<@string, @string>>(ᐧ); if (ok){
            fmt.Println(directPlainˢ, f("z"u8));
        } else {
            fmt.Println(directPlainNoMatchˢ);
        }
    }
}

} // end main_package

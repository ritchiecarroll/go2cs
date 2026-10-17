namespace go;

using fmt = fmt_package;
using runtime = runtime_package;

partial class main_package {

partial struct celsius /*num:float64*/;

partial struct point {
    internal nint x, y;
}

internal static void @catch(@string what, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!){
                    var (_, isRuntimeError) = r._<runtimeꓸError>(ᐧ);
                    var (_, isError) = r._<error>(ᐧ);
                    fmt.Printf("%s: %T runtime.Error: %v error: %v: %v\n"u8, what, r, isRuntimeError, isError, r);
                } else {
                    fmt.Printf("%s: NO PANIC\n"u8, what);
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string intToStringˢ = "int-to-string"u8;
private static readonly @string intToCelsiusˢ = "int-to-celsius"u8;
private static readonly @string intToPointˢ = "int-to-*point"u8;
private static readonly @string pointToPointˢ = "point-to-*point"u8;
private static readonly @string nilToStringˢ = "nil-to-string"u8;
private static readonly @string intToIntˢ = "int-to-int"u8;
private static readonly @string commaOkˢ = "comma-ok"u8;
private static readonly object commaOkˢ2 = (@string)"comma-ok:"u8;
private static readonly object stillRunningˢ = (@string)"still running"u8;

internal static void Main() {
    any boxed = (nint)(7);

    var boxedʗ1 = boxed;
    @catch(intToStringˢ, () => {
        _ = boxedʗ1._<@string>();
    });

    var boxedʗ2 = boxed;
    @catch(intToCelsiusˢ, () => {
        _ = boxedʗ2._<celsius>();
    });
    var boxedʗ3 = boxed;
    @catch(intToPointˢ, () => {
        _ = boxedʗ3._<ж<point>>();
    });

    any p = new point(1, 2);
    var pʗ1 = p;
    @catch(pointToPointˢ, () => {
        _ = pʗ1._<ж<point>>();
    });

    any nothing = default!;
    var nothingʗ1 = nothing;
    @catch(nilToStringˢ, () => {
        _ = nothingʗ1._<@string>();
    });

    var boxedʗ4 = boxed;
    @catch(intToIntˢ, () => {
        _ = boxedʗ4._<nint>();
    });
    var boxedʗ5 = boxed;
    @catch(commaOkˢ, () => {
        var (_, ok) = boxedʗ5._<@string>(ᐧ);
        fmt.Println(commaOkˢ2, ok);
    });

    fmt.Println(stillRunningˢ);
}

} // end main_package

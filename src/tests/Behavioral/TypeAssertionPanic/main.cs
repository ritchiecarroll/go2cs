namespace go;

using errors = errors_package;
using fmt = fmt_package;
using runtime = runtime_package;

partial class main_package {

partial struct celsius /*num:float64*/;

partial struct myErr {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string myErrˢ = "myErr"u8;

internal static @string Error(this ref myErr _) {
    return myErrˢ;
}

partial interface shape {
    float64 area();
    float64 perimeter();
}

partial struct square {
    internal float64 side;
}

internal static float64 perimeter(this square s) {
    return 4D * s.side;
}

partial struct circle {
    internal float64 r;
}

internal static float64 area(this ref circle c) {
    return 3D * c.r * c.r;
}

internal static float64 perimeter(this ref circle c) {
    return 6D * c.r;
}

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
private static readonly @string intToRuntimeErrorˢ = "int-to-runtime.Error"u8;
private static readonly @string nilToRuntimeErrorˢ = "nil-to-runtime.Error"u8;
private static readonly @string squareToShapeˢ = "square-to-shape"u8;
private static readonly @string circleToShapeˢ = "circle-to-shape"u8;
private static readonly @string boomˢ = "boom"u8;
private static readonly @string errorToMyErrˢ = "error-to-*myErr"u8;
private static readonly @string intToIntˢ = "int-to-int"u8;
private static readonly @string errorToErrorˢ = "error-to-error"u8;
private static readonly @string circleToShapeˢ2 = "*circle-to-shape"u8;
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
    @catch(intToRuntimeErrorˢ, () => {
        _ = boxedʗ4._<runtimeꓸError>();
    });
    var nothingʗ2 = nothing;
    @catch(nilToRuntimeErrorˢ, () => {
        _ = nothingʗ2._<runtimeꓸError>();
    });
    any sq = new square(2D);
    var sqʗ1 = sq;
    @catch(squareToShapeˢ, () => {
        _ = sqʗ1._<shape>();
    });
    any circ = new circle(1D);
    var circʗ1 = circ;
    @catch(circleToShapeˢ, () => {
        _ = circʗ1._<shape>();
    });

    error err = errors.New(boomˢ);
    var errʗ1 = err;
    @catch(errorToMyErrˢ, () => {
        _ = errʗ1._<ж<myErr>>();
    });

    var boxedʗ5 = boxed;
    @catch(intToIntˢ, () => {
        _ = boxedʗ5._<nint>();
    });
    any ptr = Ꮡ(new circle(1D));
    var errʗ2 = err;
    @catch(errorToErrorˢ, () => {
        _ = errʗ2._<error>();
    });
    var ptrʗ1 = ptr;
    @catch(circleToShapeˢ2, () => {
        _ = ptrʗ1._<shape>();
    });
    var boxedʗ6 = boxed;
    @catch(commaOkˢ, () => {
        var (_, ok) = boxedʗ6._<@string>(ᐧ);
        fmt.Println(commaOkˢ2, ok);
    });

    fmt.Println(stillRunningˢ);
}

} // end main_package

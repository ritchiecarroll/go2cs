namespace go;

using errors = errors_package;
using fmt = fmt_package;
using runtime = runtime_package;

partial class main_package {

partial struct celsius /*num:float64*/;

partial struct myErr {
}

internal static @string Error(this ref myErr _) { return "myErr"; }

partial interface shape {
    float64 area();
    float64 perimeter();
}

partial struct square {
    internal float64 side;
}

internal static float64 perimeter(this square s) { return 4D * s.side; }

partial struct circle {
    internal float64 r;
}

internal static float64 area(this ref circle c) { return 3D * c.r * c.r; }
internal static float64 perimeter(this ref circle c) { return 6D * c.r; }

partial struct point {
    internal nint x, y;
}

internal static void @catch(@string what, Action f) {
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("catch.func1")] () => {
            var r = recover(); if (r != nil) {
                var (_, isRuntimeError) = r._<runtime.Error>(ᐧ);
                var (_, isError) = r._<error>(ᐧ);
                fmt.Printf("%s: %T runtime.Error: %v error: %v: %v\n"u8, what, r, isRuntimeError, isError, r);
            } else {
                fmt.Printf("%s: NO PANIC\n"u8, what);
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void main() {
    any boxed = (nint)7;

    var boxedʗ1 = boxed;
    @catch("int-to-string", [GoFunc("main.func1")] () => {
        _ = boxedʗ1._<@string>();
    });

    var boxedʗ2 = boxed;
    @catch("int-to-celsius", [GoFunc("main.func2")] () => {
        _ = boxedʗ2._<celsius>();
    });
    var boxedʗ3 = boxed;
    @catch("int-to-*point", [GoFunc("main.func3")] () => {
        _ = boxedʗ3._<ж<point>>();
    });

    any p = new point(1, 2);
    var pʗ1 = p;
    @catch("point-to-*point", [GoFunc("main.func4")] () => {
        _ = pʗ1._<ж<point>>();
    });

    any nothing = default;
    var nothingʗ1 = nothing;
    @catch("nil-to-string", [GoFunc("main.func5")] () => {
        _ = nothingʗ1._<@string>();
    });

    var boxedʗ4 = boxed;
    @catch("int-to-runtime.Error", [GoFunc("main.func6")] () => {
        _ = boxedʗ4._<runtime.Error>();
    });
    var nothingʗ2 = nothing;
    @catch("nil-to-runtime.Error", [GoFunc("main.func7")] () => {
        _ = nothingʗ2._<runtime.Error>();
    });
    any sq = new square(2D);
    var sqʗ1 = sq;
    @catch("square-to-shape", [GoFunc("main.func8")] () => {
        _ = sqʗ1._<shape>();
    });
    any circ = new circle(1D);
    var circʗ1 = circ;
    @catch("circle-to-shape", [GoFunc("main.func9")] () => {
        _ = circʗ1._<shape>();
    });

    error err = errors.New("boom");
    var errʗ1 = err;
    @catch("error-to-*myErr", [GoFunc("main.func10")] () => {
        _ = errʗ1._<ж<myErr>>();
    });

    var boxedʗ5 = boxed;
    @catch("int-to-int", [GoFunc("main.func11")] () => {
        _ = boxedʗ5._<nint>();
    });
    any ptr = Ꮡ(new circle(1D));
    var errʗ2 = err;
    @catch("error-to-error", [GoFunc("main.func12")] () => {
        _ = errʗ2._<error>();
    });
    var ptrʗ1 = ptr;
    @catch("*circle-to-shape", [GoFunc("main.func13")] () => {
        _ = ptrʗ1._<shape>();
    });
    var boxedʗ6 = boxed;
    @catch("comma-ok", [GoFunc("main.func14")] () => {
        var (_, ok) = boxedʗ6._<@string>(ᐧ);
        fmt.Println("comma-ok:", ok);
    });

    fmt.Println("still running");
}

} // end main_package

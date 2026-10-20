namespace go;

using fmt = fmt_package;

partial class main_package {

internal static any retConst() {
    return (@string)(("r1" + "r2"));
}

internal static any retConstParen() {
    return (@string)(("r3" + "r4"));
}

internal static any retVar(@string a) {
    return a + "-r";
}

internal static any retVarParen(@string a) {
    return (a + "-r");
}

internal static (any, nint) pairConst() {
    return ((@string)("t1" + "t2"), 1);
}

internal static (any, nint) pairConstParen() {
    return ((@string)(("t3" + "t4")), 2);
}

internal static void Main() {
    @string a = "v";

    @catch(() => {
        throw panic("pn1" + "pn2");
    });
    @catch(() => {
        throw panic(("pn3" + "pn4"));
    });
    @catch(() => {
        throw panic(a + "-pn");
    });
    @catch(() => {
        throw panic((a + "-pn"));
    });

    fmt.Println((@string)("p1" + "p2"));
    fmt.Println((@string)(("p3" + "p4")));
    fmt.Println((@string)((("p5" + "p6"))));
    fmt.Println(a + "-x");
    fmt.Println((a + "-y"));

    fmt.Println("lit");
    fmt.Println((@string)(("lit")));

    any c1 = (@string)("q1" + "q2");
    any c2 = (@string)(("q3" + "q4"));
    any v1 = a + "-q";
    any v2 = (a + "-q");
    fmt.Println(c1, c2, v1, v2);

    fmt.Println(retConst(), retConstParen(), retVar(a), retVarParen(a));

    var (x, n) = pairConst();
    var (y, m) = pairConstParen();
    fmt.Println(x, n, y, m);

    var elems = new @string[]{a + "-g"u8, (a + "-h"u8)}.slice();
    fmt.Println(elems);
}

internal static void @catch(Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println("recovered:", recover());
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package

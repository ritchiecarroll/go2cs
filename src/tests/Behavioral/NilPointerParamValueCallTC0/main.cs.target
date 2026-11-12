namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct T {
    internal nint n;
}

public static @string Hello(this T _) { return "hello"; }

public static @string PtrHello(this ref T _) { return "ptr hello"; }

internal static ж<T> kept;

public static @string Via(this ж<T> Ꮡt) {
    kept = Ꮡt;
    return "via "u8 + Ꮡt.Hello();
}

internal static ж<T> sink;

internal static @string onlyCall(ж<T> p) { return p.Hello(); }

internal static @string alsoField(ж<T> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    @string s = Ꮡp.Hello();
    return s + " "u8 + fmt.Sprint(p.n);
}

internal static @string inClosure(ж<T> p) {
    @string f() => p.Hello();
    return f();
}

internal static @string escapes(ж<T> p) {
    sink = p;
    return p.Hello();
}

internal static @string mark(@string s) {
    fmt.Println("  evaluated:", s);
    return s;
}

internal static @string join(@string a, @string b, @string c) { return a + " "u8 + b + " "u8 + c; }

internal static @string argOrder(ж<T> p) { return join(mark("first"), p.Hello(), mark("third")); }

internal static @string ptrParam(ж<T> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    return p.PtrHello();
}

internal static void @try(@string name, Func<@string> f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover(); if (r != nil) {
                fmt.Println(name, "panicked:", r);
            }
        }, ref ᒐ);
        fmt.Println(name, "returned:", f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void main() {
    ж<T> p = default;
    var pʗ1 = p;
    @try("parameter, call only", () => onlyCall(pʗ1));
    var pʗ2 = p;
    @try("parameter, also reads a field", () => alsoField(pʗ2));
    var pʗ3 = p;
    @try("parameter captured by a closure", () => inClosure(pʗ3));
    var pʗ4 = p;
    @try("parameter escapes", () => escapes(pʗ4));
    var pʗ5 = p;
    @try("call as the middle argument", () => argOrder(pʗ5));
    var pʗ6 = p;
    @try("pointer receiver calls its value method", () => pʗ6.Via());
    var pʗ7 = p;
    @try("pointer-receiver method on a nil parameter", () => ptrParam(pʗ7));

    var q = Ꮡ(new T(n: 7));
    var qʗ1 = q;
    @try("non-nil, call only", () => onlyCall(qʗ1));
    var qʗ2 = q;
    @try("non-nil, also reads a field", () => alsoField(qʗ2));
    var qʗ3 = q;
    @try("non-nil, closure", () => inClosure(qʗ3));
    var qʗ4 = q;
    @try("non-nil, escapes", () => escapes(qʗ4));
    var qʗ5 = q;
    @try("non-nil, middle argument", () => argOrder(qʗ5));
    var qʗ6 = q;
    @try("non-nil, pointer receiver", () => qʗ6.Via());
    fmt.Println("sink is q:", sink == q, "kept is q:", kept == q);
}

} // end main_package

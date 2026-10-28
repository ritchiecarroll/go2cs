namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct T {
    internal nint n;
}

public static @string Hello(this T _) {
    return "hello";
}

public static nint N(this T t) {
    return t.n;
}

public static @string PtrHello(this ref T _) {
    return "ptr hello";
}

partial interface Helloer {
    @string Hello();
}

internal static @string direct(ж<T> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    return p.Hello();
}

internal static void @try(@string name, Func<@string> f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    fmt.Println(name, "panicked:", r);
                }
            }
        }, ref ᒐ);
        fmt.Println(name, "returned:", f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    ж<T> p = default;
    var pʗ1 = p;
    @try("value receiver, receiver unused", () => (~pʗ1).Hello());
    var pʗ2 = p;
    @try("value receiver, reads a field", () => fmt.Sprint((~pʗ2).N()));
    var pʗ3 = p;
    @try("pointer receiver, receiver unused", () => pʗ3.PtrHello());
    var pʗ4 = p;
    @try("direct call in a named func", () => direct(pʗ4));
    var pʗ5 = p;
    @try("method value", () => {
        var recvʗ1 = ~p;
        
        var pʗ6 = pʗ5;
        var f = () => recvʗ1.Hello();
        return "bound "u8 + f();
    });
    var pʗ7 = p;
    @try("through an interface", () => {
        Helloer h = new TжHelloer(pʗ7);
        return h.Hello();
    });

    var q = Ꮡ(new T(n: 7));
    var qʗ1 = q;
    @try("value receiver via non-nil pointer", () => fmt.Sprint((~qʗ1).Hello(), " ", (~qʗ1).N()));
}

} // end main_package

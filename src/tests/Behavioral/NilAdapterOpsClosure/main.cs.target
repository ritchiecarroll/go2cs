namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

public delegate void Greeter(@string name);

public static void Greet(this Greeter g, @string name) {
    g(name);
}

partial interface Greetable {
    void Greet(@string name);
}

internal static Greetable wrap(Action<@string> handler) {
    return new GreeterᴠGreetable(NilSafeDelegateConversion<Greeter, Action<@string>>(handler));
}

internal static void @try(@string label, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    fmt.Println(label, "panicked:", r);
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    @try("equality", () => {
        var (a, b) = (wrap(default), wrap(default));
        fmt.Println("a == b:", AreEqual(a, b));
    });

    @try("map-hash", () => {
        var m = new map<Greetable, nint>{};
        m[wrap(default)] = 1;
        fmt.Println("stored ok");
    });

    @try("reflect-set", () => {
        ref var dest = ref heap<Greetable>(out var Ꮡdest);
        var rv = reflect.ValueOf(Ꮡdest).Elem();
        ref var src = ref heap<Greetable>(out var Ꮡsrc);
        Ꮡsrc.ValueSlot = wrap(default);
        rv.Set(reflect.ValueOf(Ꮡsrc).Elem());
        fmt.Println("set ok, dest == nil:", Ꮡdest.ValueSlot == nil);
    });

    @try("method-value-call", () => {
        var g = wrap(default);
        var v = reflect.ValueOf(g);
        var mv = v.Method(0);
        fmt.Println("got method value, kind:", mv.Kind());
        mv.Call(new reflectꓸValue[]{reflect.ValueOf("world")}.slice());
        fmt.Println("called ok");
    });
}

} // end main_package

namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using static main_package;

partial class main_package {

public delegate @string Greeter();

} partial class mainᴛGreeter {

public static @string Greet(this Greeter g) {
    if (g == nil) {
        return "nil greeter";
    }
    return g();
}

public static @string Shout(this Greeter g) { return g() + "!"u8; }

} partial class main_package {

partial interface Greetable {
    @string Greet();
    @string Shout();
}

internal static void call(@string label, Func<@string> f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover(); if (r != nil) {
                fmt.Println(label, "panic:", r);
            }
        }, ref ᒐ);
        fmt.Println(label, "=", f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void main() {
    Greeter g = default;
    ref var i = ref heap<Greetable>(out var Ꮡi);

    i = new GreeterᴠGreetable(g);
    fmt.Println("interface nil:", i == nil);

    ref var v = ref heap<reflect.Value>(reflect.ValueOf(Ꮡi).Elem(), out var Ꮡv);
    fmt.Println("kind:", v.Kind(), "elem type:", v.Elem().Type(), "elem nil:", v.Elem().IsNil());
    fmt.Println("NumMethod:", v.NumMethod());

    ref var m = ref heap<reflect.Value>(v.Method(0), out var Ꮡm);
    fmt.Println("method kind:", m.Kind(), "type:", m.Type());
    var mʗ1 = m;
    call("Method(0).Call", () => mʗ1.Call(default)[0].String());
    call("MethodByName(Shout).Call", () => Ꮡv.Value.MethodByName("Shout").Call(default)[0].String());

    var h = new Greeter(() => "hello");
    i = new GreeterᴠGreetable(h);
    v = reflect.ValueOf(Ꮡi).Elem();
    call("non-nil Method(0).Call", () => Ꮡv.Value.Method(0).Call(default)[0].String());
    call("non-nil Method(1).Call", () => Ꮡv.Value.Method(1).Call(default)[0].String());
}

} // end main_package

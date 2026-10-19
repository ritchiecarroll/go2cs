namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

public delegate @string Greeter();

public static @string Greet(this Greeter g) {
    if (g == default!) {
        return "nil greeter";
    }
    return g();
}

public static @string Shout(this Greeter g) {
    return g() + "!"u8;
}

partial interface Greetable {
    @string Greet();
    @string Shout();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object panicˢ = (@string)"panic:"u8;

internal static void call(@string label, Func<@string> f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println(label, panicˢ, r);
                }
            }
        }, ref ᒐ);
        fmt.Println(label, (@string)"="u8, f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object interfaceNilˢ = (@string)"interface nil:"u8;
private static readonly object kindˢ = (@string)"kind:"u8;
private static readonly object elemTypeˢ = (@string)"elem type:"u8;
private static readonly object elemNilˢ = (@string)"elem nil:"u8;
private static readonly object numMethodˢ = (@string)"NumMethod:"u8;
private static readonly object methodKindˢ = (@string)"method kind:"u8;
private static readonly object typeˢ = (@string)"type:"u8;

internal static void Main() {
    Greeter g = default!;
    ref var i = ref heap<Greetable>(out var Ꮡi);

    i = new GreeterᴠGreetable(g);
    fmt.Println(interfaceNilˢ, i == default!);

    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = reflect.ValueOf(Ꮡi).Elem();
    fmt.Println(kindˢ, v.Kind(), elemTypeˢ, v.Elem().Type(), elemNilˢ, v.Elem().IsNil());
    fmt.Println(numMethodˢ, v.NumMethod());

    ref var m = ref heap<reflectꓸValue>(out var Ꮡm);
    m = v.Method(0);
    fmt.Println(methodKindˢ, m.Kind(), typeˢ, m.Type());
    var mʗ1 = m;
    call("Method(0).Call", () => mʗ1.Call(default!)[0].String());
    call("MethodByName(Shout).Call", () => Ꮡv.Value.MethodByName("Shout").Call(default!)[0].String());

    var h = new Greeter(() => "hello");
    i = new GreeterᴠGreetable(h);
    v = reflect.ValueOf(Ꮡi).Elem();
    call("non-nil Method(0).Call", () => Ꮡv.Value.Method(0).Call(default!)[0].String());
    call("non-nil Method(1).Call", () => Ꮡv.Value.Method(1).Call(default!)[0].String());
}

} // end main_package

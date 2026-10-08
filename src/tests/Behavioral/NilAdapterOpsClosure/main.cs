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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object panickedˢ = (@string)"panicked:"u8;

internal static void @try(@string label, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println(label, panickedˢ, r);
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string equalityˢ = "equality"u8;
private static readonly @string mapHashˢ = "map-hash"u8;
private static readonly object storedOkˢ = (@string)"stored ok"u8;
private static readonly @string reflectSetˢ = "reflect-set"u8;
private static readonly object setOkDestNilˢ = (@string)"set ok, dest == nil:"u8;
private static readonly @string methodValueCallˢ = "method-value-call"u8;
private static readonly object gotMethodValueKindˢ = (@string)"got method value, kind:"u8;
private static readonly object worldˢ = (@string)"world"u8;
private static readonly object calledOkˢ = (@string)"called ok"u8;

internal static void Main() {
    @try(equalityˢ, () => {
        var (a, b) = (wrap(default!), wrap(default!));
        fmt.Println((@string)"a == b:"u8, AreEqual(a, b));
    });
    @try(mapHashˢ, () => {
        var m = new map<Greetable, nint>{};
        m[wrap(default!)] = 1;
        fmt.Println(storedOkˢ);
    });
    @try(reflectSetˢ, () => {
        ref var dest = ref heap<Greetable>(out var Ꮡdest);
        var rv = reflect.ValueOf(Ꮡdest).Elem();
        ref var src = ref heap<Greetable>(out var Ꮡsrc);
        Ꮡsrc.ValueSlot = wrap(default!);
        rv.Set(reflect.ValueOf(Ꮡsrc).Elem());
        fmt.Println(setOkDestNilˢ, Ꮡdest.ValueSlot == default!);
    });
    @try(methodValueCallˢ, () => {
        var g = wrap(default!);
        var v = reflect.ValueOf(g);
        var mv = v.Method(0);
        fmt.Println(gotMethodValueKindˢ, mv.Kind());
        mv.Call(new reflectꓸValue[]{reflect.ValueOf(worldˢ)}.slice());
        fmt.Println(calledOkˢ);
    });
}

} // end main_package

namespace go;

using fmt = fmt_package;

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
private static readonly object helloˢ = (@string)"hello"u8;
private static readonly @string worldˢ = "world"u8;

internal static void Main() {
    var nilGreetable = wrap(default!);
    var (nilGreeter, nilOk) = nilGreetable._<Greeter>(ᐧ);
    fmt.Println(nilOk, nilGreeter == default!);
    var realGreetable = wrap((@string name) => {
        fmt.Println(helloˢ, name);
    });
    var (realGreeter, realOk) = realGreetable._<Greeter>(ᐧ);
    fmt.Println(realOk, realGreeter == default!);
    realGreeter.Greet(worldˢ);
}

} // end main_package

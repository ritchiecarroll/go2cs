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
    return new GreeterᴠGreetable(builtin.NilSafeDelegateConversion<Greeter, Action<@string>>(handler));
}

internal static void main() {
    var nilGreetable = wrap(default);
    var (nilGreeter, nilOk) = nilGreetable._ᶠ<Greeter>(ᐧ);
    fmt.Println(nilOk, nilGreeter == nil);

    var realGreetable = wrap([GoFunc("main.func1")] (@string name) => {
        fmt.Println("hello", name);
    });
    var (realGreeter, realOk) = realGreetable._ᶠ<Greeter>(ᐧ);
    fmt.Println(realOk, realGreeter == nil);
    realGreeter.Greet("world");
}

} // end main_package

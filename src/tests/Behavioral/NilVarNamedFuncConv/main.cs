namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

public delegate void Greeter(@string name);

} partial class mainᴛGreeter {

public static void Greet(this Greeter g, @string name) {
    g(name);
}

} partial class main_package {

partial interface Greetable {
    void Greet(@string name);
}

internal static Greetable wrap(Action<@string> handler) {
    return new GreeterᴠGreetable(NilSafeDelegateConversion<Greeter, Action<@string>>(handler));
}

internal static void main() {
    var nilGreetable = wrap(default);
    var (nilGreeter, nilOk) = nilGreetable._<Greeter>(ᐧ);
    fmt.Println(nilOk, nilGreeter == nil);

    var realGreetable = wrap((@string name) => {
        fmt.Println("hello", name);
    });
    var (realGreeter, realOk) = realGreetable._<Greeter>(ᐧ);
    fmt.Println(realOk, realGreeter == nil);
    realGreeter.Greet("world");
}

} // end main_package

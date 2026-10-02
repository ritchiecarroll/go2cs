namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct counter {
    internal nint n;
}

[GoRecv] internal static nint Bump(this ref counter c) {
    c.n++;
    return c.n;
}

[GoType] partial interface greeter {
    @string Greet();
}

[GoType] partial struct hello {
    internal @string who;
}

internal static @string Greet(this hello h) {
    return "hello "u8 + h.who;
}

[GoType] partial struct mixed {
    internal partial ref ж<counter> counter { get; }
    [GoEmbedded] internal greeter greeter;
}

[GoType] partial interface greetBumper {
    @string Greet();
    nint Bump();
}

[GoType] partial struct holder {
    [GoEmbedded] internal greeter greeter;
    internal @string tag;
}

[GoType] partial struct outer {
    internal partial ref holder holder { get; }
    internal nint extra;
}

[GoRecv] internal static nint Extra(this ref outer o) {
    return o.extra;
}

[GoType] partial interface greetExtra {
    @string Greet();
    nint Extra();
}

internal static void Main() {
    var c = Ꮡ(new counter(nil));
    var m = Ꮡ(new mixed(c, new hello("world"u8)));
    greetBumper gb = new mixedжgreetBumper(m);
    fmt.Println(gb.Greet(), gb.Bump(), gb.Bump());
    fmt.Println((~c).n);
    var o = Ꮡ(new outer(new holder(new hello("deep"u8), "t"u8), 7));
    greetExtra ge = new outerжgreetExtra(o);
    fmt.Println(ge.Greet(), ge.Extra(), (~o).tag);
    o.Value.greeter = new hello("replaced"u8);
    fmt.Println(ge.Greet());
}

} // end main_package

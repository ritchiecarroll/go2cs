namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Logger {
    internal @string name;
}

public static @string Handler(this Logger l) {
    return "logger:"u8 + l.name;
}

[GoType] partial interface ΔHandler {
    bool Enabled(nint level);
    @string Handle(@string msg);
    @string WithName(@string name);
}

[GoType] partial struct baseHandler {
    internal nint level;
}

internal static bool Enabled(this baseHandler b, nint level) {
    return level >= b.level;
}

internal static @string Handle(this baseHandler b, @string msg) {
    return "base:"u8 + msg;
}

internal static @string WithName(this baseHandler b, @string name) {
    return "base-name:"u8 + name;
}

[GoType] partial struct wrapper {
    [GoEmbedded] public ΔHandler Handler;
    internal @string prefix;
}

internal static @string Handle(this wrapper w, @string msg) {
    return w.prefix + w.Handler.Handle(msg);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string msgˢ = "msg"u8;

internal static void Main() {
    var l = new Logger(name: "test"u8);
    fmt.Println(l.Handler());
    ref var @base = ref heap<baseHandler>(out var Ꮡbase);
    @base = new baseHandler(level: 2);
    ΔHandler hv = new wrapper(Handler: @base, prefix: "wrap:"u8);
    fmt.Println(hv.Enabled(1), hv.Enabled(3));
    fmt.Println(hv.Handle(msgˢ));
    fmt.Println(hv.WithName("n"u8));
    var hp = Ꮡ(new wrapper(Handler: @base, prefix: "ptr:"u8));
    ΔHandler hpi = new wrapperжΔHandler(hp);
    fmt.Println(hpi.Enabled(2));
    fmt.Println(hpi.Handle(msgˢ));
    fmt.Println(hpi.WithName("p"u8));
}

} // end main_package

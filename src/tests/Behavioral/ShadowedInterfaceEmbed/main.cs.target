namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct Logger {
    internal @string name;
}

} partial class mainᴛLogger {

public static @string Handler(this Logger l) { return "logger:"u8 + l.name; }

} partial class main_package {

partial interface Handler {
    bool Enabled(nint level);
    @string Handle(@string msg);
    @string WithName(@string name);
}

partial struct baseHandler {
    internal nint level;
}

} partial class mainᴛbaseHandler {

internal static bool Enabled(this baseHandler b, nint level) { return level >= b.level; }
internal static @string Handle(this baseHandler b, @string msg) { return "base:"u8 + msg; }
internal static @string WithName(this baseHandler b, @string name) { return "base-name:"u8 + name; }

} partial class main_package {

partial struct wrapper {
    /*embed*/ public Handler Handler;
    internal @string prefix;
}

} partial class mainᴛwrapper {

internal static @string Handle(this wrapper w, @string msg) { return w.prefix + w.Handler.Handle(msg); }

} partial class main_package {

internal static void main() {
    var l = new Logger(name: "test");
    fmt.Println(l.Handler());

    ref var @base = ref heap<baseHandler>(new baseHandler(level: 2), out var Ꮡbase);

    Handler hv = new wrapper(Handler: @base, prefix: "wrap:");
    fmt.Println(hv.Enabled(1), hv.Enabled(3));
    fmt.Println(hv.Handle("msg"));
    fmt.Println(hv.WithName("n"));

    var hp = Ꮡ(new wrapper(Handler: @base, prefix: "ptr:"));
    Handler hpi = new wrapperжHandler(hp);
    fmt.Println(hpi.Enabled(2));
    fmt.Println(hpi.Handle("msg"));
    fmt.Println(hpi.WithName("p"));
}

} // end main_package

namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using strings = strings_package;
using time = time_package;
using Δio = io_package;

partial class main_package {

internal static channel<bool> ready = new channel<bool>(1);

internal static partial void show(@string tag) {
    var pcs = new slice<uintptr>(16);
    nint n = runtime.Callers(2, pcs);
    var frames = runtime.CallersFrames(pcs.slice(0, n));

    ref var b = ref heap(new strings.Builder(), out var Ꮡb);

    for (;;) {
        var (f, more) = frames.Next();

        if (strings.HasPrefix(f.Function, "main.")) {
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), " %s:%d"u8, f.Function, f.Line);
        } else {
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), " %s"u8, f.Function);
        }

        if (!more) {
            break;
        }
    }

    fmt.Println(tag + b.String());
}

internal static nint wait(@string tag, channel<nint> c) {
    ready.ᐸꟷ(true);
    nint v = ᐸꟷ(c);
    show(tag);
    return v;
}

partial struct T {
    internal nint x;
}

partial struct U {
    internal nint y;
}

internal static partial nint b(this ref T t, channel<nint> c) {
    nint v = wait("T after", c);
    t.x = v;
    return v;
}

internal static nint b(this ref U u, channel<nint> c) {
    u.y = 1;
    nint v = wait("U after", c);
    u.y += v;
    return u.y;
}

internal static partial void runT(channel<nint> c, channel<nint> d) {
    var t = Ꮡ(new T());
    d.ᐸꟷ(t.b(c));
}

internal static partial void runU(channel<nint> c, channel<nint> d) {
    var u = Ꮡ(new U());
    d.ᐸꟷ(u.b(c));
}

internal static partial void main() {
    var c = new channel<nint>(0);
    var d = new channel<nint>(0);

    goǃ(runT, c, d);

    ᐸꟷ(ready);
    time.Sleep(20 * time.Millisecond);
    c.ᐸꟷ(1);
    fmt.Println("T", ᐸꟷ(d));

    goǃ(runU, c, d);

    ᐸꟷ(ready);
    time.Sleep(20 * time.Millisecond);
    c.ᐸꟷ(2);
    fmt.Println("U", ᐸꟷ(d));
}

} // end main_package

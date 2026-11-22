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

internal static nint leaf(channel<nint> c) {
    ready.ᐸꟷ(true);
    nint v = ᐸꟷ(c);
    show("after");
    return v;
}

internal static nint add(nint a, nint b) {
    return a + b;
}

internal static partial nint mid(channel<nint> c) {
    nint v = add(1,
        leaf(c));
    return v;
}

internal static partial void root(channel<nint> c, channel<nint> d) {
    d.ᐸꟷ(mid(c));
}

internal static partial void main() {
    var c = new channel<nint>(0);
    var d = new channel<nint>(0);

    goǃ(root, c, d);

    ᐸꟷ(ready);
    time.Sleep(20 * time.Millisecond);
    c.ᐸꟷ(1);
    fmt.Println("multi", ᐸꟷ(d));
}

} // end main_package

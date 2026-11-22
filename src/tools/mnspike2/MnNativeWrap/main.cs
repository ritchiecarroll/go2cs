namespace go;

using fmt = fmt_package;

// M:N PROTOTYPE (A1): the red for the never-twinned rule on native wrappers. A hand project in the converted shape,
// because a Go oracle program cannot call a hand-owned C# wrapper. The wrappers live in ffi_impl.cs.
//
// caller parks on its receive, so the twin generator writes its twin. From that twin it calls a cgo-style wrapper
// that pins Go memory with `fixed` across a native call, a second wrapper that pins and calls nothing that can park,
// and a LibraryImport extern. None of the three may get a twin: a `fixed` scope cannot span a park. A2 adds a fourth,
// peek: an `unsafe` block with no `fixed` around a call that can park, which C# cannot await in.
partial class main_package {

internal static channel<nint> side = new channel<nint>(4);

internal static void caller(channel<nint> req, channel<nint> resp) {
    nint seed = ᐸꟷ(req);
    var buf = new slice<byte>(8);
    nint sum = fill(buf, seed);
    nint n = count(buf);
    nint pid = processId();
    nint echo = peek(seed);

    resp.ᐸꟷ(sum);
    resp.ᐸꟷ(n);
    resp.ᐸꟷ(echo);

    if (pid > 0) {
        resp.ᐸꟷ(1);
    } else {
        resp.ᐸꟷ(0);
    }
}

internal static void main() {
    var req = new channel<nint>(0);
    var resp = new channel<nint>(0);

    goǃ(caller, req, resp);
    req.ᐸꟷ(3);

    fmt.Println("sum:", ᐸꟷ(resp));
    fmt.Println("count:", ᐸꟷ(resp));
    fmt.Println("unsafe block:", ᐸꟷ(resp));
    fmt.Println("extern:", ᐸꟷ(resp));
    fmt.Println("side:", ᐸꟷ(side));
    fmt.Println("side:", ᐸꟷ(side));
}

} // end main_package

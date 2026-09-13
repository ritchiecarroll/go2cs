namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct root {
    internal nint fd;
    internal channel<nint> done;
}

[GoRecv] internal static void release(this ref root r) {
    r.fd = -r.fd;
    if (r.done != default!) {
        r.done.ᐸꟷ(r.fd);
    }
}

[GoType] partial struct Root {
    internal ж<root> root;
}

internal static nint /*seen*/ deferChain<T>(ref Root r, T mark) {
    nint seen = default!;
    GoFrame ᒐ = default;
    try {
        defer(r.root.release, ref ᒐ);
        seen = r.root.Value.fd;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return seen;
}

internal static nint goChain(ref Root r) {
    goǃ(r.root.release);
    return ᐸꟷ((~r.root).done);
}

internal static nint immediateChain(ref Root r) {
    r.root.release();
    return (~r.root).fd;
}

internal static nint /*before*/ directReceiver(ж<root> Ꮡp) {
    nint before = default!;
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        defer(Ꮡp.release, ref ᒐ);
        before = p.fd;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return before;
}

internal static nint localChain() {
    GoFrame ᒐ = default;
    try {
        var r = Ꮡ(new Root(root: Ꮡ(new root(fd: 7))));
        var rʗ1 = r;
        defer((~rʗ1).root.release, ref ᒐ);
        return (~(~r).root).fd;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    var a = Ꮡ(new Root(root: Ꮡ(new root(fd: 3))));
    fmt.Println(deferChain<nint>(ref (a).DerefOrNull(), 0), (~(~a).root).fd);
    var b = Ꮡ(new Root(root: Ꮡ(new root(fd: 5, done: new channel<nint>(0)))));
    fmt.Println(goChain(ref (b).DerefOrNull()), (~(~b).root).fd);
    var c = Ꮡ(new Root(root: Ꮡ(new root(fd: 11))));
    fmt.Println(immediateChain(ref (c).DerefOrNull()));
    var d = Ꮡ(new root(fd: 13));
    fmt.Println(directReceiver(d), (~d).fd);
    fmt.Println(localChain());
}

} // end main_package

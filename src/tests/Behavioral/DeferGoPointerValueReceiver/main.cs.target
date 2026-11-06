namespace go;

using fmt = fmt_package;
using Δsync = sync_package;

partial class main_package {

partial struct path {
    internal @string name;
}

internal static void Done(this path p) {
    fmt.Println("done", p.name);
}

internal static void Pop(this path p, nint k) {
    fmt.Println("pop", k, p.name);
}

internal static void Run(this path p, ж<Δsync.WaitGroup> wg) {
    fmt.Println("run", p.name);
    wg.Done();
}

internal static void pointerLocal() {
    GoFrame ᒐ = default;
    try {
        var p = Ꮡ(new path("pointee"));
        var pʗ1 = p;
        defer(ᴛ0 => ᴛ0.Done(), (~pʗ1), ref ᒐ);
        p.Value = new path("replaced");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void pointerLocalArg(nint k) {
    GoFrame ᒐ = default;
    try {
        var p = Ꮡ(new path("pointee"));
        var pʗ1 = p;
        defer((ᴛ0, ᴛ1) => ᴛ0.Pop(ᴛ1), (~pʗ1), k, ref ᒐ);
        p.Value = new path("replaced");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void pointerParam(ж<path> Ꮡp) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        defer(ᴛ0 => ᴛ0.Done(), Ꮡp.Value, ref ᒐ);
        p = new path("replaced");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void pointerParamArg(ж<path> Ꮡp, nint k) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        defer((ᴛ0, ᴛ1) => ᴛ0.Pop(ᴛ1), Ꮡp.Value, k, ref ᒐ);
        p = new path("replaced");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void goLocal() {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);
    var p = Ꮡ(new path("pointee"));
    var pʗ1 = p;
    goǃ((ᴛ0, ᴛ1) => ᴛ0.Run(ᴛ1), (~pʗ1), Ꮡwg);
    p.Value = new path("replaced");
    Ꮡwg.Wait();
}

internal static void nilDefer() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println("recovered:", recover() != nil);
        }, ref ᒐ);
        ж<path> p = default;
        var pʗ1 = p;
        defer(ᴛ0 => ᴛ0.Done(), (~pʗ1), ref ᒐ);
        fmt.Println("after the defer statement");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    pointerLocal();
    pointerLocalArg(1);
    pointerParam(Ꮡ(new path("pointee")));
    pointerParamArg(Ꮡ(new path("pointee")), 2);
    goLocal();
    nilDefer();
}

} // end main_package

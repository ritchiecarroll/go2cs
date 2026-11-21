namespace go;

using fmt = fmt_package;
using Δsync = sync_package;

partial class main_package {

partial struct scope {
    internal nint id;
    internal slice<@string> names;
}

partial struct compiler {
    internal slice<ж<scope>> scopes;
    internal nint count;
}

internal static Action lazy(this ref compiler c, Func<nint> f) {
    return () => {
        fmt.Println("lazy:", f());
    };
}

internal static ж<scope> newScope(this ref compiler c) {
    c.count++;
    return Ꮡ(new scope(id: c.count));
}

internal static void define(this ж<compiler> Ꮡc, bool Δbuiltin, ж<scope> outer) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        ж<scope> s = default;
        if (Δbuiltin) {
            s = outer;
        } else {
            s = c.scopes[len(c.scopes) - 1];
        }
        defer(Ꮡc.Value.lazy(() => Ꮡc.Value.count), ref ᒐ);
        fmt.Println("defining in scope", s.id);
        s = c.newScope();
        var sʗ1 = s;
        defer(Ꮡc.Value.lazy(() => sʗ1.id * 10), ref ᒐ);
        s.names = append(s.names, (@string)"x");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static Action later(Func<@string> f) {
    return () => {
        fmt.Println("later:", f());
    };
}

internal partial struct twoCaptures_point /*dyn*/ {
    internal nint x, y;
}

internal static void twoCaptures() {
    GoFrame ᒐ = default;
    try {
        @string label = "total";
        ref var point = ref heap<twoCaptures_point>(new twoCaptures_point(3, 4), out var Ꮡpoint);
        var pointʗ1 = point;
        defer(later(() => fmt.Sprint(label, "=", pointʗ1.x + pointʗ1.y)), ref ᒐ);
        fmt.Println("two captures");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static Action spawn(Func<@string> f, ж<Δsync.WaitGroup> wg) {
    return () => {
        GoFrame ᒐ = default;
        try {
            defer(wg.Done, ref ᒐ);
            fmt.Println("go:", f());
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
}

internal static partial void goCapture(nint n) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);
    var labels = slice<@string>(["n", fmt.Sprint(n)]);
    var labelsʗ1 = labels;
    goǃ(spawn(() => labelsʗ1[0] + "=" + labelsʗ1[1], Ꮡwg));
    Ꮡwg.Wait();
}

internal static void main() {
    var c = Ꮡ(new compiler());
    c.scopes = append(c.scopes, c.newScope());
    c.define(false, nil);
    c.define(true, Ꮡ(new scope(id: 7)));
    twoCaptures();
    goCapture(3);
}

} // end main_package

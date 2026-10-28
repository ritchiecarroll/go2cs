namespace go;

using fmt = fmt_package;
using strings = strings_package;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static void Inc(this ref counter c) {
    c.n++;
}

internal static bool isNil(ж<counter> c) {
    return c == nil;
}

internal static void passOn(ж<counter> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.Inc();
    bump(ref (Ꮡc).DerefOrNull());
}

internal static void bump(ref counter c) {
    c.n++;
}

internal static nint shadow(ж<counter> c) {
    if (c != nil) {
        nint cΔ1 = 5;
        return cΔ1;
    }
    return -1;
}

internal static Func<bool> capture(ж<counter> c) {
    return () => c == nil;
}

internal static void write(ж<strings.Builder> b, @string s) {
    b.WriteString(s);
}

internal static void deferred(ж<counter> c) {
    GoFrame ᒐ = default;
    try {
        defer(c.Inc, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static nint both(ref counter a, ж<counter> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.Inc();
    return a.n;
}

internal static void Main() {
    ref var c = ref heap(new counter(), out var Ꮡc);
    fmt.Println(isNil(Ꮡc), isNil(nil));
    passOn(Ꮡc);
    fmt.Println(c.n);
    fmt.Println(shadow(Ꮡc), shadow(nil));
    fmt.Println(capture(Ꮡc)(), capture(nil)());
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    write(Ꮡsb, "go");
    write(Ꮡsb, "2cs");
    fmt.Println(sb.String());
    deferred(Ꮡc);
    fmt.Println(c.n);
    var d = new counter(n: 10);
    fmt.Println(both(ref d, Ꮡc), c.n);
}

} // end main_package

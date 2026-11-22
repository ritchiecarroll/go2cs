namespace go.LiteralFrameNames;

using runtime = runtime_package;

partial class framelib_package {

partial struct counter {
    internal nint n;
}

partial struct gauge {
    internal nint v;
}

internal static partial @string Name⁔(/*this*/ ж<counter> Ꮡc) {
    @string f() {
        Ꮡc.Value.n++;
        return here();
    }
    return f();
}

internal static partial @string Name⁔(/*this*/ gauge g) {
    @string f() {
        g.v++;
        return here();
    }
    return f();
}

public static @string CounterName() { return (Ꮡ(new counter())).Name(); }

public static @string GaugeName() { return new gauge().Name(); }

internal static partial @string here() {
    var (pc, _, _, _) = runtime.Caller(1);
    return runtime.FuncForPC(pc).Name();
}

} // end framelib_package

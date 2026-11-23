namespace go.go;

partial class anonifaceembed_package {

partial interface Option {
    nint Apply();
}

partial interface Other {
    @string Name();
}

partial struct both {
    internal nint n;
}

internal static nint Apply⁔(/*this*/ ref both b) { return b.n; }
internal static @string Name⁔(/*this*/ ref both b) { return "both"; }
internal static nint Extra⁔(/*this*/ ref both b) { return b.n * 10; }

public partial interface With_R0 /*dyn*/ :
    Option,
    Other
{
}

public static With_R0 With(nint n) {
    return new bothжWith_R0(Ꮡ(new both(n)));
}

public partial interface WithMixed_R0 /*dyn*/ :
    Option
{
    nint Extra();
}

public static WithMixed_R0 WithMixed(nint n) {
    return new bothжWithMixed_R0(Ꮡ(new both(n)));
}

public static @string Take(With_R0 v) {
    return v.Name();
}

partial struct runner {
    internal nint n;
}

internal static nint run⁔(/*this*/ runner r) { return r.n; }

public partial interface Runner_R0 /*dyn*/ {
    nint run();
}

public static Runner_R0 Runner(nint n) { return new runner(n); }

} // end anonifaceembed_package

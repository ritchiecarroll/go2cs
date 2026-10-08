namespace go.ItabLateRegistration;

using fmt = fmt_package;

partial class lib_package {

partial interface Named {
    @string Name();
}

partial interface Sized {
    nint Size();
}

partial struct Circle {
    public nint R;
}

public static @string Name(this Circle c) {
    return fmt.Sprintf("circle(%d)"u8, c.R);
}

public static nint Size(this Circle c) {
    return c.R * 2;
}

partial struct Square {
    public nint S;
}

public static @string Name(this Square s) {
    return fmt.Sprintf("square(%d)"u8, s.S);
}

public static nint Size(this Square s) {
    return s.S * 4;
}

partial struct Blank {
    public nint N;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string blankˢ = "blank"u8;

public static @string Tag(this Blank b) {
    return blankˢ;
}

public static (@string, bool) TryName(any v) {
    var (n, ok) = v._<Named>(ᐧ);
    if (!ok) {
        return ("-", false);
    }
    return (n.Name(), true);
}

public static (nint, bool) TrySize(any v) {
    var (s, ok) = v._<Sized>(ᐧ);
    if (!ok) {
        return (-1, false);
    }
    return (s.Size(), true);
}

} // end lib_package

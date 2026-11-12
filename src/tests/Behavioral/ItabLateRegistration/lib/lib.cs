namespace go.ItabLateRegistration;

using fmt = fmt_package;
using static lib_package;

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

} partial class libᴛCircle {

public static @string Name(this Circle c) { return fmt.Sprintf("circle(%d)"u8, c.R); }

public static nint Size(this Circle c) { return c.R * 2; }

} partial class lib_package {

partial struct Square {
    public nint S;
}

} partial class libᴛSquare {

public static @string Name(this Square s) { return fmt.Sprintf("square(%d)"u8, s.S); }

public static nint Size(this Square s) { return s.S * 4; }

} partial class lib_package {

partial struct Blank {
    public nint N;
}

} partial class libᴛBlank {

public static @string Tag(this Blank b) { return "blank"; }

} partial class lib_package {

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

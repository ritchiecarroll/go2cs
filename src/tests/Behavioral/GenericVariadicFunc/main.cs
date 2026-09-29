namespace go;

using fmt = fmt_package;

partial class main_package {

public static T First<T>(params Span<T> valsʗp) {
    var vals = valsʗp.sslice();

    return vals[0];
}

public static nint Count<T>(params Span<T> valsʗp) {
    var vals = valsʗp.sslice();

    return len(vals);
}

public static nint DeferredCount<T>(params Span<T> valsʗp) {
    GoFrame ᒐ = default;
    try {
        var vals = valsʗp.sslice();

        defer(() => {
        }, ref ᒐ);
        return len(vals);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

public static T Or<T>(params Span<T> valsʗp) {
    var vals = valsʗp.sslice();

    T zero = GoZero<T>();
    foreach (var (_, v) in vals) {
        if (!AreEqual(v, zero)) {
            return v;
        }
    }
    return zero;
}

internal static void Main() {
    fmt.Println(First(10, 20, 30));
    fmt.Println(First(1.5D, 2.5D));
    fmt.Println(Count(1, 2, 3, 4));
    fmt.Println(DeferredCount((@string)"x", (@string)"y"));
    @string s1 = "go"u8;
    @string s2 = "2cs"u8;
    fmt.Println(First(s1, s2));
    fmt.Println(First((@string)"A", (@string)"B", (@string)"C"));
}

} // end main_package

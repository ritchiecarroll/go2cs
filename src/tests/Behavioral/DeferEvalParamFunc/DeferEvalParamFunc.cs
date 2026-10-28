namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        defer(ᴛ1 => fmt.Println("Deferred result:", ᴛ1), add(3, 4), ref ᒐ);
        fmt.Println("Doing something else");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static nint add(nint x, nint y) {
    nint result = x + y;
    fmt.Println("Calculate:", result);
    return result;
}

} // end main_package

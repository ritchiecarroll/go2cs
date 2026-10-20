namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        nint count = 0;
        defer(() => {
            fmt.Println("Deferred count (closure):", count);
        }, ref ᒐ);
        count = 10;
        fmt.Println("Count before defer:", count);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package

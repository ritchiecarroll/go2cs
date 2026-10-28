namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    printSquare(5);
}

internal static void printSquare(nint n) {
    GoFrame ᒐ = default;
    try {
        defer(ᴛ1 => fmt.Println("Deferred square:", ᴛ1), n * n, ref ᒐ);
        n++;
        fmt.Println("Immediate n:", n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package

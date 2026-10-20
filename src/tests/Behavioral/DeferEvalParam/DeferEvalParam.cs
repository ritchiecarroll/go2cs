namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    printSquare(5);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferredSquareˢ = (@string)"Deferred square:"u8;

internal static void printSquare(nint n) {
    GoFrame ᒐ = default;
    try {
        defer((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), deferredSquareˢ, n * n, ref ᒐ);
        n++;
        fmt.Println("Immediate n:", n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package

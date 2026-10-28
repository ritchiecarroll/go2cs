namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("Open file");
        defer(() => fmt.Println("Close file"), ref ᒐ);
        fmt.Println("Write data to file");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package

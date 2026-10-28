namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    fmt.Println(fmt.Errorf("plain message"u8));
    fmt.Println(fmt.Errorf("got %v"u8, (nint)42));
    fmt.Println(fmt.Errorf("name %s = %d"u8, "x", (nint)7));
    fmt.Println(fmt.Errorf("%v and %v"u8, true, "y"));

    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != nil) {
                        fmt.Println(fmt.Errorf("recovered: %v"u8, r));
                    }
                }
            }, ref ᒐ);
            throw panic("kaboom");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();

    var @base = fmt.Errorf("base failure"u8);
    var wrapped = fmt.Errorf("while doing X: %w"u8, @base);
    fmt.Println(wrapped);
    fmt.Println(wrapped.Error());
}

} // end main_package

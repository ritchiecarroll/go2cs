namespace go;

using fmt = fmt_package;

partial class main_package {

internal static bool writePanicked;

internal static void tryWrite(map<@string, nint> m) {
    GoFrame ᒐ = default;
    try {
        writePanicked = false;
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    writePanicked = true;
                }
            }
        }, ref ᒐ);
        m["x"] = 1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    map<@string, nint> m = default;

    fmt.Println(m["a"]);

    var (v, ok) = m["a", ꟷ];
    fmt.Println(v, ok);

    fmt.Println(len(m));

    nint count = 0;
    foreach ((_, _) in m) {
        count++;
    }
    fmt.Println(count);

    delete(m, "a"u8);
    fmt.Println("delete ok");

    fmt.Println(m == nil);

    var e = new map<@string, nint>{};
    fmt.Println(e == nil);
    fmt.Println(len(e));

    tryWrite(m);
    fmt.Println(writePanicked);

    tryWrite(new map<@string, nint>{});
    fmt.Println(writePanicked);
}

} // end main_package

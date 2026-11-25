namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct setting {
    internal @string name;
    internal nint count;
}

internal static nint bump(this ref setting s) {
    s.count++;
    return s.count;
}

partial struct Setting {
    internal @string tag;
    /*embed*/ internal ж<setting> setting;
}

internal static ж<setting> lookup(@string name) {
    return Ꮡ(new setting(name: name));
}

public static ж<Setting> New(@string tag) {
    return Ꮡ(new Setting(tag: tag));
}

internal static void main() {
    var s = New("debug");

    fmt.Println(s.setting == nil);

    var sʗ1 = s;
    func([GoFunc("main.func1")] () => {
        GoFrame ᒐ = default;
        try {
            defer([GoFunc("main.func1.1")] () => {
                fmt.Println("recovered:", recover());
            }, ref ᒐ);
            fmt.Println(sʗ1.name);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();

    ж<setting> p = default;
    s.setting = p;
    fmt.Println(s.setting == nil);

    var evalᴛ1 = lookup("gcpacertrace");
    s.setting = evalᴛ1;
    fmt.Println(s.setting == nil);
    var evalᴛ2 = s.bump();
    var evalᴛ3 = s.bump();
    fmt.Println(s.name, evalᴛ2, evalᴛ3);

    var direct = lookup("other");
    s.setting = direct;
    s.count = 10;
    nint before = direct.count;
    nint after = s.bump();
    fmt.Println(before, after, direct.count);
}

} // end main_package

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

internal static void Main() {
    var s = New("debug");

    fmt.Println((~s).setting == nil);

    var sʗ1 = s;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println("recovered:", recover());
            }, ref ᒐ);
            fmt.Println((~sʗ1).name);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();

    ж<setting> p = default!;
    s.Value.setting = p;
    fmt.Println((~s).setting == nil);

    s.Value.setting = lookup("gcpacertrace");
    fmt.Println((~s).setting == nil);
    fmt.Println((~s).name, s.bump(), s.bump());

    var direct = lookup("other");
    s.Value.setting = direct;
    s.Value.count = 10;
    nint before = direct.Value.count;
    nint after = s.bump();
    fmt.Println(before, after, (~direct).count);
}

} // end main_package

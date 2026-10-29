namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct item {
    internal nint n;
}

partial class itemPtr /*ж<item>*/;

internal partial struct topLevel_s /*dyn*/ {
    internal nint i;
}

internal partial class topLevel_sPtr /*ж<topLevel_s>*/;

internal static void topLevel() {
    var ps = Ꮡ(new topLevel_s(1));
    var dps = new topLevel_sPtr(ps);
    fmt.Println("top level:", (dps.Value).i);
}

internal partial struct inClosure_s /*dyn*/ {
    internal nint i;
}

internal partial class inClosure_sPtr /*ж<inClosure_s>*/;

internal static void inClosure() {
    void run() {
        var ps = Ꮡ(new inClosure_s(2));
        var dps = new inClosure_sPtr(ps);
        (dps.Value).i = 3;
        fmt.Println("in closure:", ps.i);
    }

    run();
}

internal partial struct fromNil_s /*dyn*/ {
    internal nint i;
}

internal partial class fromNil_sPtr /*ж<fromNil_s>*/;

internal static void fromNil() {
    fromNil_sPtr dps = ((fromNil_sPtr)nil);
    fmt.Println("from nil:", dps == nil);
}

internal static void Main() {
    topLevel();
    inClosure();
    fromNil();

    var p = new itemPtr(Ꮡ(new item(4)));
    fmt.Println("package level:", (p.Value).n);
}

} // end main_package

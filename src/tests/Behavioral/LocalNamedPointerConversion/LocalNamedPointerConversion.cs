namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct item {
    internal nint n;
}

partial class itemPtr /*ж<item>*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object topLevelˢ = (@string)"top level:"u8;

internal partial struct topLevel_s /*dyn*/ {
    internal nint i;
}

internal partial class topLevel_sPtr /*ж<topLevel_s>*/;

internal static void topLevel() {
    var ps = Ꮡ(new topLevel_s(1));
    var dps = new topLevel_sPtr(ps);
    fmt.Println(topLevelˢ, (dps.Value).i);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object inClosureˢ = (@string)"in closure:"u8;

internal partial struct inClosure_s /*dyn*/ {
    internal nint i;
}

internal partial class inClosure_sPtr /*ж<inClosure_s>*/;

internal static void inClosure() {
    void run() {
        var ps = Ꮡ(new inClosure_s(2));
        var dps = new inClosure_sPtr(ps);
        (dps.Value).i = 3;
        fmt.Println(inClosureˢ, (~ps).i);
    }
    run();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fromNilˢ = (@string)"from nil:"u8;

internal partial struct fromNil_s /*dyn*/ {
    internal nint i;
}

internal partial class fromNil_sPtr /*ж<fromNil_s>*/;

internal static void fromNil() {
    fromNil_sPtr dps = ((fromNil_sPtr)nil);
    fmt.Println(fromNilˢ, dps == nil);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object packageLevelˢ = (@string)"package level:"u8;

internal static void Main() {
    topLevel();
    inClosure();
    fromNil();
    var p = new itemPtr(Ꮡ(new item(4)));
    fmt.Println(packageLevelˢ, (p.Value).n);
}

} // end main_package

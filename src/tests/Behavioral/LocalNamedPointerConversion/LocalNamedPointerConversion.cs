namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct item {
    internal nint n;
}

[GoType("ж<item>")] partial class itemPtr;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object topLevelˢ = (@string)"top level:"u8;

[GoType("dyn")] internal partial struct topLevel_s {
    internal nint i;
}

[GoLocalName("sPtr")] [GoType("ж<topLevel_s>")] internal partial class topLevel_sPtr;

internal static void topLevel() {
    var ps = Ꮡ(new topLevel_s(1));
    var dps = new topLevel_sPtr(ps);
    fmt.Println(topLevelˢ, (dps.Value).i);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object inClosureˢ = (@string)"in closure:"u8;

[GoType("dyn")] internal partial struct inClosure_s {
    internal nint i;
}

[GoLocalName("sPtr")] [GoType("ж<inClosure_s>")] internal partial class inClosure_sPtr;

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

[GoType("dyn")] internal partial struct fromNil_s {
    internal nint i;
}

[GoLocalName("sPtr")] [GoType("ж<fromNil_s>")] internal partial class fromNil_sPtr;

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

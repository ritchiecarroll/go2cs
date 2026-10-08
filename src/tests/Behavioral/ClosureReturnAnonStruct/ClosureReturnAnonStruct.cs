namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string alphaˢ = "alpha"u8;
private static readonly @string betaˢ = "beta"u8;

internal partial struct closureReturningAnonStruct_func_R0 /*dyn*/ {
    internal @string name;
    internal nint size;
}

internal static void closureReturningAnonStruct() {
    closureReturningAnonStruct_func_R0 makeEntry(@string name, nint size) => new closureReturningAnonStruct_func_R0(name, size);
    var e1 = makeEntry(alphaˢ, 10);
    var e2 = makeEntry(betaˢ, 20);
    fmt.Printf("entries: %s=%d %s=%d total=%d\n"u8, e1.name, e1.size, e2.name, e2.size, e1.size + e2.size);
}

internal static void Main() {
    closureReturningAnonStruct();
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

internal partial struct closureReturningAnonStruct_func_R0 /*dyn*/ {
    internal @string name;
    internal nint size;
}

internal static void closureReturningAnonStruct() {
    closureReturningAnonStruct_func_R0 makeEntry(@string name, nint size) => new closureReturningAnonStruct_func_R0(name, size);

    var e1 = makeEntry("alpha", 10);
    var e2 = makeEntry("beta", 20);

    fmt.Printf("entries: %s=%d %s=%d total=%d\n"u8, e1.name, e1.size, e2.name, e2.size, e1.size + e2.size);
}

internal static void Main() {
    closureReturningAnonStruct();
}

} // end main_package

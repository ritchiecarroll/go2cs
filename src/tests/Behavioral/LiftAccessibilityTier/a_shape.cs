namespace go;

using fmt = fmt_package;

partial class main_package {

internal partial struct RegisterInternalShape_a /*dyn*/ {
    public nint X;
}

public static void RegisterInternalShape() {
    fmt.Println(new RegisterInternalShape_a(X: 10));
}

} // end main_package

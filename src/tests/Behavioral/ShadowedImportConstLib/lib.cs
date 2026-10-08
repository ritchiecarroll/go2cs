namespace go;

partial class ShadowedImportConstLib_package {

partial struct Span /*num:int64*/;

public static Span ΔPeak => 60;

partial struct Meter {
    public nint Level;
}

public static nint Peak(this Meter m) {
    return m.Level + 1;
}

} // end ShadowedImportConstLib_package

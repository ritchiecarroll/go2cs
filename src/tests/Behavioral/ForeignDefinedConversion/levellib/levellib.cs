namespace go.ForeignDefinedConversion;

partial class levellib_package {

[GoType("num:uint32")] partial struct Level;

public static uint32 Double(this Level l) {
    return (uint32)l * 2;
}

} // end levellib_package

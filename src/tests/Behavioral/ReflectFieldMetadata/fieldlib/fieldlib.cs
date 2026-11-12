namespace go.ReflectFieldMetadata;

using static fieldlib_package;

partial class fieldlib_package {

partial struct Outer {
    public nint Exported;
    internal nint unexported;
}

} partial class fieldlibᴛOuter {

public static nint Sum(this Outer o) { return o.Exported + o.unexported; }

} // end fieldlibᴛOuter

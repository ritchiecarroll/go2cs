namespace go.ReflectFieldMetadata;

partial class fieldlib_package {

partial struct Outer {
    public nint Exported;
    internal nint unexported;
}

public static nint Sum(this Outer o) {
    return o.Exported + o.unexported;
}

} // end fieldlib_package

namespace go.MethodHolderAlias.geom;

partial class part_package {

partial struct Side {
    public nint Len;
}

internal static nint Double⁔(/*this*/ Side s) { return Double(s.Len); }

public static nint Double(nint n) { return n * 2; }

} // end part_package

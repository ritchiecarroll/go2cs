namespace go.MethodHolderAlias.geom;

using part = part_package;
using static part_package;

partial class part_package {

partial struct Side {
    public nint Len;
}

} partial class partᴛSide {

public static nint Double(this Side s) { return part.Double(s.Len); }

} partial class part_package {

public static nint Double(nint n) { return n * 2; }

} // end part_package

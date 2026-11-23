namespace go.MethodHolderAlias;

using part = go.MethodHolderAlias.geom.part_package;
using go.MethodHolderAlias.geom;

partial class geom_package {

public static UntypedInt Unit => 3;

partial struct Shape {
    public part.Side W, H;
}

internal static nint Area⁔(/*this*/ Shape s) { return Area(s.W.Len, s.H.Len); }

internal static nint Unit⁔(/*this*/ ref Shape s) { return (nint)Unit + s.W.Double(); }

public static nint Area(nint w, nint h) { return w * h; }

public static nint 量 = 1000;

partial struct 尺 {
    internal nint n;
}

} // end geom_package

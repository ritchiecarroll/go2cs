namespace go.MethodHolderAlias;

using part = go.MethodHolderAlias.geom.part_package;
using go.MethodHolderAlias.geom;
using Δgeom = geom_package;
using static geom_package;

partial class geom_package {

public static UntypedInt Unit => 3;

partial struct Shape {
    public part.Side W, H;
}

} partial class geomᴛShape {

public static nint Area(this Shape s) { return Δgeom.Area(s.W.Len, s.H.Len); }

public static nint Unit(this ref Shape s) { return (nint)Δgeom.Unit + s.W.Double(); }

} partial class geom_package {

public static nint Area(nint w, nint h) { return w * h; }

} // end geom_package

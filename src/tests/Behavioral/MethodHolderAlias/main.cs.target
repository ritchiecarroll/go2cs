namespace go;

using fmt = fmt_package;
using static MethodHolderAlias.geom_package;
using part = MethodHolderAlias.geom.part_package;
using html = MethodHolderAlias.html.plate_package;
using tone = MethodHolderAlias.tone_package;
using MethodHolderAlias;
using MethodHolderAlias.geom;
using MethodHolderAlias.html;
using geom = MethodHolderAlias.geom_package;
using plate = MethodHolderAlias.html.plate_package;
using static main_package;

partial class main_package {

partial struct field {
    internal geom.Shape s;
}

} partial class mainᴛfield {

internal static nint Area(this field f) { return MethodHolderAlias.geom_package.Area(f.s.W.Len, f.s.H.Len) + (nint)Unit; }

} partial class main_package {

internal static void main() {
    ref var s = ref heap<geom.Shape>(new Shape(W: new part.Side(Len: 4), H: new part.Side(Len: 5)), out var Ꮡs);
    fmt.Println("child namespace", s.Area(), s.Unit(), Area(2, 3), (nint)(Unit));

    var area = ((Func<geom.Shape, nint>)(global::go.MethodHolderAlias.geomᴛShape.Area));
    var unit = ((Func<ж<geom.Shape>, nint>)(global::go.MethodHolderAlias.geomᴛShape.Unit));
    fmt.Println("expressions", area(s), unit(Ꮡs), ((Func<part.Side, nint>)(global::go.MethodHolderAlias.geom.partᴛSide.Double))(new part.Side(Len: 7)));

    fmt.Println("dot import", new field(s).Area());

    var (pitch, @base) = tone.Play(2);
    fmt.Println("type named like its package", pitch, @base, tone.Pitch(1), (nint)(tone.Base));

    fmt.Println("import of the same name", html.New("page").Render(), html.Render("x"));
}

} // end main_package

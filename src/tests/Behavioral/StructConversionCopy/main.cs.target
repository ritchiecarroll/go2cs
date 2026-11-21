namespace go;

using fmt = fmt_package;
using lib = StructConversionCopy.lib_package;
using other = StructConversionCopy.other_package;
using StructConversionCopy;

partial class main_package {

partial struct chord {
    public @string Name;
    public array<nint> Notes = new(2);
    public lib.Inner In;
}

partial struct chord2 {
    public @string Name;
    public array<nint> Notes = new(2);
    public lib.Inner In;
}

partial struct wrapped /*chord*/;

partial struct notes /*[2]nint*/;

partial struct holder {
    public lib.Chord L;
}

internal static lib.Chord fresh() {
    return new lib.Chord(Name: "f", Notes: array<nint>([1, 2]), In: new lib.Inner(V: array<nint>([3, 4])));
}

internal partial struct main_x /*dyn*/ {
    public @string Name;
    public array<nint> Notes = new(2);
    public lib.Inner In;
}

internal static void main() {
    ref var lc = ref heap<lib.Chord>(fresh(), out var Ꮡlc);
    var c = ((chord)lc).ΔClone();
    (c.Notes[0], c.In.V[1]) = (9, 8);
    fmt.Println("local from foreign:", lc.Notes, lc.In.V, c.Notes, c.In.V);

    var back = ((lib.Chord)c).ΔClone();
    (back.Notes[1], back.In.V[0]) = (7, 6);
    fmt.Println("foreign from local:", c.Notes, c.In.V, back.Notes, back.In.V);

    var c2 = ((chord2)c).ΔClone();
    (c2.Notes[0], c2.In.V[0]) = (5, 5);
    fmt.Println("local from local:", c.Notes, c.In.V, c2.Notes, c2.In.V);

    var w = (wrapped)c;
    (w.Notes[1], w.In.V[1]) = (17, 17);
    var u = (chord)w;
    u.Notes[0] = 18;
    fmt.Println("wrapper:", c.Notes, c.In.V, w.Notes, w.In.V, u.Notes);

    var oc = otherꓸChord(lc).ΔClone();
    (oc.Notes[0], oc.In.V[1]) = (4, 4);
    fmt.Println("foreign from foreign:", lc.Notes, lc.In.V, oc.Notes, oc.In.V);

    var x = ((main_x)c).ΔClone();
    (x.Notes[0], x.In.V[0]) = (3, 3);
    fmt.Println("unnamed from named:", c.Notes, c.In.V, x.Notes, x.In.V);

    var h = new holder(L: fresh());
    var hc = ((chord)h.L).ΔClone();
    hc.Notes[0] = 11;
    fmt.Println("field:", h.L.Notes, hc.Notes);

    var s = slice<lib.Chord>([fresh()]);
    var sc = ((chord)s[0]).ΔClone();
    sc.In.V[0] = 12;
    fmt.Println("element:", s[0].In.V, sc.In.V);

    var p = Ꮡlc;
    var pc = ((chord)(p.Value)).ΔClone();
    pc.Notes[1] = 13;
    fmt.Println("pointer:", p.Notes, pc.Notes);

    var raw = array<nint>([1, 2]);
    var n = ((notes)raw).Clone();
    n[0] = 14;
    fmt.Println("array:", raw, n);

    var same = ((chord)c).ΔClone();
    same.Notes[1] = 15;
    fmt.Println("same type:", c.Notes, same.Notes);

    touch((chord)lc);
    fmt.Println("argument:", lc.Notes);
}

internal static void touch(chord c) {
    c = c.ΔClone();

    c.Notes[0] = 16;
}

} // end main_package

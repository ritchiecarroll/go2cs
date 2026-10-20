namespace go;

using fmt = fmt_package;

partial class main_package {

public partial struct counts {
    internal array<nint> vals = new(4);
    internal nint n;
}

partial struct Counts /*counts*/;

partial struct Tally /*Counts*/;

partial struct Score /*Tally*/;

internal static void Main() {
    Tally t = new();
    t.vals[1] = 5;
    t.n = 2;
    fmt.Println("zero:", len(t.vals), t.vals, t.n);

    var p = @new<Tally>();
    p.Value.vals[3] = 7;
    fmt.Println("new:", len((~p).vals), (~p).vals);

    var l = new Tally(new counts(n: 1));
    l.vals[2] = 9;
    fmt.Println("literal:", len(l.vals), l.vals, l.n);

    var c = t.ΔClone();
    c.vals[0] = 11;
    fmt.Println("copy:", t.vals, c.vals);

    Score s = new();
    s.vals[0] = 3;
    s.n = 4;
    fmt.Println("three levels:", len(s.vals), s.vals, s.n);
}

} // end main_package

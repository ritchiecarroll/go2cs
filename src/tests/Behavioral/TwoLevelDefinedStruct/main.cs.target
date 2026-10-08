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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object zeroˢ = (@string)"zero:"u8;
private static readonly object newˢ = (@string)"new:"u8;
private static readonly object literalˢ = (@string)"literal:"u8;
private static readonly object copyˢ = (@string)"copy:"u8;
private static readonly object threeLevelsˢ = (@string)"three levels:"u8;

internal static void Main() {
    Tally t = new();
    t.vals[1] = 5;
    t.n = 2;
    fmt.Println(zeroˢ, len(t.vals), t.vals, t.n);
    var p = @new<Tally>();
    p.Value.vals[3] = 7;
    fmt.Println(newˢ, len((~p).vals), (~p).vals);
    var l = new Tally(new counts(n: 1));
    l.vals[2] = 9;
    fmt.Println(literalˢ, len(l.vals), l.vals, l.n);
    var c = t.ΔClone();
    c.vals[0] = 11;
    fmt.Println(copyˢ, t.vals, c.vals);
    Score s = new();
    s.vals[0] = 3;
    s.n = 4;
    fmt.Println(threeLevelsˢ, len(s.vals), s.vals, s.n);
}

} // end main_package

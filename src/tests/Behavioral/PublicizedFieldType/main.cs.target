namespace go;

using fmt = fmt_package;

partial class main_package {

public partial struct d /*[3]rune*/;

public partial struct inner {
    internal nint v;
}

public partial struct level /*num:nint*/;

partial struct CaseRange {
    public uint32 Lo;
    public d Delta;
    public inner Item;
    public level Lvl;
}

public partial struct coder {
    internal @string tag;
    internal nint seq;
}

partial struct EncBuffer /*coder*/;

public partial struct tally /*num:nint*/;

public partial struct weight /*num:nint*/;

public static tally Tally(this CaseRange cr) {
    cr = cr.ΔClone();

    return ((tally)(nint)cr.Lo);
}

public static nint Weigh(this CaseRange cr, weight w) {
    cr = cr.ΔClone();

    return (nint)w * 2;
}

internal static void Main() {
    CaseRange cr = new();
    cr.Lo = 65;
    cr.Item = new inner(v: 9);
    cr.Lvl = ((level)3);
    fmt.Println(cr.Lo);
    fmt.Println(cr.Delta[0]);
    fmt.Println(cr.Item.v);
    fmt.Println(cr.Lvl);
    var b = new EncBuffer(new coder(tag: "png"u8, seq: 7));
    fmt.Println(b.tag, b.seq);
    fmt.Println(cr.Tally(), cr.Weigh(3));
}

} // end main_package

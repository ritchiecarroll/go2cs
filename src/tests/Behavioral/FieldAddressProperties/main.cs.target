namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Element {
    internal ж<Element> next, prev;
    internal ж<List> list;
    public any Value;
}

partial struct List {
    internal Element root;
    internal nint len;
}

public static ж<List> Init(this ж<List> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    l.root.next = Ꮡl.Ꮡroot;
    l.root.prev = Ꮡl.Ꮡroot;
    l.len = 0;
    return Ꮡl;
}

public static ж<Element> PushBack(this ж<List> Ꮡl, any v) {
    ref var l = ref Ꮡl.DerefOrNull();

    var e = Ꮡ(new Element(Value: v, list: Ꮡl));
    var at = l.root.prev;
    e.prev = at;
    e.next = at.next;
    e.prev.next = e;
    e.next.prev = e;
    l.len++;
    return e;
}

public static ж<Element> Front(this ref List l) {
    if (l.len == 0) {
        return default;
    }
    return l.root.next;
}

public static ж<Element> Next(this ref Element e) {
    {
        var p = e.next; if (e.list != nil && p != e.list.Ꮡroot) {
            return p;
        }
    }
    return default;
}

partial struct counter {
    internal nint hits;
    internal @string name;
}

partial struct pair<T> {
    public T First;
    public nint Count;
}

internal static slice<ж<nint>> kept;

internal static ж<nint> keep(ж<nint> p) {
    kept = append(kept, p);
    return p;
}

internal static void bump(ref nint p) {
    p++;
}

internal static void Main() {
    var l = @new<List>().Init();
    for (nint i = 1; i <= 3; i++) {
        l.PushBack(i);
    }
    for (var e = l.Front(); e != nil; e = e.Next()) {
        fmt.Print((~e).Value, " ");
    }
    fmt.Println(l.len, (~l.root.next).Value, (~l.root.prev).Value);

    ref var c = ref heap<ж<counter>>(Ꮡ(new counter(name: "c")), out var Ꮡc);
    var (a, b) = (keep(c.Ꮡhits), keep(c.Ꮡhits));
    fmt.Println("same field, two sites:", a == b);
    bump(ref (a).DerefOrNull());
    bump(ref (b).DerefOrNull());
    fmt.Println("hits:", c.hits);

    var pp = Ꮡc;
    var q = keep((pp.ValueSlot).Ꮡhits);
    bump(ref (q).DerefOrNull());
    fmt.Println("through a pointer to a pointer:", c.hits, q == a);

    ref var v = ref heap(new counter(), out var Ꮡv);
    var vh = keep(Ꮡv.Ꮡhits);
    bump(ref (vh).DerefOrNull());
    fmt.Println("heap value:", v.hits, vh == Ꮡv.Ꮡhits);

    var g = Ꮡ(new pair<@string>(First: "x"));
    var gf = g.ᏑFirst;
    gf.Value = "y";
    var gc = keep(g.ᏑCount);
    bump(ref (gc).DerefOrNull());
    fmt.Println("generic:", g.First, g.Count);
}

} // end main_package

global using NodePtr = go.ж<go.main_package.Node>;
global using ChainPtr = go.ж<go.main_package.Node>;
global using ListPtr = go.ж<go.main_package.List>;

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Node {
    internal nint kind;
    internal @string name;
    internal ж<Node> next;
}

partial struct List /*[]nint*/;

internal static slice<ж<Node>> registry;

public static (nint, bool) Mode(this NodePtr Ꮡp, nint def) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (Ꮡp == nil) {
        return (def, false);
    }
    return (p.kind, true);
}

public static (nint, bool) ModeNamed(this ж<Node> Ꮡnode, nint def) {
    ref var node = ref Ꮡnode.DerefOrNull();

    if (Ꮡnode == nil) {
        return (def, false);
    }
    return (node.kind, true);
}

public static ж<Node> Self(this NodePtr Ꮡp) { return Ꮡp; }

internal static @string describe(ж<Node> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (Ꮡn == nil) {
        return "<nil>";
    }
    return n.name;
}

public static @string Describe(this NodePtr Ꮡp) { return describe(Ꮡp); }

public static void Register(this NodePtr Ꮡp) { registry = append(registry, Ꮡp); }

public static ж<nint> KindPtr(this NodePtr Ꮡp) { return Ꮡp.Ꮡkind; }

public static void LinkFrom(this NodePtr Ꮡp, ж<Node> Ꮡother) {
    ref var other = ref Ꮡother.DerefOrNull();

    other.next = Ꮡp;
}

public static Func<@string> Namer(this NodePtr Ꮡp) { return () => Ꮡp.Value.name; }

public static Func<nint, (nint, bool)> ModeFunc(this NodePtr Ꮡp) { return Ꮡp.Mode; }

public static @string Announce(this ref Node p) {
    fmt.Println("  side effect ran");
    return p.name;
}

public static void SetKind(this ref Node p, nint kind) { p.kind = kind; }

public static @string SafeName(this NodePtr Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    var (_, ok) = Ꮡp.Mode(0); if (!ok) {
        return "none";
    }
    return p.name;
}

public static bool IsNil(this ChainPtr Ꮡp) { return Ꮡp == nil; }

public static ж<Node> Tail(this NodePtr Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    while (Ꮡp != nil && p.next != nil) {
        Ꮡp = p.next; p = ref Ꮡp.DerefOrNull();
    }
    return Ꮡp;
}

public static nint Kind(this Node n) { return n.kind; }

public static void Push(this ref List l, nint v) { l = append(l, v); }

public static nint Len(this ListPtr Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (Ꮡl == nil) {
        return -1;
    }
    return len(l);
}

public static @string /*s*/ Deferred(this NodePtr Ꮡp) {
    @string s = default;
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        defer(() => {
            var r = recover(); if (r != nil) {
                s = "recovered";
            }
        }, ref ᒐ);
        s = p.name;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return s;
}

public static @string String(this NodePtr Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (Ꮡp == nil) {
        return "Node(nil)";
    }
    return "Node("u8 + p.name + ")"u8;
}

internal static nint viaParam(NodePtr Ꮡq) {
    ref var q = ref Ꮡq.DerefOrNull();

    if (Ꮡq == nil) {
        return -1;
    }
    q.kind++;
    return q.kind + Ꮡq.Kind();
}

internal static @string viaLocal(ж<Node> n) {
    NodePtr q = n;
    if (q == nil) {
        return "<nil>";
    }
    return q.Describe() + "/"u8 + q.name;
}

internal static nint walk(NodePtr Ꮡq) {
    ref var q = ref Ꮡq.DerefOrNull();

    nint count = 0;
    while (Ꮡq != nil) {
        count++;
        Ꮡq = q.next; q = ref Ꮡq.DerefOrNull();
    }
    return count;
}

internal static ж<nint> kindOf(NodePtr q) { return q.Ꮡkind; }

internal static Node copyOf(ref Node q) { return q; }

internal static void elements(ж<Node> n) {
    var nodes = slice<NodePtr>([n, default, Ꮡ(new Node(kind: 8, name: "lit"))]);
    foreach (var (i, q) in range(nodes)) {
        var (mode, ok) = q.Mode(-1);
        fmt.Println(i, mode, ok, q.Describe());
    }

    var byName = new map<@string, NodePtr>{["n"] = n};
    fmt.Println(byName["n"] == n, byName["missing"] == nil, byName["missing"].IsNil());

    var ch = new channel<NodePtr>(1);
    ch.ᐸꟷ(n);
    fmt.Println(ᐸꟷ(ch) == n);

    NodePtr typedNil = default;
    any boxed = typedNil.OrTypedNil();
    fmt.Println(boxed == nil, boxed._<NodePtr>() == nil);

    switch (boxed.type()) {
    case NodePtr q: {
        fmt.Println("pointer case", q == nil);
        break;
    }
    default: {
        var q = boxed;
        fmt.Println("default case");
        break;
    }}
}

internal static void setKind(ж<Node> Ꮡp, nint kind)
    /* where P : *Node (erased: P renders as ж<Node>) */
{
    ref var p = ref Ꮡp.DerefOrNull();

    (p).kind = kind;
}

internal static bool isNilOf(ж<Node> p)
    /* where P : *Node (erased: P renders as ж<Node>) */
{ return p == nil; }

internal static nint kindVia(ж<Node> Ꮡp)
    /* where P : *Node (erased: P renders as ж<Node>) */
{
    ref var p = ref Ꮡp.DerefOrNull();

    return (p).kind;
}

internal static void @try(@string label, Action fn) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover(); if (r != nil) {
                fmt.Println(label, "panic:", r);
            }
        }, ref ᒐ);
        fn();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct holder {
    internal NodePtr attrs;
    internal ListPtr list;
}

internal static void run(@string tag, ж<holder> h) {
    fmt.Println("==", tag);
    @try("Mode", () => {
        var (ᴛ1, ᴛ2) = h.Value.attrs.Mode(7);
        fmt.Println(ᴛ1, ᴛ2);
    });
    @try("ModeNamed", () => {
        var (ᴛ3, ᴛ4) = h.Value.attrs.ModeNamed(7);
        fmt.Println(ᴛ3, ᴛ4);
    });
    @try("Self", () => {
        fmt.Println(h.Value.attrs.Self() == h.Value.attrs);
    });
    @try("Describe", () => {
        fmt.Println(h.Value.attrs.Describe());
    });
    @try("Register", () => {
        h.Value.attrs.Register();
        fmt.Println(len(registry), registry[len(registry) - 1] == h.Value.attrs);
    });
    @try("KindPtr", () => {
        var p = h.Value.attrs.KindPtr();
        p.Value = p.Value + 1;
        fmt.Println(p.Value, h.Value.attrs.kind);
    });
    @try("LinkFrom", () => {
        ref var o = ref heap(new Node(), out var Ꮡo);
        h.Value.attrs.LinkFrom(Ꮡo);
        fmt.Println(o.next == h.Value.attrs);
    });
    @try("Namer", () => {
        var f = h.Value.attrs.Namer();
        fmt.Println("namer made");
        fmt.Println(f());
    });
    @try("ModeFunc", () => {
        var f = h.Value.attrs.ModeFunc();
        var (ᴛ5, ᴛ6) = f(9);
        fmt.Println(ᴛ5, ᴛ6);
    });
    @try("Announce", () => {
        fmt.Println(h.Value.attrs.Announce());
    });
    @try("SetKind", () => {
        h.Value.attrs.SetKind(h.Value.attrs.kind + 10);
        fmt.Println(h.Value.attrs.kind);
    });
    @try("SafeName", () => {
        fmt.Println(h.Value.attrs.SafeName());
    });
    @try("IsNil", () => {
        fmt.Println(h.Value.attrs.IsNil());
    });
    @try("Tail", () => {
        fmt.Println(h.Value.attrs.Tail() == h.Value.attrs);
    });
    @try("Kind", () => {
        fmt.Println(h.Value.attrs.Kind());
    });
    @try("Len", () => {
        fmt.Println(h.Value.list.Len());
    });
    @try("Push", () => {
        h.Value.list.Push(4);
        fmt.Println(h.Value.list.Len(), h.Value.list.ValueSlot);
    });
    @try("Deferred", () => {
        fmt.Println(h.Value.attrs.Deferred());
    });
    @try("String", () => {
        fmt.Println(h.Value.attrs.String());
    });
    @try("Stringer", () => {
        fmt.Println(h.Value.attrs.OrTypedNil());
    });
    @try("expression", () => {
        var f = ((Func<ж<Node>, nint, (nint, bool)>)(Mode));
        var (ᴛ7, ᴛ8) = f(h.Value.attrs, 12);
        fmt.Println(ᴛ7, ᴛ8);
    });
}

internal static void Main() {
    run("nil", Ꮡ(new holder()));
    run("non-nil", Ꮡ(new holder(attrs: Ꮡ(new Node(kind: 3, name: "attrs")), list: Ꮡ(new List(new nint[]{1, 2}.slice())))));
    run("zero value", Ꮡ(new holder(attrs: Ꮡ(new Node()), list: Ꮡ(new List(new nint[]{}.slice())))));

    ref var v = ref heap(new Node(), out var Ꮡv);
    v.SetKind(5);
    var (ᴛ9, ᴛ10) = Ꮡv.Mode(0);
    fmt.Println(ᴛ9, ᴛ10);
    fmt.Println(v.Announce(), Ꮡv.Tail() == Ꮡv, v.Kind());

    var linked = Ꮡ(new Node(name: "head", next: Ꮡ(new Node(name: "tail"))));
    NodePtr q = linked;
    fmt.Println(linked.Tail().name, q.Tail() == linked.next);
    
    var qʗ1 = q;
    var mode = (nint p1) => qʗ1.Mode(p1);
    var (ᴛ11, ᴛ12) = mode(11);
    fmt.Println(ᴛ11, ᴛ12);

    fmt.Println(viaParam(linked), viaParam(nil));
    fmt.Println(viaLocal(linked), viaLocal(nil));
    fmt.Println(walk(linked), walk(nil));
    kindOf(linked).Value = 9;
    fmt.Println(copyOf(ref (linked).DerefOrNull()).kind);
    @try("copyOf", () => {
        fmt.Println(copyOf(ref ((NodePtr)default).DerefOrNull()).kind);
    });
    elements(linked);

    setKind(linked, 21);
    fmt.Println(linked.kind, isNilOf(linked), isNilOf(nil), kindVia(linked));
}

} // end main_package

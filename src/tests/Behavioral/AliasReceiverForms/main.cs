global using AttrNode = go.main_package.Node;
global using Chain = go.main_package.Node;
global using ListAlias = go.main_package.List;

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

public static (nint, bool) Mode(this ref AttrNode n, nint def) {
    if (n == nil) {
        return (def, false);
    }
    return (n.kind, true);
}

public static (nint, bool) ModeNamed(this ж<Node> Ꮡnode, nint def) {
    ref var node = ref Ꮡnode.DerefOrNull();

    if (Ꮡnode == nil) {
        return (def, false);
    }
    return (node.kind, true);
}

public static ж<Node> Self(this ref AttrNode n) { return Ꮡn; }

internal static @string describe(ж<Node> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (Ꮡn == nil) {
        return "<nil>";
    }
    return n.name;
}

public static @string Describe(this ref AttrNode n) { return describe(n); }

public static void Register(this ref AttrNode n) { registry = append(registry, n); }

public static ж<nint> KindPtr(this ref AttrNode n) { return n.Ꮡkind; }

public static void LinkFrom(this ref AttrNode n, ж<Node> Ꮡother) {
    ref var other = ref Ꮡother.DerefOrNull();

    other.next = n;
}

public static Func<@string> Namer(this ref AttrNode n) { return () => n.name; }

public static Func<nint, (nint, bool)> ModeFunc(this ref AttrNode n) { return n.Mode; }

public static @string Announce(this ref AttrNode n) {
    fmt.Println("  side effect ran");
    return n.name;
}

public static @string SafeName(this ref AttrNode n) {
    var (_, ok) = n.Mode(0); if (!ok) {
        return "none";
    }
    return n.name;
}

public static bool IsNil(this ref Chain n) { return n == nil; }

public static nint Kind(this AttrNode n) { return n.kind; }

public static void Push(this ref ListAlias l, nint v) { l = append(l, v); }

public static nint Len(this ref ListAlias l) {
    if (l == nil) {
        return -1;
    }
    return len(l);
}

public static @string /*s*/ Deferred(this ref AttrNode n) {
    @string s = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover(); if (r != nil) {
                s = "recovered";
            }
        }, ref ᒐ);
        s = n.name;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return s;
}

public static @string String(this ref AttrNode n) {
    if (n == nil) {
        return "Node(nil)";
    }
    return "Node("u8 + n.name + ")"u8;
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
    internal ж<AttrNode> attrs;
    internal ж<ListAlias> list;
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
    @try("SafeName", () => {
        fmt.Println(h.Value.attrs.SafeName());
    });
    @try("IsNil", () => {
        fmt.Println(h.Value.attrs.IsNil());
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
}

internal static void Main() {
    run("nil", Ꮡ(new holder()));
    run("non-nil", Ꮡ(new holder(attrs: Ꮡ(new Node(kind: 3, name: "attrs")), list: Ꮡ(new List(new nint[]{1, 2}.slice())))));
    run("zero value", Ꮡ(new holder(attrs: Ꮡ(new Node()), list: Ꮡ(new List(new nint[]{}.slice())))));
}

} // end main_package

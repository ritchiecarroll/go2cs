namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct node {
    internal nint val;
    internal slice<ж<node>> kids;
}

} partial class mainᴛnode {

internal static bool each(this ref node n, Func<ж<node>, bool> f) {
    foreach (var (_, k) in range(n.kids)) {
        if (f(k)) {
            return true;
        }
    }
    return false;
}

} partial class main_package {

internal static nint deepest(ж<node> n) {
    for (nint i = 0; i < 10; i++) {
        if (len(n.DerefOrNull().kids) == 0) {
            return n.DerefOrNull().val;
        }
        n.DerefOrNull().each((ж<node> k) => {
            n = k;
            return true;
        });
    }
    return -1;
}

internal static nint afterCall(ж<node> n) {
    void step() {
        n = n.DerefOrNull().kids[0];
    }
    step();
    return n.DerefOrNull().val;
}

internal static void writeAfter(ж<node> n) {
    var root = n;
    void step() {
        n = n.DerefOrNull().kids[0];
    }
    step();
    n.DerefOrNull().val += 10;
    fmt.Println("writeAfter:", root.val, n.DerefOrNull().val);
    n.DerefOrNull().val -= 10;
}

internal static nint copyAfter(ж<node> n) {
    void step() {
        n = n.DerefOrNull().kids[0];
    }
    step();
    var c = n.DerefOrNull();
    return c.val;
}

internal static partial nint goMoved(ж<node> n) {
    var done = new channel<bool>(0);
    var doneʗ1 = done;
    goǃ(() => {
        n = n.DerefOrNull().kids[0];
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    return n.DerefOrNull().val;
}

internal static nint /*v*/ deferMoved(ж<node> n) {
    nint v = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            v = n.DerefOrNull().val;
        }, ref ᒐ);
        defer(() => {
            n = n.DerefOrNull().kids[0];
        }, ref ᒐ);
        v = n.DerefOrNull().val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return v;
}

internal static nint rangeMoved(ж<node> n) {
    void f() {
        foreach (var (_, vᴛ1) in range(n.DerefOrNull().kids)) {
            n = vᴛ1;

        }
    }
    f();
    return n.DerefOrNull().val;
}

internal static nint rangeBody(ж<node> Ꮡn) {
    ref var nΔ1 = ref Ꮡn.DerefOrNull();

    foreach (var (_, vᴛ1) in range(nΔ1.kids)) {
        Ꮡn = vᴛ1;
        nΔ1 = ref Ꮡn.DerefOrNull();

    }
    return nΔ1.val;
}

} partial class mainᴛnode {

internal static nint lastKid(this ж<node> Ꮡn) {
    ref var nΔ1 = ref Ꮡn.DerefOrNull();

    foreach (var (_, vᴛ1) in range(nΔ1.kids)) {
        Ꮡn = vᴛ1;
        nΔ1 = ref Ꮡn.DerefOrNull();

    }
    return nΔ1.val;
}

} partial class main_package {

internal static nint mixed(ж<node> n) {
    n = n.DerefOrNull().kids[0];
    void step() {
        n = n.DerefOrNull().kids[0];
    }
    step();
    return n.DerefOrNull().val;
}

} partial class mainᴛnode {

internal static nint last(this ж<node> Ꮡn) {
    Ꮡn.DerefOrNull().each((ж<node> k) => {
        Ꮡn = k;
        return false;
    });
    return Ꮡn.DerefOrNull().val;
}

} partial class main_package {

internal static bool errAfter(ж<error> e) {
    ref var other = ref heap<error>(out var Ꮡother);
    void set() {
        e = Ꮡother;
    }
    set();
    return e.DerefOrNull() == nil;
}

internal static (nint, nint) sliceAfter(ж<slice<nint>> s) {
    ref var other = ref heap<slice<nint>>(slice<nint>([7]), out var Ꮡother);
    void set() {
        s = Ꮡother;
    }
    set();
    return (len(s.DerefOrNull()), (s.DerefOrNull())[0]);
}

} partial class mainᴛnode {

internal static nint count(this ж<node> Ꮡn) {
    if (Ꮡn == nil) {
        return 0;
    }
    return 1;
}

internal static nint tag(this ref node n) {
    fmt.Println("tag runs before its deref");
    return n.val;
}

} partial class main_package {

internal static nint nilCount(ж<node> n) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            n = default;
        }, ref ᒐ);
        return n.count();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static nint /*v*/ nilTag(ж<node> n) {
    nint v = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            if (recover() != nil) {
                v = -1;
            }
        }, ref ᒐ);
        defer(() => {
            n = default;
        }, ref ᒐ);
        v = n.DerefOrNull().tag();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return v;
}

internal static nint gotoWalk(ж<node> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

loop:
    if (len(n.kids) == 0) {
        return n.val;
    }
    Ꮡn = n.kids[0]; n = ref Ꮡn.DerefOrNull();
    goto loop;
}

internal static void main() {
    var leaf = Ꮡ(new node(val: 3));
    var root = Ꮡ(new node(val: 1, kids: [Ꮡ(new node(val: 2, kids: slice<ж<node>>([leaf])))]));

    fmt.Println("deepest:", deepest(root));
    fmt.Println("afterCall:", afterCall(root));
    writeAfter(root);
    fmt.Println("copyAfter:", copyAfter(root));
    fmt.Println("goMoved:", goMoved(root));
    fmt.Println("deferMoved:", deferMoved(root));
    fmt.Println("rangeMoved:", rangeMoved(root));
    fmt.Println("rangeBody:", rangeBody(root));
    fmt.Println("lastKid:", root.lastKid());
    fmt.Println("mixed:", mixed(root));
    fmt.Println("last:", root.last());
    ref var failed = ref heap<error>(fmt.Errorf("failed"u8), out var Ꮡfailed);
    fmt.Println("errAfter:", errAfter(Ꮡfailed));
    var (ᴛ1, ᴛ2) = sliceAfter(Ꮡ(slice<nint>([1, 2, 3])));
    fmt.Println("sliceAfter:", fmt.Sprint(ᴛ1, ᴛ2));
    fmt.Println("nilCount:", nilCount(nil), nilCount(leaf));
    fmt.Println("nilTag:", nilTag(nil));
    fmt.Println("gotoWalk:", gotoWalk(root));
    fmt.Println("root:", root.val, root.kids[0].val, leaf.val);
}

} // end main_package

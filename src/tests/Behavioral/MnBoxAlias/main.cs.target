namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct inner {
    internal nint n;
}

partial struct acct {
    internal nint bal;
    internal inner sub;
}

partial struct cell {
    internal nint v;
}

partial struct holder {
    internal cell c;
}

partial struct bag {
    internal slice<cell> s;
}

internal static nint waitAt(this ref bag b, nint i) {
    return b.s[i].waitAndRead();
}

internal static readonly ж<cell> ᏑpkgCell = new StaticBox<cell>(new cell(v: 1));
internal static ref cell pkgCell => ref ((StaticBox<cell>)ᏑpkgCell).ValueSlot;

internal static channel<EmptyStruct> ready = new channel<EmptyStruct>(0);
internal static channel<EmptyStruct> resume = new channel<EmptyStruct>(0);

internal static nint waitAndRead(this ref acct a) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    return a.bal;
}

internal static nint waitAndRead(this ref inner s) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    return s.n;
}

internal static nint waitAndRead(this ref cell c) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    return c.v;
}

internal static nint valueRead(this acct a) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    nint got = a.bal;
    a.bal = 777;
    return got;
}

internal static nint readCell(ref cell c) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    return c.v;
}

internal static nint forward(ref cell c) {
    return readCell(ref c);
}

partial struct ptrs {
    internal ж<cell> p;
}

internal static nint readBoth(ref holder h, ref cell c) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    return h.c.v * 100 + c.v;
}

internal static nint readArr(ref array<cell> a, ref cell c) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    return a[1].v * 100 + c.v;
}

internal static void thunkRead(channel<nint> done, ref cell c) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    done.ᐸꟷ(c.v);
}

internal static @string /*out*/ nilParam(ref holder nh) {
    @string @out = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover(); if (r != nil) {
                @out = fmt.Sprint("panic: ", r);
            }
        }, ref ᒐ);
        nint v = readCell(ref nonnil(ref nh).c);
        @out = fmt.Sprint(v);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return @out;
}

internal static void writer(Action write) {
    ᐸꟷ(ready);
    write();
    resume.ᐸꟷ(new EmptyStruct());
}

internal static @string /*out*/ nilRoot(ж<acct> Ꮡnp) {
    @string @out = default;
    GoFrame ᒐ = default;
    try {
        ref var np = ref Ꮡnp.DerefOrNull();

        defer(() => {
            var r = recover(); if (r != nil) {
                @out = fmt.Sprint("panic: ", r);
            }
        }, ref ᒐ);
        nint v = np.waitAndRead();
        @out = fmt.Sprint(v);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return @out;
}

internal static @string /*out*/ nilField(ж<holder> Ꮡnh) {
    @string @out = default;
    GoFrame ᒐ = default;
    try {
        ref var nh = ref Ꮡnh.DerefOrNull();

        defer(() => {
            var r = recover(); if (r != nil) {
                @out = fmt.Sprint("panic: ", r);
            }
        }, ref ᒐ);
        nint v = nh.c.waitAndRead();
        @out = fmt.Sprint(v);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return @out;
}

internal static partial void main() {
    var a = Ꮡ(new acct(bal: 1, sub: new inner(n: 1)));

    var aʗ1 = a;
    goǃ(writer, () => {
        aʗ1.bal = 2;
    });
    nint root = a.waitAndRead();
    fmt.Println("root", root);

    var aʗ2 = a;
    goǃ(writer, () => {
        aʗ2.sub.n = 7;
    });
    nint field = a.Ꮡsub.waitAndRead();
    fmt.Println("field", field);

    var cells = slice<cell>([new(v: 1)]);
    var old = cells;
    cells = append(cells, new cell(v: 9));
    var oldʗ1 = old;
    goǃ(writer, () => {
        oldʗ1[0].v = 4;
    });
    nint element = old[0].waitAndRead();
    fmt.Println("element", element);
    fmt.Println("element new-array", cells[0].v, "old-array", old[0].v);

    var b = Ꮡ(new bag(s: [new(v: 1), new(v: 1)]));
    var bʗ1 = b;
    goǃ(writer, () => {
        bʗ1.s[1].v = 3;
    });
    nint ownElement = b.waitAt(1);
    fmt.Println("own-element", ownElement);

    var pc = Ꮡ(new cell(v: 1));
    var pcʗ1 = pc;
    goǃ(writer, () => {
        pcʗ1.v = 5;
    });
    nint param = readCell(ref (pc).DerefOrNull());
    fmt.Println("param", param);

    var pcʗ2 = pc;
    goǃ(writer, () => {
        pcʗ2.v = 6;
    });
    nint forwarded = forward(ref (pc).DerefOrNull());
    fmt.Println("param-forward", forwarded);

    ref var hc = ref heap(new cell(), out var Ꮡhc);
    hc.v = 1;
    var hp = Ꮡhc;
    var hpʗ1 = hp;
    goǃ(writer, () => {
        hpʗ1.v = 8;
    });
    fmt.Println("param-heap", readCell(ref hc));

    var cs = slice<cell>([new(v: 1), new(v: 1)]);
    var csʗ1 = cs;
    goǃ(writer, () => {
        csʗ1[1].v = 9;
    });
    fmt.Println("param-element", readCell(ref cs[1]));

    var h = Ꮡ(new holder(c: new cell(v: 1)));
    var hʗ1 = h;
    goǃ(writer, () => {
        hʗ1.c.v = 10;
    });
    fmt.Println("param-field", readCell(ref (h.Ꮡc).DerefOrNull()));

    var e = Ꮡ(new ptrs(p: Ꮡ(new cell(v: 1))));
    var eʗ1 = e;
    goǃ(writer, () => {
        eʗ1.p.v = 11;
    });
    fmt.Println("param-ptr-field", readCell(ref (e.p).DerefOrNull()));

    var hh = Ꮡ(new holder(c: new cell(v: 1)));
    var hhʗ1 = hh;
    goǃ(writer, () => {
        hhʗ1.c.v = 12;
    });
    fmt.Println("param-two", readBoth(ref (hh).DerefOrNull(), ref (hh.Ꮡc).DerefOrNull()));

    ref var arr = ref heap(new array<cell>(2), out var Ꮡarr);
    var ap = Ꮡarr;
    var apʗ1 = ap;
    goǃ(writer, () => {
        apʗ1.Value[1].v = 13;
    });
    fmt.Println("param-array", readArr(ref arr, ref arr[1]));

    var tp = Ꮡ(new cell(v: 1));
    var done = new channel<nint>(0);
    goǃ((ᴛ1, ᴛ2) => thunkRead(ᴛ1, ref ᴛ2.DerefOrNull()), done, tp);
    var tpʗ1 = tp;
    goǃ(writer, () => {
        tpʗ1.v = 14;
    });
    fmt.Println("param-thunk", ᐸꟷ(done));

    goǃ(writer, () => {
        pkgCell.v = 15;
    });
    fmt.Println("param-package", readCell(ref pkgCell));

    var aʗ3 = a;
    goǃ(writer, () => {
        aʗ3.bal = 50;
    });
    nint value = a.valueRead();
    fmt.Println("value", value);
    fmt.Println("value after", a.bal);

    goǃ(writer, () => {
    });
    fmt.Println("nil", nilRoot(nil));

    fmt.Println("nil-field", nilField(nil));

    fmt.Println("param-nil-field", nilParam(ref ((ж<holder>)default).DerefOrNull()));
}

} // end main_package

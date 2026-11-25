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

partial struct rcell {
    internal @string name;
    internal nint v;
}

partial struct rholder {
    internal slice<nint> s;
    internal rcell c;
}

partial struct rnest {
    internal slice<nint> p;
    internal rholder h;
}

partial struct nest {
    internal holder h;
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

internal static nint chainRead(ref nest n) {
    return readCell(ref nonnil(ref n).h.c);
}

internal static nint readRCell(ref rcell c) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    return c.v;
}

internal static nint chainRef(ref rnest n) {
    return readRCell(ref nonnil(ref n).h.c);
}

internal static @string /*out*/ nilChain(ref nest n) {
    @string @out = default;
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("nilChain.func1")] () => {
            var r = recover(); if (r != nil) {
                @out = fmt.Sprint("panic: ", r);
            }
        }, ref ᒐ);
        nint v = readCell(ref nonnil(ref n).h.c);
        @out = fmt.Sprint(v);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return @out;
}

internal static @string /*out*/ nilParam(ref holder nh) {
    @string @out = default;
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("nilParam.func1")] () => {
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

internal static nint readCellK(ref cell c, nint k) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    return c.v * 100 + k;
}

internal static nint parkK() {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    return 3;
}

internal static void bumpCell(ref cell c) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    c.v += 20;
}

internal static void writer2(Action first, Action second) {
    ᐸꟷ(ready);
    first();
    resume.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(ready);
    second();
    resume.ᐸꟷ(new EmptyStruct());
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

        defer([GoFunc("nilRoot.func1")] () => {
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

        defer([GoFunc("nilField.func1")] () => {
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
    goǃ(writer, [GoFunc("main.func1")] () => {
        aʗ1.bal = 2;
    });
    nint root = a.waitAndRead();
    fmt.Println("root", root);

    var aʗ2 = a;
    goǃ(writer, [GoFunc("main.func2")] () => {
        aʗ2.sub.n = 7;
    });
    nint field = a.Ꮡsub.waitAndRead();
    fmt.Println("field", field);

    var cells = slice<cell>([new(v: 1)]);
    var old = cells;
    cells = append(cells, new cell(v: 9));
    var oldʗ1 = old;
    goǃ(writer, [GoFunc("main.func3")] () => {
        oldʗ1[0].v = 4;
    });
    nint element = old[0].waitAndRead();
    fmt.Println("element", element);
    fmt.Println("element new-array", cells[0].v, "old-array", old[0].v);

    var b = Ꮡ(new bag(s: [new(v: 1), new(v: 1)]));
    var bʗ1 = b;
    goǃ(writer, [GoFunc("main.func4")] () => {
        bʗ1.s[1].v = 3;
    });
    nint ownElement = b.waitAt(1);
    fmt.Println("own-element", ownElement);

    var pc = Ꮡ(new cell(v: 1));
    var pcʗ1 = pc;
    goǃ(writer, [GoFunc("main.func5")] () => {
        pcʗ1.v = 5;
    });
    nint param = readCell(ref (pc).DerefOrNull());
    fmt.Println("param", param);

    var pcʗ2 = pc;
    goǃ(writer, [GoFunc("main.func6")] () => {
        pcʗ2.v = 6;
    });
    nint forwarded = forward(ref (pc).DerefOrNull());
    fmt.Println("param-forward", forwarded);

    ref var hc = ref heap(new cell(), out var Ꮡhc);
    hc.v = 1;
    var hp = Ꮡhc;
    var hpʗ1 = hp;
    goǃ(writer, [GoFunc("main.func7")] () => {
        hpʗ1.v = 8;
    });
    fmt.Println("param-heap", readCell(ref hc));

    var cs = slice<cell>([new(v: 1), new(v: 1)]);
    var csʗ1 = cs;
    goǃ(writer, [GoFunc("main.func8")] () => {
        csʗ1[1].v = 9;
    });
    fmt.Println("param-element", readCell(ref cs[1]));

    var h = Ꮡ(new holder(c: new cell(v: 1)));
    var hʗ1 = h;
    goǃ(writer, [GoFunc("main.func9")] () => {
        hʗ1.c.v = 10;
    });
    fmt.Println("param-field", readCell(ref (h.Ꮡc).DerefOrNull()));

    var e = Ꮡ(new ptrs(p: Ꮡ(new cell(v: 1))));
    var eʗ1 = e;
    goǃ(writer, [GoFunc("main.func10")] () => {
        eʗ1.p.v = 11;
    });
    fmt.Println("param-ptr-field", readCell(ref (e.p).DerefOrNull()));

    var hh = Ꮡ(new holder(c: new cell(v: 1)));
    var hhʗ1 = hh;
    goǃ(writer, [GoFunc("main.func11")] () => {
        hhʗ1.c.v = 12;
    });
    fmt.Println("param-two", readBoth(ref (hh).DerefOrNull(), ref (hh.Ꮡc).DerefOrNull()));

    ref var arr = ref heap(new array<cell>(2), out var Ꮡarr);
    var ap = Ꮡarr;
    var apʗ1 = ap;
    goǃ(writer, [GoFunc("main.func12")] () => {
        apʗ1.Value[1].v = 13;
    });
    fmt.Println("param-array", readArr(ref arr, ref arr[1]));

    var tp = Ꮡ(new cell(v: 1));
    var done = new channel<nint>(0);
    goǃ((ᴛ1, ᴛ2) => thunkRead(ᴛ1, ref ᴛ2.DerefOrNull()), done, tp);
    var tpʗ1 = tp;
    goǃ(writer, [GoFunc("main.func13")] () => {
        tpʗ1.v = 14;
    });
    fmt.Println("param-thunk", ᐸꟷ(done));

    goǃ(writer, [GoFunc("main.func14")] () => {
        pkgCell.v = 15;
    });
    fmt.Println("param-package", readCell(ref pkgCell));

    var nc = Ꮡ(new nest(h: new holder(c: new cell(v: 1))));
    var ncʗ1 = nc;
    goǃ(writer, [GoFunc("main.func15")] () => {
        ncʗ1.h.c.v = 16;
    });
    fmt.Println("param-chain", readCell(ref (nc.Ꮡh.Ꮡc).DerefOrNull()));

    var ncʗ2 = nc;
    goǃ(writer, [GoFunc("main.func16")] () => {
        ncʗ2.h.c.v = 17;
    });
    fmt.Println("param-chain-param", chainRead(ref (nc).DerefOrNull()));

    var rn = Ꮡ(new rnest(p: [1], h: new rholder(s: [2], c: new rcell(name: "r", v: 1))));
    var rnʗ1 = rn;
    goǃ(writer, [GoFunc("main.func17")] () => {
        rnʗ1.h.c.v = 18;
    });
    fmt.Println("param-chain-ref", chainRef(ref (rn).DerefOrNull()));

    var pk = Ꮡ(new cell(v: 1));

    var pkʗ1 = pk;
    goǃ(writer2, [GoFunc("main.func18")] () => {
    }, [GoFunc("main.func19")] () => {
        pkʗ1.v = 19;
    });
    var evalᴛ1 = parkK();
    fmt.Println("param-later-call", readCellK(ref (pk).DerefOrNull(), evalᴛ1));

    cell sv = default;
    sv.v = 1;
    goǃ(writer, [GoFunc("main.func20")] () => {
    });
    bumpCell(ref sv);
    fmt.Println("param-stack", sv.v);

    goǃ(writer2, [GoFunc("main.func21")] () => {
    }, [GoFunc("main.func22")] () => {
    });
    var si = new cell(v: parkK());
    bumpCell(ref si);
    fmt.Println("param-stack-init", si.v);

    var aʗ3 = a;
    goǃ(writer, [GoFunc("main.func23")] () => {
        aʗ3.bal = 50;
    });
    nint value = a.valueRead();
    fmt.Println("value", value);
    fmt.Println("value after", a.bal);

    goǃ(writer, [GoFunc("main.func24")] () => {
    });
    fmt.Println("nil", nilRoot(nil));

    fmt.Println("nil-field", nilField(nil));

    fmt.Println("param-nil-field", nilParam(ref ((ж<holder>)default).DerefOrNull()));
    fmt.Println("param-nil-chain", nilChain(ref ((ж<nest>)default).DerefOrNull()));
}

} // end main_package

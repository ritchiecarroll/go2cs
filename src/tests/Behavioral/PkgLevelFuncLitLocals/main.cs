namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static void inc(this ж<counter> Ꮡc, nint by) {
    var p = Ꮡc.Ꮡn;
    p.Value += by;
}

internal static nint get(this counter c) { return c.n; }

partial struct entry {
    internal @string name;
    internal Func<nint> f;
}

[GoLazy] partial class lazyᴛtable { internal static slice<entry> table = [
    new("var-decl", [GoFunc("init.func1")] () => {
        ref var c = ref heap(new counter(), out var Ꮡc);
        Ꮡc.inc(3);
        Ꮡc.inc(4);
        return c.get();
    }),
    new("short-define", [GoFunc("init.func2")] () => {
        ref var c = ref heap<counter>(new counter(n: 10), out var Ꮡc);
        Ꮡc.inc(5);
        return c.get();
    }),
    new("for-init", [GoFunc("init.func3")] () => {
        nint total = 0;
        for (var cᴛ1 = (new counter(n: 1)); cᴛ1.get() < 4; ) {
            ref var c = ref heap<counter>(out var Ꮡc);
            c = cᴛ1;
            Ꮡc.inc(1);
            total = c.get();
            cᴛ1 = c;
        }
        return total;
    }),
    new("if-init", [GoFunc("init.func4")] () => {
        ref var c = ref heap<counter>((new counter(n: 7)), out var Ꮡc); if (c.get() > 0) {
            Ꮡc.inc(2);
            return c.get();
        }
        return -1;
    }),
    new("switch-init", [GoFunc("init.func5")] () => {
        ref var c = ref heap<counter>((new counter(n: 20)), out var Ꮡc);
        switch (ᐧ) {
        default: {
            Ꮡc.inc(2);
            return c.get();
        }}

    }),
    new("range-value", [GoFunc("init.func6")] () => {
        nint total = 0;
        foreach (var (_, vᴛ1) in range(slice<counter>([new(n: 1), new(n: 2)]))) {
            ref var c = ref heap(new counter(), out var Ꮡc);
            c = vᴛ1;

            Ꮡc.inc(10);
            total += c.get();
        }
        return total;
    }),
    new("nested-literal", [GoFunc("init.func7")] () => {
        ref var c = ref heap(new counter(), out var Ꮡc);
        [GoFunc("init.func7.1")] void inner() {
            Ꮡc.inc(6);
        }
        inner();
        inner();
        return Ꮡc.Value.get();
    })
]; }

[GoLazy] partial class lazyᴛsingle { internal static Func<nint> single = [GoFunc("init.func8")] () => {
    ref var c = ref heap(new counter(), out var Ꮡc);
    Ꮡc.inc(21);
    Ꮡc.inc(21);
    return c.get();
}; }

internal static slice<nint> inDecl() {
    slice<nint> @out = default;

    ref var a = ref heap(new counter(), out var Ꮡa);
    Ꮡa.inc(3);
    Ꮡa.inc(4);
    @out = append(@out, a.get());

    ref var b = ref heap<counter>(new counter(n: 10), out var Ꮡb);
    Ꮡb.inc(5);
    @out = append(@out, b.get());

    return @out;
}

internal static void main() {
    foreach (var (_, e) in range(table)) {
        fmt.Println(e.name, e.f());
    }
    fmt.Println("single", single());
    fmt.Println("control", inDecl());
}

} // end main_package

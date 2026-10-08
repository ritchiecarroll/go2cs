namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static void inc(this ж<counter> Ꮡc, nint by) {
    var p = Ꮡc.of(counter.Ꮡn);
    p.Value += by;
}

internal static nint get(this counter c) {
    return c.n;
}

partial struct entry {
    internal @string name;
    internal Func<nint> f;
}

internal static slice<entry> table = new entry[]{
    new("var-decl"u8, () => {
        ref var c = ref heap(new counter(), out var Ꮡc);
        Ꮡc.inc(3);
        Ꮡc.inc(4);
        return c.get();
    }),
    new("short-define"u8, () => {
        ref var c = ref heap<counter>(out var Ꮡc);
        c = new counter(n: 10);
        Ꮡc.inc(5);
        return c.get();
    }),
    new("for-init"u8, () => {
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
    new("if-init"u8, () => {
        {
            ref var c = ref heap<counter>(out var Ꮡc);
            c = (new counter(n: 7)); if (c.get() > 0) {
                Ꮡc.inc(2);
                return c.get();
            }
        }
        return -1;
    }),
    new("switch-init"u8, () => {
        {
            ref var c = ref heap<counter>(out var Ꮡc);
            c = (new counter(n: 20));
            switch (ᐧ) {
            default: {
                Ꮡc.inc(2);
                return c.get();
            }}
        }

    }),
    new("range-value"u8, () => {
        nint total = 0;
        foreach (var (_, vᴛ1) in new counter[]{new(n: 1), new(n: 2)}.slice()) {
            ref var c = ref heap(new counter(), out var Ꮡc);
            c = vᴛ1;

            Ꮡc.inc(10);
            total += c.get();
        }
        return total;
    }),
    new("nested-literal"u8, () => {
        ref var c = ref heap(new counter(), out var Ꮡc);
        void inner() {
            Ꮡc.inc(6);
        }
        inner();
        inner();
        return Ꮡc.Value.get();
    })
}.slice();

internal static Func<nint> single = () => {
    ref var c = ref heap(new counter(), out var Ꮡc);
    Ꮡc.inc(21);
    Ꮡc.inc(21);
    return c.get();
};

internal static slice<nint> inDecl() {
    slice<nint> @out = default!;
    ref var a = ref heap(new counter(), out var Ꮡa);
    Ꮡa.inc(3);
    Ꮡa.inc(4);
    @out = append(@out, a.get());
    ref var b = ref heap<counter>(out var Ꮡb);
    b = new counter(n: 10);
    Ꮡb.inc(5);
    @out = append(@out, b.get());
    return @out;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object singleˢ = (@string)"single"u8;
private static readonly object controlˢ = (@string)"control"u8;

internal static void Main() {
    foreach (var (_, e) in table) {
        fmt.Println(e.name, e.f());
    }
    fmt.Println(singleˢ, single());
    fmt.Println(controlˢ, inDecl());
}

} // end main_package

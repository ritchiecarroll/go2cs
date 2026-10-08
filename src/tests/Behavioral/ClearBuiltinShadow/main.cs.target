namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct header /*map[@string, slice<@string>]*/;

partial struct cell {
    internal array<int32> v = new(4);
    internal map<@string, nint> m;
}

partial struct box {
    internal slice<nint> data;
}

internal static void clear(this ref box b) {
    b.data = default!;
}

partial struct guard {
    internal error err;
}

internal static void recover(this ж<guard> Ꮡg) {
    GoFrame ᒐ = default;
    try {
        ref var g = ref Ꮡg.DerefOrNull();

        {
            var e = builtin.recover(); if (e != default!) {
                g.err = fmt.Errorf("recovered: %v"u8, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void run(this ж<guard> Ꮡg) {
    GoFrame ᒐ = default;
    try {
        defer(Ꮡg.recover, ref ᒐ);
        throw panic("boom");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    var s = new nint[]{1, 2, 3}.slice();
    builtin.clear(s);
    fmt.Println(s[0], s[1], s[2]);
    var b = Ꮡ(new box(data: new nint[]{4, 5}.slice()));
    b.clear();
    fmt.Println(len((~b).data));
    var m = new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2};
    builtin.clear(m);
    fmt.Println(len(m));
    var g = Ꮡ(new guard(nil));
    g.run();
    fmt.Println((~g).err);
    var h = new header(new map<@string, slice<@string>>{["a"u8] = new @string[]{"1"u8}.slice(), ["b"u8] = new @string[]{"2"u8}.slice()});
    var alias = h;
    builtin.clear(h);
    fmt.Println(len(h), len(alias));
    header hn = default!;
    builtin.clear(hn);
    fmt.Println(len(hn));
    var q = GoReflect.WithElemDims(new slice<array<int32>>(3, () => new(4)), 4);
    q[1][2] = 7;
    builtin.clear(q);
    fmt.Println(len(q[1]), q[1][2]);
    q[2][3] = 9;
    fmt.Println(q[2]);
    var c = new slice<cell>(2, () => new());
    c[0].v[1] = 3;
    c[0].m = new map<@string, nint>{["x"u8] = 1};
    builtin.clear(c);
    fmt.Println(len(c[0].v), c[0].v[1], c[0].m == default!);
    c[0].v[1] += 4;
    fmt.Println(c[0].v);
    var n = GoReflect.WithElemDims(new slice<array<array<int32>>>(2, () => new(2, () => new(3))), 2, 3);
    n[0][1][2] = 5;
    builtin.clear(n);
    n[0][1][2] = 8;
    fmt.Println(n);
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct counter {
    internal nint n;
    internal @string tag;
}

internal static void add(this ref counter c, nint k) {
    c.n += k;
    c.tag += "+"u8;
}

internal static void writeThrough(ref counter p, nint k) {
    p.n += k;
}

internal static void run(Action f) {
    f();
}

internal static void n1() {
    run(() => {
        counter c = default!;
        c.add(3);
        c.add(4);
        fmt.Println((@string)"N1:"u8, c.n, c.tag);
    });
}

internal static void n2() {
    run(() => {
        ref var c = ref heap(new counter(), out var Ꮡc);
        var p = Ꮡc;
        p.Value.n = 10;
        writeThrough(ref (p).DerefOrNull(), 5);
        fmt.Println((@string)"N2:"u8, c.n);
    });
}

internal static void n3() {
    run(() => {
        ref var c = ref heap(new counter(), out var Ꮡc);
        void inner(nint k) {
            Ꮡc.Value.n += k;
        }
        inner(7);
        inner(8);
        fmt.Println((@string)"N3:"u8, Ꮡc.Value.n);
    });
}

internal static void n4() {
    ref var c = ref heap(new counter(), out var Ꮡc);
    run(() => {
        Ꮡc.Value.n += 9;
    });
    fmt.Println((@string)"N4:"u8, c.n);
}

internal static void n5() {
    run(() => {
        ref var a = ref heap(new array<nint>(3), out var Ꮡa);
        var q = Ꮡa.at<nint>(1);
        q.Value = 42;
        fmt.Println((@string)"N5:"u8, a);
    });
}

internal static void n6() {
    run(() => {
        ref var c = ref heap(new counter(), out var Ꮡc);
        writeThrough(ref (Ꮡc).DerefOrNull(), 11);
        fmt.Println((@string)"N6:"u8, c.n);
    });
}

internal static void n7() {
    run(() => {
        counter c = default!;
        c.n = 5;
        var d = c;
        d.n = 6;
        fmt.Println((@string)"N7:"u8, c.n, d.n);
    });
}

internal static void n8() {
    run(() => {
        ref var c = ref heap(new counter(), out var Ꮡc);
        var bump = Ꮡc.add;
        bump(2);
        bump(3);
        fmt.Println((@string)"N8:"u8, c.n, c.tag);
    });
}

internal static void Main() {
    n1();
    n2();
    n3();
    n4();
    n5();
    n6();
    n7();
    n8();
}

} // end main_package

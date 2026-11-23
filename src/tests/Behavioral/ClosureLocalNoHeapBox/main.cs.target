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

internal static void writeThrough(ref counter p, nint k) { p.n += k; }

internal static void run(Action f) { f(); }

internal static void n1() {
    run([GoFunc("n1.func1")] () => {
        counter c = default;
        c.add(3);
        c.add(4);
        fmt.Println("N1:", c.n, c.tag);
    });
}

internal static void n2() {
    run([GoFunc("n2.func1")] () => {
        ref var c = ref heap(new counter(), out var Ꮡc);
        var p = Ꮡc;
        p.n = 10;
        writeThrough(ref (p).DerefOrNull(), 5);
        fmt.Println("N2:", c.n);
    });
}

internal static void n3() {
    run([GoFunc("n3.func1")] () => {
        ref var c = ref heap(new counter(), out var Ꮡc);
        [GoFunc("n3.func1.1")] void inner(nint k) {
            Ꮡc.Value.n += k;
        }
        inner(7);
        inner(8);
        fmt.Println("N3:", Ꮡc.Value.n);
    });
}

internal static void n4() {
    ref var c = ref heap(new counter(), out var Ꮡc);
    run([GoFunc("n4.func1")] () => {
        Ꮡc.Value.n += 9;
    });
    fmt.Println("N4:", c.n);
}

internal static void n5() {
    run([GoFunc("n5.func1")] () => {
        ref var a = ref heap(new array<nint>(3), out var Ꮡa);
        var q = Ꮡa.at<nint>(1);
        q.Value = 42;
        fmt.Println("N5:", a);
    });
}

internal static void n6() {
    run([GoFunc("n6.func1")] () => {
        ref var c = ref heap(new counter(), out var Ꮡc);
        writeThrough(ref (Ꮡc).DerefOrNull(), 11);
        fmt.Println("N6:", c.n);
    });
}

internal static void n7() {
    run([GoFunc("n7.func1")] () => {
        counter c = default;
        c.n = 5;
        var d = c;
        d.n = 6;
        fmt.Println("N7:", c.n, d.n);
    });
}

internal static void n8() {
    run([GoFunc("n8.func1")] () => {
        ref var c = ref heap(new counter(), out var Ꮡc);
        var bump = Ꮡc.add;
        bump(2);
        bump(3);
        fmt.Println("N8:", c.n, c.tag);
    });
}

internal static void main() {
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

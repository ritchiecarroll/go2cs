namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var x = (uint64)0b1011010;
    (x, var frac) = ((x >> (int)(3)), (uint64)(x & 7));
    fmt.Println(x, frac);
    nint a = 10;
    nint b = 20;
    (a, b, nint c) = (b, a, a + b);
    fmt.Println(a, b, c);
    nint p = 1;
    nint q = 2;
    nint r = 3;
    (p, q, r, nint s) = (q, r, p, p + q + r);
    fmt.Println(p, q, r, s);
    nint m = 5;
    m = m + 1;
    nint n = 100;
    fmt.Println(m, n);
    var e = Ꮡ(new edge(@out: 7, arg: 9));
    (e.Value.@out, e.Value.arg) = (e.Value.arg, e.Value.@out);
    fmt.Println((~e).@out, (~e).arg);
    var f = new edge(@out: 1, arg: 2);
    var g = new edge(@out: 3, arg: 4);
    (f.@out, g.@out, f.arg) = (g.@out, f.arg, f.@out);
    fmt.Println(f.@out, f.arg, g.@out, g.arg);
    (left, right) = (right, left);
    fmt.Println(left, right);
}

partial struct edge {
    internal nint @out;
    internal nint arg;
}

internal static nint left = 10;

internal static nint right = 20;

} // end main_package

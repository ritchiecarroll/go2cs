namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct holder {
    internal array<nint> arr = new(4);
}

internal static array<nint> pkg = new(4);

internal static void set(ref array<nint> p) { p[0] = 50; }

internal static void main() {
    array<nint> a = new(4);
    var s = a[..];
    a = new nint[]{1, 2, 3, 4}.array();
    fmt.Println("slice reads:", s[0]);
    s[1] = 20;
    fmt.Println("slice writes:", a[1]);

    ref var b = ref heap(new array<nint>(4), out var Ꮡb);
    var p = Ꮡb.at<nint>(0);
    b = new nint[]{1, 2, 3, 4}.array();
    p.Value = 30;
    fmt.Println("element pointer:", b[0], p.Value);

    ref var c = ref heap(new array<nint>(4), out var Ꮡc);
    var q = Ꮡc;
    c = new nint[]{1, 2, 3, 4}.array();
    q.Value[0] = 40;
    fmt.Println("array pointer:", c[0]);
    set(ref c);
    fmt.Println("array pointer argument:", c[0]);

    array<nint> d = new(4);
    array<nint> other = new(4);
    other[0] = 9;
    var t = d[..];
    d = other.Clone();
    t[0] = 60;
    fmt.Println("from a variable:", d[0], other[0]);

    holder h = new();
    var u = h.arr[..];
    h.arr = new nint[]{1, 2, 3, 4}.array();
    u[0] = 70;
    fmt.Println("field:", h.arr[0]);

    var v = pkg[..];
    pkg = new nint[]{1, 2, 3, 4}.array();
    v[0] = 80;
    fmt.Println("package:", pkg[0]);

    ref var e = ref heap(new array<nint>(4), out var Ꮡe);
    var w = e[..];
    var r = Ꮡe;
    r.Value = new nint[]{1, 2, 3, 4}.array();
    w[0] = 90;
    fmt.Println("through a pointer:", e[0]);
}

} // end main_package

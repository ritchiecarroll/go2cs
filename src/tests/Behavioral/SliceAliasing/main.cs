namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void run() {
    var @base = new slice<uint32>(6);
    var d = @base[2..5];
    copy(d, new uint32[]{7, 8, 9}.slice());
    fmt.Println(@base, d, len(d), cap(d));
    d[0] = 42;
    @base[3] = 43;
    fmt.Println(@base[2], d[1]);
    var e = d[1..3];
    e[0] = 99;
    fmt.Println(@base, len(e), cap(e));
    var s = new slice<nint>(3, 10);
    var t = s[1..];
    fmt.Println(len(t), cap(t));
    var u = s[1..8];
    u[6] = 5;
    fmt.Println(len(u), cap(u), u[6]);
    var arr = new nint[]{0, 10, 20, 30, 40, 50}.array();
    var f = arr[2..5];
    var g = f[1..3];
    g[0] = 31;
    fmt.Println(arr, g[0], len(g), cap(g));
    var h = @base.slice(1, 3, 4);
    h[0] = 77;
    fmt.Println(len(h), cap(h), @base[1]);
    var w = @base[2..4];
    var x = append(w, (uint32)(500));
    x[0] = 501;
    fmt.Println(@base, len(x), cap(x));
    var y = append(x, (uint32)(1), (uint32)(2), (uint32)(3));
    y[0] = 999;
    fmt.Println(@base[2], y[0], len(y), cap(y));
    var z = @base.slice(1, 3, 3);
    var grown = append(z, (uint32)(111));
    fmt.Println(@base[3], grown[2], len(grown), cap(grown));
}

internal static void Main() {
    run();
}

} // end main_package

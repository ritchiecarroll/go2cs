global using strs = go.slice<go.@string>;

namespace go;

using fmt = fmt_package;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

partial struct box {
    internal slice<nint> data;
    internal slice<@string> tags;
}

partial struct IntSlice /*[]nint*/;
// type List[T any] = []T

internal static nint sum(slice<nint> xs) {
    nint total = 0;
    foreach (var (_, x) in range(xs)) {
        total += x;
    }
    return total;
}

internal static slice<nint> pair() {
    return [1, 2];
}

internal static T first<T>(slice<T> xs) {
    return xs[0];
}

internal static nint count(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.sslice();

    return len(xs);
}

internal static void main() {
    var x = slice<nint>([1, 2, 3]);
    slice<@string> y = slice<@string>(["a", "b"]);
    fmt.Println(x, len(x), cap(x), y);

    var e = slice<nint>([]);
    slice<nint> n = default;
    fmt.Printf("empty: %v nil=%v len=%d cap=%d\n"u8, e, e == nil, len(e), cap(e));
    fmt.Printf("nil:   %v nil=%v len=%d cap=%d\n"u8, n, n == nil, len(n), cap(n));

    var (a, b) = (pair(), pair());
    a[0] = 99;
    fmt.Println("fresh:", a, b);

    x = [4, 5];
    fmt.Println("assigned:", x, len(x), cap(x));

    ref var bx = ref heap<box>(new box(data: [4, 5], tags: ["t"]), out var Ꮡbx);
    var p = Ꮡbx;
    p.data = [6, 7, 8];
    fmt.Println("box:", bx.data, bx.tags);

    var m = new map<@string, slice<nint>>{["k"] = [1, 2]};
    var grid = slice<slice<nint>>([[1, 2], [3]]);
    fmt.Println("map:", m["k"], "grid:", grid, len(grid[1]));

    fmt.Println("sum:", sum([1, 2, 3]), "first:", first<nint>(slice<nint>([9, 8])));

    fmt.Printf("%T %T\n"u8, x, grid);

    fmt.Println("any:", slice<nint>([1]));
    fmt.Println("inferred:", first(slice<nint>([7, 8])), len(slice<nint>([1, 2])), append(slice<nint>([1]), (nint)2));
    foreach (var (_, v) in range(slice<nint>([10, 20]))) {
        fmt.Print(v, " ");
    }
    fmt.Println(count(slice<nint>([1, 2, 3]).ꓸꓸꓸ));
    var named = new IntSlice(new nint[]{3, 1}.slice());
    fmt.Println("named:", named, len(named));

    strs ss = slice<@string>(["a", "b"]);
    var ls = slice<nint>([4, 5, 6]);
    var rows = array<IntSlice>([slice<nint>([1, 2]), slice<nint>([3])]);
    var list = slice<IntSlice>([slice<nint>([7]), slice<nint>([8, 9])]);
    fmt.Printf("alias: %v %T %v %T %v %T %v %T\n"u8, ss, ss, ls, ls, rows, rows[1], list, list[0]);

    var keyed = new slice<nint>(3){[2] = 5};
    var arr = array<nint>([1, 2, 3]);
    var pairs = GoReflect.WithElemDims(new array<nint>[]{array<nint>([1, 2]), array<nint>([3, 4])}.slice(), 2);
    fmt.Println("kept:", keyed, arr, pairs, len(pairs[1]));
}

} // end main_package

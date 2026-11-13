global using triple = go.array<nint>;

namespace go;

using fmt = fmt_package;
using runtime = runtime_package;

partial class main_package {

partial struct digits /*[3]nint*/;

partial struct cell {
    internal array<bool> marks = new(2);
}

internal static array<nint> primes = array<nint>([2, 3, 5, 7, 11, 13]);

internal static array<byte> table = array<byte>([0x10, 0x20, 0x30, 0x40]);

internal static partial nint line() {
    var (_, _, n, _) = runtime.Caller(1);
    return n;
}

internal static array<nint> fresh() {
    return array<nint>([1, 2, 3]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object oneˢ = (@string)"one"u8;

internal static void main() {
    fmt.Println(primes, len(primes));
    var @short = array<byte>(8, [1, 2]);
    fmt.Println(@short, len(@short));
    var dots = array<@string>(["a", "b", "c"]);
    fmt.Println(dots, len(dots));
    fmt.Println(table[3]);

    var (a, b) = (fresh(), fresh());
    a[0] = 9;
    fmt.Println(a, b);

    triple t = array<nint>([4, 5, 6]);
    var d = new digits(new nint[]{7, 8, 9}.array());
    fmt.Println(t, d);

    var keyed = new array<nint>(5){[1] = 10, [3] = 30};
    var empty = new nint[]{}.array(4);
    var mixed = array<any>([(nint)1, oneˢ]);
    fmt.Println(keyed, empty, mixed);

    var grid = new array<nint>[]{array<nint>([1, 2, 3]), array<nint>(3, [4])}.array();
    fmt.Println(grid, len(grid[1]));

    var rows = GoReflect.WithElemDims(new array<@string>[]{array<@string>(["x", "y"]), array<@string>(2, ["z"])}.slice(), 2);
    var ptrs = slice<ж<array<byte>>>([Ꮡ(array<byte>(4, [1])), Ꮡ(array<byte>(4, [2, 3]))]);
    fmt.Println(rows, len(rows[1]), ptrs[0].Value, ptrs[1].Value);

    var cells = new cell[]{new(marks: array<bool>([true, false]))}.array(3, () => new());
    fmt.Println(cells[0].marks, cells[2].marks, len(cells[2].marks));

    var lines = array<nint>([
        line(),
        line()
    ]);
    fmt.Println(lines[1] - lines[0]);
}

} // end main_package

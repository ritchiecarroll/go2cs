global using triple = go.array<nint>;

namespace go;

using fmt = fmt_package;
using runtime = runtime_package;

partial class main_package {

partial struct digits /*[3]nint*/;

partial struct cell {
    internal array<bool> marks = new(2);
}

internal static array<nint> primes = new nint[]{2, 3, 5, 7, 11, 13}.array();

internal static array<byte> table = new byte[]{0x10, 0x20, 0x30, 0x40}.array();

internal static partial nint line() {
    var (_, _, n, _) = runtime.Caller(1);
    return n;
}

internal static array<nint> fresh() {
    return new nint[]{1, 2, 3}.array();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object oneˢ = (@string)"one"u8;

internal static void main() {
    fmt.Println(primes, len(primes));
    var @short = new byte[]{1, 2}.array(8);
    fmt.Println(@short, len(@short));
    var dots = new @string[]{"a", "b", "c"}.array();
    fmt.Println(dots, len(dots));
    fmt.Println(table[3]);

    var (a, b) = (fresh(), fresh());
    a[0] = 9;
    fmt.Println(a, b);

    triple t = new nint[]{4, 5, 6}.array();
    var d = new digits(new nint[]{7, 8, 9}.array());
    fmt.Println(t, d);

    var keyed = new array<nint>(5){[1] = 10, [3] = 30};
    var empty = new nint[]{}.array(4);
    var mixed = new any[]{(nint)1, oneˢ}.array();
    fmt.Println(keyed, empty, mixed);

    var grid = new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4}.array(3)}.array();
    fmt.Println(grid, len(grid[1]));

    var rows = GoReflect.WithElemDims(new array<@string>[]{new @string[]{"x", "y"}.array(), new @string[]{"z"}.array(2)}.slice(), 2);
    var ptrs = slice<ж<array<byte>>>([Ꮡ(new byte[]{1}.array(4)), Ꮡ(new byte[]{2, 3}.array(4))]);
    fmt.Println(rows, len(rows[1]), ptrs[0].Value, ptrs[1].Value);

    var cells = new cell[]{new(marks: new bool[]{true, false}.array())}.array(3, () => new());
    fmt.Println(cells[0].marks, cells[2].marks, len(cells[2].marks));

    var lines = new nint[]{
        line(),
        line()
    }.array();
    fmt.Println(lines[1] - lines[0]);
}

} // end main_package

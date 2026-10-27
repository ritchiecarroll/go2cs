namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct sub {
    internal nint mu, n;
}

partial struct item {
    internal sub inner;
}

internal static readonly ж<array<item>> Ꮡpool = new StaticBox<array<item>>(new array<item>(3));
internal static ref array<item> pool => ref ((StaticBox<array<item>>)Ꮡpool).ValueSlot;


partial struct gridᴛ1 /*dyn*/ {
    internal sub cell;
    internal array<byte> pad = new(4);
}
internal static readonly ж<array<gridᴛ1>> Ꮡgrid = new StaticBox<array<gridᴛ1>>(new array<gridᴛ1>(3, () => new()));
internal static ref array<gridᴛ1> grid => ref ((StaticBox<array<gridᴛ1>>)Ꮡgrid).ValueSlot;

internal static partial void setInt(ref nint p) {
    p = 7;
}

internal static void Main() {
    pool[1].inner.n = 5;
    setInt(ref pool[1].inner.mu);
    setInt(ref pool[2].inner.mu);

    grid[0].cell.n = 9;
    setInt(ref grid[0].cell.mu);

    fmt.Println(pool[1].inner.mu, pool[1].inner.n, pool[2].inner.mu);
    fmt.Println(grid[0].cell.mu, grid[0].cell.n);
}

} // end main_package

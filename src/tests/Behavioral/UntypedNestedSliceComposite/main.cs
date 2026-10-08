namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct rank /*num:nint*/;

internal static rank rA => /* iota */ 0;
internal static rank rB => 1;
internal static rank rC => 2;

internal static slice<slice<rank>> order = new golib.SparseArray<slice<rank>>{
    [(int)rA] = new rank[]{}.slice(),
    [(int)rB] = new rank[]{rA}.slice(),
    [(int)rC] = new rank[]{rA, rB}.slice()
}.slice();

internal static slice<array<nint>> grid = GoReflect.WithElemDims(new array<nint>[]{
    new nint[]{1, 2}.array(),
    new nint[]{3, 4}.array()
}.slice(), 2);

partial struct dbgVar {
    internal @string name;
    internal ж<int32> value;
}

internal static ж<int32> Ꮡx = new StandardBox<int32>(7);
internal static ref int32 x => ref Ꮡx.Value;

internal static slice<ж<dbgVar>> dbgvars = new ж<dbgVar>[]{
    Ꮡ(new dbgVar(name: "a"u8, value: Ꮡx)),
    Ꮡ(new dbgVar(name: "b"u8, value: Ꮡx))
}.slice();

partial struct js /*num:nint*/;

internal static js j0 => /* iota */ 0;
internal static js j1 => 1;
internal static js j2 => 2;
internal static js j3 => 3;
internal static js numJS => 4;

internal static slice<array<js>> jsTable = GoReflect.WithElemDims(new golib.SparseArray<array<js>>{
    [(int)j0] = new golib.SparseArray<js>{[(int)j1] = j2, [(int)j3] = j1}.array(4),
    [(int)j2] = new golib.SparseArray<js>{[(int)j0] = j3, [(int)j3] = j2}.array(4)
}.slice(), 4);

internal static slice<slice<@string>> sparseRows = new slice<@string>[]{
    new golib.SparseArray<@string>{[2] = "two"u8, [0] = "zero"u8}.slice(),
    new golib.SparseArray<@string>{[1] = "one"u8}.slice()
}.slice();

internal static void Main() {
    fmt.Println(len(order[rA]), len(order[rB]), len(order[rC]));
    fmt.Println(order[rC][0], order[rC][1]);
    fmt.Println(grid[0][1], grid[1][0]);
    fmt.Println((~dbgvars[0]).name, (~dbgvars[1]).value.Value, len(dbgvars));
    fmt.Println(jsTable[j0][j1], jsTable[j0][j2], jsTable[j0][j3]);
    fmt.Println(jsTable[j2][j0], jsTable[j2][j3]);
    fmt.Println(sparseRows[0][0], sparseRows[0][2], len(sparseRows[0]));
    fmt.Println(sparseRows[1][1], len(sparseRows[1]));
}

} // end main_package

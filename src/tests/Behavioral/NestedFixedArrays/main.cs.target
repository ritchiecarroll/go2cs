namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct row /*[4]byte*/;

partial struct inner {
    internal array<byte> b = new(3);
}

partial struct holder {
    internal array<array<byte>> grid = new(2, () => new(3));
}

internal static array<array<byte>> gNested = new(2, () => new(4));

internal static array<array<array<byte>>> gDeep = new(2, () => new(3, () => new(4)));

internal static array<inner> gStructElem = new(2, () => new());

internal static void Main() {
    array<array<byte>> x = new(2, () => new(4));
    fmt.Println(len(x), len(x[0]), len(x[1]));
    x[1][2] = 7;
    x[0][3] = 9;
    fmt.Println(x[1][2], x[0][3], x[0][2]);
    array<array<array<byte>>> deep = new(2, () => new(3, () => new(4)));
    fmt.Println(len(deep), len(deep[1]), len(deep[1][2]));
    deep[1][2][3] = 5;
    fmt.Println(deep[1][2][3], deep[0][0][0]);
    holder h = new();
    fmt.Println(len(h.grid), len(h.grid[1]));
    h.grid[1][2] = 4;
    fmt.Println(h.grid[1][2], h.grid[0][2]);
    array<inner> se = new(2, () => new());
    fmt.Println(len(se), len(se[1].b));
    se[1].b[2] = 3;
    fmt.Println(se[1].b[2], se[0].b[2]);
    array<row> nr = new(2);
    fmt.Println(len(nr), len(nr[1]));
    nr[1][3] = 6;
    fmt.Println(nr[1][3], nr[0][3]);
    fmt.Println(len(gNested), len(gNested[1]));
    gNested[1][2] = 8;
    fmt.Println(gNested[1][2], gNested[0][2]);
    fmt.Println(len(gDeep), len(gDeep[1]), len(gDeep[1][2]));
    fmt.Println(len(gStructElem), len(gStructElem[1].b));
    array<array<byte>> share = new(3, () => new(2));
    share[0][0] = 1;
    share[1][0] = 2;
    share[2][0] = 3;
    fmt.Println(share[0][0], share[1][0], share[2][0]);
    ref var boxed = ref heap(new array<array<int32>>(3, () => new(4)), out var Ꮡboxed);
    var pb = Ꮡboxed;
    fmt.Println(3, len(pb.Value[1]));
    pb.Value[1][2] = 11;
    pb.Value[2][3] = 12;
    fmt.Println(pb.Value[1][2], pb.Value[2][3], pb.Value[0][2]);
    pb.Value[0][0] = 1;
    pb.Value[0][1] = 2;
    copy(pb.Value[2][..2], pb.Value[0][..2]);
    fmt.Println(pb.Value[2][0], pb.Value[2][1]);
    ref var boxedStruct = ref heap(new array<inner>(2, () => new()), out var ᏑboxedStruct);
    var ps = ᏑboxedStruct;
    fmt.Println(2, len(ps.Value[1].b));
    ps.Value[1].b[2] = 13;
    fmt.Println(ps.Value[1].b[2], ps.Value[0].b[2]);
    var np = Ꮡ(new array<nint>(12));
    fmt.Println(12, len(np.Value));
    np.Value[3] = 14;
    fmt.Println(np.Value[3], np.Value[0]);
    var nq = Ꮡ(new array<array<nint>>(2, () => new(3)));
    nq.Value[1][2] = 15;
    fmt.Println(2, len(nq.Value[0]), nq.Value[1][2], nq.Value[0][2]);
    var ns = Ꮡ(new array<inner>(2, () => new()));
    ns.Value[1].b[2] = 16;
    fmt.Println(2, len(ns.Value[1].b), ns.Value[1].b[2], ns.Value[0].b[2]);
    var nr2 = @new<row>();
    nr2.Value[3] = 17;
    fmt.Println(4, nr2.Value[3], nr2.Value[0]);
    var grid = GoReflect.WithElemDims(new slice<array<int32>>(3, () => new(4)), 4);
    fmt.Println(len(grid), len(grid[0]), len(grid[2]));
    grid[1][2] = 18;
    grid[2][3] = 19;
    fmt.Println(grid[1][2], grid[2][3], grid[0][2]);
    var capped = GoReflect.WithElemDims(new slice<array<nint>>(1, () => new(2), 3), 2);
    capped = GoReflect.WithElemDims(capped[..3], 2);
    capped[2][1] = 20;
    fmt.Println(len(capped), cap(capped), len(capped[2]), capped[2][1], capped[0][1]);
    var ms = new slice<inner>(2, () => new());
    ms[1].b[2] = 21;
    fmt.Println(len(ms), len(ms[1].b), ms[1].b[2], ms[0].b[2]);
    var mr = GoReflect.WithElemDims(new slice<row>(2), 4);
    mr[1][3] = 22;
    fmt.Println(len(mr), len(mr[1]), mr[1][3], mr[0][3]);
}

} // end main_package

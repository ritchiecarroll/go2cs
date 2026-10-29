global using aliased = go.array<byte>;

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct named /*[6]byte*/;

partial struct cell {
    public array<uint8> Buf = new(4);
    public @string Tag;
}

internal static UntypedInt idx => 2;

internal static array<byte> pkgEmpty = new byte[]{}.array(8);

internal static array<byte> pkgPartial = new byte[]{1, 2}.array(8);

internal static array<array<uint8>> pkgNested = new array<uint8>[]{}.array(2, () => new(3));

internal static void Main() {
    var empty = new byte[]{}.array(8);
    fmt.Println("empty", len(empty), empty[0], empty[7]);

    var partial = new byte[]{1, 2}.array(8);
    fmt.Println("partial", len(partial), partial[0], partial[1], partial[2], partial[7]);

    var full = new byte[]{1, 2, 3}.array();
    fmt.Println("full", len(full), full[0], full[2]);

    var ellipsis = new byte[]{4, 5, 6}.array();
    fmt.Println("ellipsis", len(ellipsis), ellipsis[2]);

    var keyed = new array<byte>(8){[5] = 1};
    fmt.Println("keyed", len(keyed), keyed[5], keyed[0], keyed[7]);

    var keyedZero = new array<byte>(8){[0] = 9};
    fmt.Println("keyedZero", len(keyedZero), keyedZero[0], keyedZero[7]);

    fmt.Println("named empty", len(new named(new byte[6].array())));
    var np = new named(new byte[]{1, 2}.array(6));
    fmt.Println("named partial", len(np), np[0], np[5]);

    fmt.Println("alias empty", len(new byte[]{}.array(5)));
    var ap = new byte[]{9}.array(5);
    fmt.Println("alias partial", len(ap), ap[0], ap[4]);

    fmt.Println("pkg", len(pkgEmpty), len(pkgPartial), pkgPartial[1], pkgPartial[7]);

    var ints = new nint[]{7}.array(4);
    fmt.Println("ints", len(ints), ints[0], ints[3]);

    var strs = new @string[]{"a"}.array(3);
    fmt.Println("strs", len(strs), strs[0], strs[2] == "");

    fmt.Println("slice ctl", len(slice<byte>([])), len(slice<byte>([1, 2])));

    var w = new byte[]{1}.array(8);
    w[7] = 42;
    fmt.Println("write", len(w), w[0], w[6], w[7]);

    var nestedEmpty = new array<uint8>[]{}.array(2, () => new(3));
    fmt.Println("nested empty", len(nestedEmpty), len(nestedEmpty[0]), len(nestedEmpty[1]));
    nestedEmpty[1][2] = 7;
    fmt.Println("nested empty write", nestedEmpty[1][2], nestedEmpty[0][2]);

    var nestedPartial = new array<uint8>[]{new uint8[]{1, 2, 3}.array()}.array(2, () => new(3));
    fmt.Println("nested partial", len(nestedPartial), len(nestedPartial[0]), len(nestedPartial[1]), nestedPartial[0][1], nestedPartial[1][1]);

    var nestedFull = new array<uint8>[]{new uint8[]{1, 2, 3}.array(), new uint8[]{4, 5, 6}.array()}.array();
    fmt.Println("nested full", len(nestedFull), len(nestedFull[1]), nestedFull[1][2]);

    var deep = new array<array<uint8>>[]{}.array(2, () => new(3, () => new(4)));
    fmt.Println("deep", len(deep), len(deep[1]), len(deep[1][2]));
    deep[1][2][3] = 9;
    fmt.Println("deep write", deep[1][2][3]);

    var namedElems = new named[]{}.array(2);
    fmt.Println("named elems", len(namedElems), len(namedElems[0]), len(namedElems[1]));

    var cells = new cell[]{}.array(2, () => new());
    fmt.Println("cells", len(cells), len(cells[0].Buf), len(cells[1].Buf));
    cells[1].Buf[3] = 5;
    fmt.Println("cells write", cells[1].Buf[3], cells[0].Buf[3], cells[0].Tag == ""u8);

    var keyedNested = new array<array<uint8>>(4, () => new(3)){[1] = new uint8[]{1, 2, 3}.array()};
    fmt.Println("keyed nested", len(keyedNested), len(keyedNested[0]), len(keyedNested[1]), len(keyedNested[3]), keyedNested[1][2]);

    var sparseNested = new golib.SparseArray<array<uint8>>{[idx] = new uint8[]{4, 5, 6}.array()}.array(4, () => new(3));
    fmt.Println("sparse nested", len(sparseNested), len(sparseNested[0]), len(sparseNested[idx]), len(sparseNested[3]), sparseNested[idx][1]);
    sparseNested[3][2] = 8;
    fmt.Println("sparse nested write", sparseNested[3][2], sparseNested[0][2]);

    fmt.Println("pkg nested", len(pkgNested), len(pkgNested[0]), len(pkgNested[1]));
}

} // end main_package

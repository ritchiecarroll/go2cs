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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object emptyˢ = (@string)"empty"u8;
private static readonly object partialˢ = (@string)"partial"u8;
private static readonly object fullˢ = (@string)"full"u8;
private static readonly object ellipsisˢ = (@string)"ellipsis"u8;
private static readonly object keyedˢ = (@string)"keyed"u8;
private static readonly object keyedZeroˢ = (@string)"keyedZero"u8;
private static readonly object namedEmptyˢ = (@string)"named empty"u8;
private static readonly object namedPartialˢ = (@string)"named partial"u8;
private static readonly object aliasEmptyˢ = (@string)"alias empty"u8;
private static readonly object aliasPartialˢ = (@string)"alias partial"u8;
private static readonly object pkgˢ = (@string)"pkg"u8;
private static readonly object intsˢ = (@string)"ints"u8;
private static readonly object strsˢ = (@string)"strs"u8;
private static readonly object sliceCtlˢ = (@string)"slice ctl"u8;
private static readonly object writeˢ = (@string)"write"u8;
private static readonly object nestedEmptyˢ = (@string)"nested empty"u8;
private static readonly object nestedEmptyWriteˢ = (@string)"nested empty write"u8;
private static readonly object nestedPartialˢ = (@string)"nested partial"u8;
private static readonly object nestedFullˢ = (@string)"nested full"u8;
private static readonly object deepˢ = (@string)"deep"u8;
private static readonly object deepWriteˢ = (@string)"deep write"u8;
private static readonly object namedElemsˢ = (@string)"named elems"u8;
private static readonly object cellsˢ = (@string)"cells"u8;
private static readonly object cellsWriteˢ = (@string)"cells write"u8;
private static readonly object keyedNestedˢ = (@string)"keyed nested"u8;
private static readonly object sparseNestedˢ = (@string)"sparse nested"u8;
private static readonly object sparseNestedWriteˢ = (@string)"sparse nested write"u8;
private static readonly object pkgNestedˢ = (@string)"pkg nested"u8;

internal static void Main() {
    var empty = new byte[]{}.array(8);
    fmt.Println(emptyˢ, len(empty), empty[0], empty[7]);
    var partial = new byte[]{1, 2}.array(8);
    fmt.Println(partialˢ, len(partial), partial[0], partial[1], partial[2], partial[7]);
    var full = new byte[]{1, 2, 3}.array();
    fmt.Println(fullˢ, len(full), full[0], full[2]);
    var ellipsis = new byte[]{4, 5, 6}.array();
    fmt.Println(ellipsisˢ, len(ellipsis), ellipsis[2]);
    var keyed = new array<byte>(8){[5] = 1};
    fmt.Println(keyedˢ, len(keyed), keyed[5], keyed[0], keyed[7]);
    var keyedZero = new array<byte>(8){[0] = 9};
    fmt.Println(keyedZeroˢ, len(keyedZero), keyedZero[0], keyedZero[7]);
    fmt.Println(namedEmptyˢ, len(new named(new byte[6].array())));
    var np = new named(new byte[]{1, 2}.array(6));
    fmt.Println(namedPartialˢ, len(np), np[0], np[5]);
    fmt.Println(aliasEmptyˢ, len(new byte[]{}.array(5)));
    var ap = new byte[]{9}.array(5);
    fmt.Println(aliasPartialˢ, len(ap), ap[0], ap[4]);
    fmt.Println(pkgˢ, len(pkgEmpty), len(pkgPartial), pkgPartial[1], pkgPartial[7]);
    var ints = new nint[]{7}.array(4);
    fmt.Println(intsˢ, len(ints), ints[0], ints[3]);
    var strs = new @string[]{"a"u8}.array(3);
    fmt.Println(strsˢ, len(strs), strs[0], strs[2] == "");
    fmt.Println(sliceCtlˢ, len(new byte[]{}.slice()), len(new byte[]{1, 2}.slice()));
    var w = new byte[]{1}.array(8);
    w[7] = 42;
    fmt.Println(writeˢ, len(w), w[0], w[6], w[7]);
    var nestedEmpty = new array<uint8>[]{}.array(2, () => new(3));
    fmt.Println(nestedEmptyˢ, len(nestedEmpty), len(nestedEmpty[0]), len(nestedEmpty[1]));
    nestedEmpty[1][2] = 7;
    fmt.Println(nestedEmptyWriteˢ, nestedEmpty[1][2], nestedEmpty[0][2]);
    var nestedPartial = new array<uint8>[]{new uint8[]{1, 2, 3}.array()}.array(2, () => new(3));
    fmt.Println(nestedPartialˢ, len(nestedPartial), len(nestedPartial[0]), len(nestedPartial[1]), nestedPartial[0][1], nestedPartial[1][1]);
    var nestedFull = new array<uint8>[]{new uint8[]{1, 2, 3}.array(), new uint8[]{4, 5, 6}.array()}.array();
    fmt.Println(nestedFullˢ, len(nestedFull), len(nestedFull[1]), nestedFull[1][2]);
    var deep = new array<array<uint8>>[]{}.array(2, () => new(3, () => new(4)));
    fmt.Println(deepˢ, len(deep), len(deep[1]), len(deep[1][2]));
    deep[1][2][3] = 9;
    fmt.Println(deepWriteˢ, deep[1][2][3]);
    var namedElems = new named[]{}.array(2);
    fmt.Println(namedElemsˢ, len(namedElems), len(namedElems[0]), len(namedElems[1]));
    var cells = new cell[]{}.array(2, () => new());
    fmt.Println(cellsˢ, len(cells), len(cells[0].Buf), len(cells[1].Buf));
    cells[1].Buf[3] = 5;
    fmt.Println(cellsWriteˢ, cells[1].Buf[3], cells[0].Buf[3], cells[0].Tag == ""u8);
    var keyedNested = new array<array<uint8>>(4, () => new(3)){[1] = new uint8[]{1, 2, 3}.array()};
    fmt.Println(keyedNestedˢ, len(keyedNested), len(keyedNested[0]), len(keyedNested[1]), len(keyedNested[3]), keyedNested[1][2]);
    var sparseNested = new golib.SparseArray<array<uint8>>{[idx] = new uint8[]{4, 5, 6}.array()}.array(4, () => new(3));
    fmt.Println(sparseNestedˢ, len(sparseNested), len(sparseNested[0]), len(sparseNested[idx]), len(sparseNested[3]), sparseNested[idx][1]);
    sparseNested[3][2] = 8;
    fmt.Println(sparseNestedWriteˢ, sparseNested[3][2], sparseNested[0][2]);
    fmt.Println(pkgNestedˢ, len(pkgNested), len(pkgNested[0]), len(pkgNested[1]));
}

} // end main_package

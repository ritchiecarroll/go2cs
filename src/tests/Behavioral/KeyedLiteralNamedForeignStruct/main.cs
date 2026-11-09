namespace go;

using fmt = fmt_package;
using guid = KeyedLiteralNamedForeignStruct.guid_package;
using KeyedLiteralNamedForeignStruct;

partial class main_package {

[GoLazy] partial class lazyᴛid { internal static guid.GUID id = new guid.GUID(
    Data1: 0x25a207b9,
    Data2: 0xddf3,
    Data3: 0x4660,
    Data4: new byte[]{0x8e, 0xe9, 0x76, 0xe5, 0x8c, 0x74, 0x06, 0x3e}.array()
); }

internal static void Main() {
    fmt.Println(id.Data1, id.Data2, id.Data3, id.Data4[1], id.First());

    var partial = new guid.GUID(Data2: 9);
    fmt.Println(partial.Data1, partial.Data2, partial.Data4[7]);

    var p = Ꮡ(new guid.GUID(Data1: 5));
    p.Data2 = 6;
    fmt.Println(p.Data1, p.Data2, p.First());

    var list = slice<guid.GUID>([new(Data1: 10), new(Data3: 11)]);
    fmt.Println(len(list), list[0].Data1, list[1].Data3);

    var ptrs = slice<ж<guid.GUID>>([Ꮡ(new guid.GUID(Data1: 12))]);
    ptrs[0].Data2 = 13;
    fmt.Println(len(ptrs), ptrs[0].Data1, ptrs[0].Data2, ptrs[0].First());

    var copied = id.ΔClone();
    copied.Data1 = 1;
    fmt.Println(id.Data1 == 0x25a207b9, copied.Data1);

    var o = new guid.Outer(A: 7, B: new byte[]{1, 2}.array());
    fmt.Println(o.A, o.B[1], o.Sum());

    var (vals, refs) = guid.Pair();
    fmt.Println(len(vals), vals[0].A, vals[1].B[0], vals[1].Sum(), refs[0].A);

    var empty = new guid.GUID();
    var emptyPtr = Ꮡ(new guid.GUID());
    var emptyList = slice<guid.GUID>([new(), new(Data1: 1)]);
    var emptyPtrs = slice<ж<guid.GUID>>([Ꮡ(new guid.GUID())]);
    fmt.Println(empty.Data1, emptyPtr.Data4[0], len(emptyList), emptyList[0].Data2, emptyList[1].Data1, emptyPtrs[0].Data3);

    var (zero, zeros, zeroPtrs) = guid.Zeros();
    fmt.Println(zero.A, len(zeros), zeros[0].Sum(), zeroPtrs[0].B[1]);
}

} // end main_package

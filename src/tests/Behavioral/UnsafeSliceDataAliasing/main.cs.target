namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct item {
    internal @string key;
    internal nint val;
}

partial struct group {
    internal ж<item> ptr;
    internal nint n;
}

internal static group newGroup(slice<item> items) {
    return new group(ptr: @unsafe.SliceData(items), n: len(items));
}

internal static slice<item> items(this group g) {
    return @unsafe.Slice(g.ptr, g.n);
}

internal static void show(@string label, slice<item> items) {
    fmt.Print(label, ":");
    foreach (var (_, it) in items) {
        fmt.Print(" ", it.key, "=", it.val);
    }
    fmt.Println();
}

internal static void Main() {
    var items = new item[]{new("a", 1), new("b", 2), new("c", 3)}.slice();

    var g = newGroup(items);
    show("round trip", g.items());
    fmt.Println("len:", len(g.items()));

    g.items()[1].val = 20;
    show("after write through", items);

    fmt.Println("identity:", @unsafe.SliceData(items) == Ꮡ(items, 0));
    var tail = items[1..];
    fmt.Println("tail identity:", @unsafe.SliceData(tail) == Ꮡ(items, 1));
    fmt.Println("tail head:", (~@unsafe.SliceData(tail)).key);

    var nums = new int64[]{10, 20, 30}.slice();
    fmt.Println("int64 head:", @unsafe.SliceData(nums).Value);
    fmt.Println("int64 tail head:", @unsafe.SliceData(nums[2..]).Value);

    var ptrs = new ж<item>[]{Ꮡ(items, 0), Ꮡ(items, 2)}.slice();
    fmt.Println("ptr elem:", (~@unsafe.Slice(@unsafe.SliceData(ptrs), len(ptrs))[1]).key);

    var b = slice<byte>("hello"u8);
    fmt.Println("bytes:", (@string)@unsafe.Slice(@unsafe.SliceData(b), len(b)));
    fmt.Println("bytes tail:", (@string)@unsafe.Slice(@unsafe.SliceData(b[2..]), 3));
}

} // end main_package

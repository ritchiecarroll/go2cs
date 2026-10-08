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
    fmt.Print(label, (@string)":"u8);
    foreach (var (_, it) in items) {
        fmt.Print((@string)" "u8, it.key, (@string)"="u8, it.val);
    }
    fmt.Println();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string roundTripˢ = "round trip"u8;
private static readonly object lenˢ = (@string)"len:"u8;
private static readonly @string afterWriteThroughˢ = "after write through"u8;
private static readonly object identityˢ = (@string)"identity:"u8;
private static readonly object tailIdentityˢ = (@string)"tail identity:"u8;
private static readonly object tailHeadˢ = (@string)"tail head:"u8;
private static readonly object int64Headˢ = (@string)"int64 head:"u8;
private static readonly object int64TailHeadˢ = (@string)"int64 tail head:"u8;
private static readonly object ptrElemˢ = (@string)"ptr elem:"u8;
private static readonly object bytesˢ = (@string)"bytes:"u8;
private static readonly object bytesTailˢ = (@string)"bytes tail:"u8;

internal static void Main() {
    var items = new item[]{new("a"u8, 1), new("b"u8, 2), new("c"u8, 3)}.slice();
    var g = newGroup(items);
    show(roundTripˢ, g.items());
    fmt.Println(lenˢ, len(g.items()));
    g.items()[1].val = 20;
    show(afterWriteThroughˢ, items);
    fmt.Println(identityˢ, @unsafe.SliceData(items) == Ꮡ(items, 0));
    var tail = items[1..];
    fmt.Println(tailIdentityˢ, @unsafe.SliceData(tail) == Ꮡ(items, 1));
    fmt.Println(tailHeadˢ, (~@unsafe.SliceData(tail)).key);
    var nums = new int64[]{10, 20, 30}.slice();
    fmt.Println(int64Headˢ, @unsafe.SliceData(nums).Value);
    fmt.Println(int64TailHeadˢ, @unsafe.SliceData(nums[2..]).Value);
    var ptrs = new ж<item>[]{Ꮡ(items, 0), Ꮡ(items, 2)}.slice();
    fmt.Println(ptrElemˢ, (~@unsafe.Slice(@unsafe.SliceData(ptrs), len(ptrs))[1]).key);
    var b = slice<byte>("hello"u8);
    fmt.Println(bytesˢ, ((@string)@unsafe.Slice(@unsafe.SliceData(b), len(b))));
    fmt.Println(bytesTailˢ, ((@string)@unsafe.Slice(@unsafe.SliceData(b[2..]), 3)));
}

} // end main_package

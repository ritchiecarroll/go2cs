namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Stats_BySize /*dyn*/ {
    public uint32 Size;
    public uint64 Count;
}

partial struct Stats {
    public nint Total;
    public array<Stats_BySize> BySize = new(3);
}


partial struct poolᴛ1 /*dyn*/ {
    internal nint item;
    internal array<byte> pad = new(4);
}
internal static array<poolᴛ1> pool = new(2, () => new());

internal static array<nint> nums = new(3);

internal static ж<array<nint>> Ꮡaddr = new StandardBox<array<nint>>(new array<nint>(2));
internal static ref array<nint> addr => ref Ꮡaddr.Value;

partial struct Composed_Ptrs /*dyn*/ {
    public uint32 Size;
}

partial struct Composed_Slice /*dyn*/ {
    public @string Name;
}

partial struct Composed_ByKey /*dyn*/ {
    public nint Count;
}

partial interface Composed_Tagged /*dyn*/ {
    @string Tag();
}

partial struct Composed {
    public array<ж<Composed_Ptrs>> Ptrs = new(2);
    public slice<ж<Composed_Slice>> Slice;
    public map<@string, Composed_ByKey> ByKey;
    public map<@string, Composed_Tagged> Tagged;
}

internal static Composed composed = new();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string absentˢ = "absent"u8;

internal static (bool, nint, nint, bool) composedReads() {
    return (composed.Ptrs[0] == nil, len(composed.Slice), composed.ByKey[absentˢ].Count, composed.Tagged[absentˢ] == default!);
}

internal partial struct reservedIsNil_type /*dyn*/ {
    internal nint r7;
}

internal static bool reservedIsNil() {
    return ((ж<reservedIsNil_type>)nil) == nil;
}

internal static nint statsTotal() {
    var s = new Stats(Total: 42);
    return s.Total;
}

internal partial struct localHeapAnon_firstFree /*dyn*/ {
    internal nint @base, bound;
}

internal static nint localHeapAnon() {
    ref var firstFree = ref heap<localHeapAnon_firstFree>(out var ᏑfirstFree);
    firstFree = new localHeapAnon_firstFree(
        @base: 3,
        bound: 7
    );
    var p = ᏑfirstFree;
    p.Value.bound = 10;
    return firstFree.@base + firstFree.bound;
}

internal static void Main() {
    pool[0].item = 5;
    pool[1].item = 6;
    nums[0] = 11;
    nums[2] = 13;
    var p = Ꮡaddr;
    p.Value[0] = 21;
    p.Value[1] = 22;
    fmt.Println(pool[0].item, pool[1].item);
    fmt.Println(nums[0], nums[2], addr[0], addr[1]);
    fmt.Println(statsTotal());
    fmt.Println(localHeapAnon());
    var (noPtr, sliceLen, count, noTag) = composedReads();
    fmt.Println(noPtr, sliceLen, count, noTag);
    fmt.Println(reservedIsNil());
}

} // end main_package

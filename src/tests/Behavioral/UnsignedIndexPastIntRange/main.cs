namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct bytes /*[]byte*/;

partial struct path /*@string*/;

partial struct block /*[4]byte*/;

internal static void @try(@string name, Func<byte> f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Printf("%s: %v\n"u8, name, r);
                }
            }
        }, ref ᒐ);
        fmt.Printf("%s: read %d\n"u8, name, f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string stringUint64ˢ = "string uint64"u8;
private static readonly @string stringUintˢ = "string uint"u8;
private static readonly @string stringUintptrˢ = "string uintptr"u8;
private static readonly @string stringInt64ˢ = "string int64"u8;
private static readonly @string stringUint32ˢ = "string uint32"u8;
private static readonly @string namedStringUint64ˢ = "named string uint64"u8;
private static readonly @string sliceUintˢ = "slice uint"u8;
private static readonly @string sliceUintptrˢ = "slice uintptr"u8;
private static readonly @string sliceUint32ˢ = "slice uint32"u8;
private static readonly @string arrayUintˢ = "array uint"u8;
private static readonly @string arrayUintptrˢ = "array uintptr"u8;
private static readonly @string namedSliceUintˢ = "named slice uint"u8;
private static readonly @string namedArrayUintˢ = "named array uint"u8;
private static readonly @string sliceWriteUintˢ = "slice write uint"u8;
private static readonly @string pointerToArrayUintˢ = "pointer to array uint"u8;
private static readonly @string stringInRangeˢ = "string in range"u8;
private static readonly @string sliceInRangeˢ = "slice in range"u8;
private static readonly @string arrayInRangeˢ = "array in range"u8;
private static readonly @string namedSliceInRangeˢ = "named slice in range"u8;
private static readonly @string namedStringInRangeˢ = "named string in range"u8;
private static readonly @string namedArrayInRangeˢ = "named array in range"u8;
private static readonly @string sliceByteIndexˢ = "slice byte index"u8;

internal static void Main() {
    @string s = "0123456789"u8;
    var b = slice<byte>("abcdefghij"u8);
    ref var a = ref heap(new array<byte>(10), out var Ꮡa);
    copy(a[..], b);
    var nb = ((bytes)b);
    path np = ((path)s);
    ref var nk = ref heap<block>(out var Ꮡnk);
    nk = new block(new byte[]{1, 2, 3, 4}.array());
    var pa = Ꮡa;
    uint64 u64 = (uint64)(4294967296L + 5);
    uint32 u32 = unchecked((uint32)(4294967295UL));
    nuint u = unchecked((nuint)(9223372036854775810UL));
    uintptr up = unchecked((nuint)(9223372036854775811UL));
    int64 i64 = 4294967301L;
    uint32 in32 = 7;
    uint64 in64 = 2;
    uint8 b8 = 250;
    @try(stringUint64ˢ, () => s[u64]);
    @try(stringUintˢ, () => s[u]);
    @try(stringUintptrˢ, () => s[up]);
    @try(stringInt64ˢ, () => s[(nint)(i64)]);
    @try(stringUint32ˢ, () => s[u32]);
    @try(namedStringUint64ˢ, () => np[u64]);
    var bʗ1 = b;
    @try(sliceUintˢ, () => bʗ1[u]);
    var bʗ2 = b;
    @try(sliceUintptrˢ, () => bʗ2[up]);
    var bʗ3 = b;
    @try(sliceUint32ˢ, () => bʗ3[u32]);
    @try(arrayUintˢ, () => Ꮡa.Value[u]);
    @try(arrayUintptrˢ, () => Ꮡa.Value[up]);
    var nbʗ1 = nb;
    @try(namedSliceUintˢ, () => nbʗ1[u]);
    var nkʗ1 = nk;
    @try(namedArrayUintˢ, () => nkʗ1[u]);
    var bʗ4 = b;
    @try(sliceWriteUintˢ, () => {
        bʗ4[u] = 1;
        return 0;
    });
    var paʗ1 = pa;
    @try(pointerToArrayUintˢ, () => paʗ1.Value[u]);
    @try(stringInRangeˢ, () => s[in32]);
    var bʗ5 = b;
    @try(sliceInRangeˢ, () => bʗ5[in64]);
    @try(arrayInRangeˢ, () => Ꮡa.Value[in32]);
    var nbʗ2 = nb;
    @try(namedSliceInRangeˢ, () => nbʗ2[in64]);
    @try(namedStringInRangeˢ, () => np[in64]);
    var nkʗ2 = nk;
    @try(namedArrayInRangeˢ, () => nkʗ2[in64]);
    var bʗ6 = b;
    @try(sliceByteIndexˢ, () => bʗ6[b8]);
}

} // end main_package

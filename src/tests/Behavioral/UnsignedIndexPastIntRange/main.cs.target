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
                var r = recover(); if (r != nil) {
                    fmt.Printf("%s: %v\n"u8, name, r);
                }
            }
        }, ref ᒐ);
        fmt.Printf("%s: read %d\n"u8, name, f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    @string s = "0123456789";
    var b = slice<byte>("abcdefghij"u8);
    ref var a = ref heap(new array<byte>(10), out var Ꮡa);
    copy(a[..], b);
    var nb = (bytes)b;
    path np = (path)s;
    ref var nk = ref heap<block>(new block(new byte[]{1, 2, 3, 4}.array()), out var Ꮡnk);
    var pa = Ꮡa;

    uint64 u64 = (uint64)(4294967296L + 5);
    uint32 u32 = unchecked((uint32)(4294967295UL));
    nuint u = unchecked((nuint)(9223372036854775810UL));
    uintptr up = unchecked((nuint)(9223372036854775811UL));
    int64 i64 = 4294967301L;
    uint32 in32 = 7;
    uint64 in64 = 2;
    uint8 b8 = 250;

    @try("string uint64", () => s[u64]);
    @try("string uint", () => s[u]);
    @try("string uintptr", () => s[up]);
    @try("string int64", () => s[(nint)(i64)]);
    @try("string uint32", () => s[u32]);
    @try("named string uint64", () => np[u64]);

    var bʗ1 = b;
    @try("slice uint", () => bʗ1[u]);
    var bʗ2 = b;
    @try("slice uintptr", () => bʗ2[up]);
    var bʗ3 = b;
    @try("slice uint32", () => bʗ3[u32]);
    @try("array uint", () => Ꮡa.Value[u]);
    @try("array uintptr", () => Ꮡa.Value[up]);
    var nbʗ1 = nb;
    @try("named slice uint", () => nbʗ1[u]);
    var nkʗ1 = nk;
    @try("named array uint", () => nkʗ1[u]);
    var bʗ4 = b;
    @try("slice write uint", () => {
        bʗ4[u] = 1;
        return 0;
    });
    var paʗ1 = pa;
    @try("pointer to array uint", () => paʗ1.Value[u]);

    @try("string in range", () => s[in32]);
    var bʗ5 = b;
    @try("slice in range", () => bʗ5[in64]);
    @try("array in range", () => Ꮡa.Value[in32]);
    var nbʗ2 = nb;
    @try("named slice in range", () => nbʗ2[in64]);
    @try("named string in range", () => np[in64]);
    var nkʗ2 = nk;
    @try("named array in range", () => nkʗ2[in64]);
    var bʗ6 = b;
    @try("slice byte index", () => bʗ6[b8]);
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct bytes /*[]byte*/;
partial struct path /*@string*/;
partial struct block /*[4]byte*/;

internal static void @try(@string name, Func<byte> f) {
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("try.func1")] () => {
            var r = recover(); if (r != nil) {
                fmt.Printf("%s: %v\n"u8, name, r);
            }
        }, ref ᒐ);
        var evalᴛ1 = f();
        fmt.Printf("%s: read %d\n"u8, name, evalᴛ1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void main() {
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

    @try("string uint64", [GoFunc("main.func1")] () => s[u64]);
    @try("string uint", [GoFunc("main.func2")] () => s[u]);
    @try("string uintptr", [GoFunc("main.func3")] () => s[up]);
    @try("string int64", [GoFunc("main.func4")] () => s[(nint)(i64)]);
    @try("string uint32", [GoFunc("main.func5")] () => s[u32]);
    @try("named string uint64", [GoFunc("main.func6")] () => np[u64]);

    var bʗ1 = b;
    @try("slice uint", [GoFunc("main.func7")] () => bʗ1[u]);
    var bʗ2 = b;
    @try("slice uintptr", [GoFunc("main.func8")] () => bʗ2[up]);
    var bʗ3 = b;
    @try("slice uint32", [GoFunc("main.func9")] () => bʗ3[u32]);
    @try("array uint", [GoFunc("main.func10")] () => Ꮡa.Value[u]);
    @try("array uintptr", [GoFunc("main.func11")] () => Ꮡa.Value[up]);
    var nbʗ1 = nb;
    @try("named slice uint", [GoFunc("main.func12")] () => nbʗ1[u]);
    var nkʗ1 = nk;
    @try("named array uint", [GoFunc("main.func13")] () => nkʗ1[u]);
    var bʗ4 = b;
    @try("slice write uint", [GoFunc("main.func14")] () => {
        bʗ4[u] = 1;
        return 0;
    });
    var paʗ1 = pa;
    @try("pointer to array uint", [GoFunc("main.func15")] () => paʗ1.Value[u]);

    @try("string in range", [GoFunc("main.func16")] () => s[in32]);
    var bʗ5 = b;
    @try("slice in range", [GoFunc("main.func17")] () => bʗ5[in64]);
    @try("array in range", [GoFunc("main.func18")] () => Ꮡa.Value[in32]);
    var nbʗ2 = nb;
    @try("named slice in range", [GoFunc("main.func19")] () => nbʗ2[in64]);
    @try("named string in range", [GoFunc("main.func20")] () => np[in64]);
    var nkʗ2 = nk;
    @try("named array in range", [GoFunc("main.func21")] () => nkʗ2[in64]);
    var bʗ6 = b;
    @try("slice byte index", [GoFunc("main.func22")] () => bʗ6[b8]);
}

} // end main_package

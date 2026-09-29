namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[]nint")] partial struct named;

[GoType("@string")] partial struct namedStr;

[GoType("[3]nint")] partial struct namedArr;

internal static any sink;

[GoType("dyn")] internal partial interface try_type {
    void RuntimeError();
}

internal static void @try(@string name, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover();
            var (_, isErr) = r._<try_type>(ᐧ);
            fmt.Printf("%-24s %v | runtime.Error=%v\n"u8, name, r, isErr);
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string abcˢ = "abc"u8;
private static readonly object lenˢ = (@string)"len"u8;
private static readonly object capˢ = (@string)"cap"u8;
private static readonly object doneˢ = (@string)"done"u8;

[GoType("dyn")] internal partial struct main_cases {
    internal @string name;
    internal Action f;
}

internal static void Main() {
    var s = new slice<nint>(3, 10);
    ref var a = ref heap(new array<nint>(3), out var Ꮡa);
    var p = Ꮡa;
    var n = ((named)s);
    ref var na = ref heap(new namedArr(), out var Ꮡna);
    @string str = abcˢ;
    namedStr ns = ((namedStr)(@string)abcˢ);
    nint neg = -1;
    nint big = unchecked((nint)(4294967301L));
    nint lo4 = 4;
    nint hi2 = 2;
    nint hi5 = 5;
    nint hi11 = 11;
    uint64 u64 = (uint64)(1099511627776L + 3);
    uint32 u32 = 11;
    int64 i64 = 4294967301L;
    ref var a6 = ref heap(new array<nint>(6), out var Ꮡa6);
    nint two = 2;
    nint five = 5;
        var sʗ1 = s;

        var sʗ2 = s;

        var sʗ3 = s;

        var sʗ4 = s;

        var sʗ5 = s;

        var sʗ6 = s;

        var sʗ7 = s;

        var sʗ8 = s;

        var sʗ9 = s;

        var sʗ10 = s;

        var sʗ11 = s;

        var sʗ12 = s;

        var nʗ1 = n;

        var nʗ2 = n;





        var pʗ1 = p;

        var naʗ1 = na;










        var sʗ13 = s;

        var sʗ14 = s;

        var sʗ15 = s;

        var a6ʗ1 = a6;

        var a6ʗ2 = a6;

        var sʗ16 = s;
    var cases = new main_cases[]{
        new("slice [:hi>cap]"u8, () => {
            sink = sʗ1[..(int)(hi11)];
        }),
        new("slice [:len<hi<=cap]"u8, () => {
            sink = len(sʗ2[..(int)(hi5)]);
        }),
        new("slice [:-1]"u8, () => {
            sink = sʗ3[..(int)(neg)];
        }),
        new("slice [-1:]"u8, () => {
            sink = sʗ4[(int)(neg)..];
        }),
        new("slice [-1:2]"u8, () => {
            sink = sʗ5[(int)(neg)..(int)(hi2)];
        }),
        new("slice [4:2]"u8, () => {
            sink = sʗ6[(int)(lo4)..(int)(hi2)];
        }),
        new("slice [4:]"u8, () => {
            sink = sʗ7[(int)(lo4)..];
        }),
        new("slice [:big]"u8, () => {
            sink = sʗ8[..(int)(big)];
        }),
        new("slice [big:]"u8, () => {
            sink = sʗ9[(int)(big)..];
        }),
        new("slice [:i64]"u8, () => {
            sink = sʗ10[..(int)(i64)];
        }),
        new("slice [:u64]"u8, () => {
            sink = sʗ11[..(int)(u64)];
        }),
        new("slice [:u32]"u8, () => {
            sink = sʗ12[..(int)(u32)];
        }),
        new("named [:-1]"u8, () => {
            sink = nʗ1[..(int)(neg)];
        }),
        new("named [4:2]"u8, () => {
            sink = nʗ2[(int)(lo4)..(int)(hi2)];
        }),
        new("array [:5]"u8, () => {
            sink = Ꮡa.Value[..(int)(hi5)];
        }),
        new("array [:-1]"u8, () => {
            sink = Ꮡa.Value[..(int)(neg)];
        }),
        new("array [4:2]"u8, () => {
            sink = Ꮡa.Value[(int)(lo4)..(int)(hi2)];
        }),
        new("array [4:]"u8, () => {
            sink = Ꮡa.Value[(int)(lo4)..];
        }),
        new("ptrarray [:5]"u8, () => {
            sink = (~pʗ1)[..(int)(hi5)];
        }),
        new("namedarr [:5]"u8, () => {
            sink = naʗ1[..(int)(hi5)];
        }),
        new("string [:5]"u8, () => {
            sink = str[..(int)(hi5)];
        }),
        new("string [:-1]"u8, () => {
            sink = str[..(int)(neg)];
        }),
        new("string [-1:]"u8, () => {
            sink = str[(int)(neg)..];
        }),
        new("string [4:2]"u8, () => {
            sink = str[(int)(lo4)..(int)(hi2)];
        }),
        new("string [4:]"u8, () => {
            sink = str[(int)(lo4)..];
        }),
        new("string [:big]"u8, () => {
            sink = str[..(int)(big)];
        }),
        new("namedstr [:5]"u8, () => {
            sink = ns[..(int)(hi5)];
        }),
        new("strlit [:5]"u8, () => {
            @string t = "abc"u8[..(int)(hi5)];
            sink = t;
        }),
        new("strlit [-1:]"u8, () => {
            @string t = "abc"u8[(int)(neg)..];
            sink = t;
        }),
        new("slice3 [-1:2:5]"u8, () => {
            var t = sʗ13.slice(neg, two, five);
            sink = t;
            fmt.Println(lenˢ, len(t), capˢ, cap(t));
        }),
        new("slice3 [0:-1:5]"u8, () => {
            var t = sʗ14.slice(0, neg, five);
            sink = t;
            fmt.Println(lenˢ, len(t), capˢ, cap(t));
        }),
        new("slice3 [0:2:-1]"u8, () => {
            var t = sʗ15.slice(0, two, neg);
            sink = t;
            fmt.Println(lenˢ, len(t), capˢ, cap(t));
        }),
        new("array3 [-1:2:5]"u8, () => {
            var t = a6ʗ1.slice(neg, two, five);
            sink = t;
            fmt.Println(lenˢ, len(t), capˢ, cap(t));
        }),
        new("array3 [0:2:-1]"u8, () => {
            var t = a6ʗ2.slice(0, two, neg);
            sink = t;
            fmt.Println(lenˢ, len(t), capˢ, cap(t));
        }),
        new("slice3 [:2:5] in range"u8, () => {
            var t = sʗ16.slice(-1, two, five);
            sink = t;
            fmt.Println(lenˢ, len(t), capˢ, cap(t));
        })
    }.slice();
    foreach (var (_, c) in cases) {
        @try(c.name, c.f);
    }
    fmt.Println(doneˢ);
}

} // end main_package

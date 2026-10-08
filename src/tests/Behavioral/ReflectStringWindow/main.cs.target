namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

public partial struct named /*@string*/;

partial struct box {
    public named S;
    public slice<byte> B;
}

internal static void show(@string label, reflectꓸValue v) {
    fmt.Printf("%s: kind=%v type=%v len=%d val=%v\n"u8, label, v.Kind(), v.Type(), v.Len(), v.Interface());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object hLloˢ = (@string)"héllo"u8;
private static readonly @string namedWholeˢ = "named whole"u8;
private static readonly @string named25ˢ = "named [2:5]"u8;
private static readonly @string named0ˢ = "named [:0]"u8;
private static readonly @string named66ˢ = "named [6:6]"u8;
private static readonly object abcdefˢ = (@string)"abcdef"u8;
private static readonly @string plain14ˢ = "plain [1:4]"u8;
private static readonly object panicsˢ = (@string)"panics:"u8;

internal static void Main() {
    ref var s = ref heap<reflectꓸValue>(out var Ꮡs);
    s = reflect.ValueOf(hLloˢ);
    for (nint i = 0; i < s.Len(); i++) {
        var e = s.Index(i);
        fmt.Printf("index %d: kind=%v type=%v val=%v canAddr=%v canSet=%v\n"u8,
            i, e.Kind(), e.Type(), e.Interface(), e.CanAddr(), e.CanSet());
    }
    var b = new box(S: "abcdef"u8, B: slice<byte>("wxyz"u8));
    var rb = reflect.ValueOf(b);
    var sv = rb.Field(0);
    show(namedWholeˢ, sv);
    show(named25ˢ, sv.Slice(2, 5));
    show(named0ˢ, sv.Slice(0, 0));
    show(named66ˢ, sv.Slice(6, 6));
    ref var plain = ref heap<reflectꓸValue>(out var Ꮡplain);
    plain = reflect.ValueOf(abcdefˢ);
    show(plain14ˢ, plain.Slice(1, 4));
    var full = new nint[]{0, 1, 2, 3, 4, 5, 6, 7}.slice();
    ref var rf = ref heap<reflectꓸValue>(out var Ꮡrf);
    rf = reflect.ValueOf(full);
    var t3 = rf.Slice3(2, 5, 6);
    fmt.Printf("slice3 slice: len=%d cap=%d val=%v\n"u8, t3.Len(), t3.Cap(), t3.Interface());
    t3.Index(0).SetInt(99);
    fmt.Printf("slice3 aliases parent: %v\n"u8, full);
    ref var arr = ref heap<array<nint>>(out var Ꮡarr);
    arr = new nint[]{10, 11, 12, 13, 14, 15}.array();
    var ra = reflect.ValueOf(Ꮡarr).Elem();
    var a3 = ra.Slice3(1, 3, 5);
    fmt.Printf("slice3 array: len=%d cap=%d val=%v\n"u8, a3.Len(), a3.Cap(), a3.Interface());
    var bs = rb.Field(1);
    fmt.Printf("bytes [1:3]: %v\n"u8, bs.Slice(1, 3).Interface());
    var sʗ1 = s;

    var plainʗ1 = plain;


    var plainʗ2 = plain;

        var rfʗ1 = rf;
    fmt.Println(panicsˢ, probe(() => {
        sʗ1.Index(99);
    }), probe(() => {
        plainʗ1.Slice(4, 2);
    }),
        probe(() => {
            reflect.ValueOf((nint)(42)).Index(0);
        }), probe(() => {
        plainʗ2.Slice3(0, 1, 2);
    }),
        probe(() => {
            rfʗ1.Slice3(0, 2, 99);
        }));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noPanicˢ = "no panic"u8;

internal static @string /*msg*/ probe(Action f) {
    @string msg = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    msg = fmt.Sprint(r);
                }
            }
        }, ref ᒐ);
        f();
        msg = noPanicˢ;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

} // end main_package

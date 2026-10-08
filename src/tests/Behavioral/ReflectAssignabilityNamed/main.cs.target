namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct S /*@string*/;

partial struct B /*[]byte*/;

internal static void @try(@string label, Action set) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Printf("%s: PANIC %v\n"u8, label, r);
                    return;
                }
            }
            fmt.Printf("%s: ok\n"u8, label);
        }, ref ᒐ);
        set();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sStringMustPanicˢ = "S->string (must PANIC)"u8;
private static readonly @string byteBMustBeOkˢ = "[]byte->B (must be ok)"u8;
private static readonly @string bByteMustBeOkˢ = "B->[]byte (must be ok)"u8;
private static readonly @string stringSMustPanicˢ = "string->S (must PANIC)"u8;
private static readonly object sStringˢ = (@string)"S->string:"u8;
private static readonly object stringSˢ = (@string)"| string->S:"u8;
private static readonly object byteBˢ = (@string)"[]byte->B:"u8;
private static readonly object bByteˢ = (@string)"| B->[]byte:"u8;

internal static void Main() {
    ref var dstString = ref heap(new @string(), out var ᏑdstString);
    @try(sStringMustPanicˢ, () => {
        reflect.ValueOf(ᏑdstString).Elem().Set(reflect.ValueOf(((S)(@string)"x"u8)));
    });
    ref var dstB = ref heap<B>(out var ᏑdstB);
    @try(byteBMustBeOkˢ, () => {
        reflect.ValueOf(ᏑdstB).Elem().Set(reflect.ValueOf(slice<byte>("y"u8)));
    });
    ref var dstBytes = ref heap<slice<byte>>(out var ᏑdstBytes);
    @try(bByteMustBeOkˢ, () => {
        reflect.ValueOf(ᏑdstBytes).Elem().Set(reflect.ValueOf(((B)slice<byte>((@string)"z"u8))));
    });
    ref var dstS = ref heap(new S(), out var ᏑdstS);
    @try(stringSMustPanicˢ, () => {
        reflect.ValueOf(ᏑdstS).Elem().Set(reflect.ValueOf((@string)"w"u8));
    });
    var st = reflect.TypeOf((@string)""u8);
    var St = reflect.TypeOf(((S)(@string)""u8));
    var bt = reflect.TypeOf(slice<byte>(default!));
    var Bt = reflect.TypeOf(((B)default!));
    fmt.Println(sStringˢ, St.AssignableTo(st), stringSˢ, st.AssignableTo(St));
    fmt.Println(byteBˢ, bt.AssignableTo(Bt), bByteˢ, Bt.AssignableTo(bt));
}

} // end main_package

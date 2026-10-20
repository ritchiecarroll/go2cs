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

internal static void Main() {
    ref var dstString = ref heap(new @string(), out var ᏑdstString);
    @try("S->string (must PANIC)", () => {
        reflect.ValueOf(ᏑdstString).Elem().Set(reflect.ValueOf(((S)(@string)"x"u8)));
    });

    ref var dstB = ref heap<B>(out var ᏑdstB);
    @try("[]byte->B (must be ok)", () => {
        reflect.ValueOf(ᏑdstB).Elem().Set(reflect.ValueOf(slice<byte>("y"u8)));
    });

    ref var dstBytes = ref heap<slice<byte>>(out var ᏑdstBytes);
    @try("B->[]byte (must be ok)", () => {
        reflect.ValueOf(ᏑdstBytes).Elem().Set(reflect.ValueOf(((B)slice<byte>((@string)"z"u8))));
    });

    ref var dstS = ref heap(new S(), out var ᏑdstS);
    @try("string->S (must PANIC)", () => {
        reflect.ValueOf(ᏑdstS).Elem().Set(reflect.ValueOf("w"));
    });

    var st = reflect.TypeOf("");
    var St = reflect.TypeOf(((S)(@string)""u8));
    var bt = reflect.TypeOf(slice<byte>(default!));
    var Bt = reflect.TypeOf(((B)default!));
    fmt.Println("S->string:", St.AssignableTo(st), "| string->S:", st.AssignableTo(St));
    fmt.Println("[]byte->B:", bt.AssignableTo(Bt), "| B->[]byte:", Bt.AssignableTo(bt));
}

} // end main_package

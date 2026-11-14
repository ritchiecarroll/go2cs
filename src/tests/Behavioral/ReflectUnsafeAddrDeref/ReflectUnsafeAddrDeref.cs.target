namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct opcode /*num:nint*/;

partial struct code {
    internal any v;
    internal opcode op;
}

partial struct codeInt {
    internal any v;
    internal nint op;
}

partial struct plainNamed {
    internal nint a;
    internal opcode op;
}

partial struct plain {
    internal nint a;
    internal nint b;
}

partial struct bits {
    internal nint n;
    internal float64 f;
}

internal static void @try(@string name, Func<any> f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover(); if (r != nil) {
                fmt.Println(name, "PANIC", r);
            }
        }, ref ᒐ);
        fmt.Println(name, f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void main() {
    ref var p = ref heap<plain>(new plain(4, 5), out var Ꮡp);
    @try("plain int as *int", () => ~@unsafe.Pointer.To<nint>((@unsafe.Pointer)reflect.ValueOf(Ꮡp).Elem().Field(1).UnsafeAddr()));
    ref var pn = ref heap<plainNamed>(new plainNamed(4, 6), out var Ꮡpn);
    @try("plain opcode as *int", () => ~@unsafe.Pointer.To<nint>((@unsafe.Pointer)reflect.ValueOf(Ꮡpn).Elem().Field(1).UnsafeAddr()));
    @try("plain opcode as *opcode", () => (nint)(~@unsafe.Pointer.To<opcode>((@unsafe.Pointer)reflect.ValueOf(Ꮡpn).Elem().Field(1).UnsafeAddr())));
    var ci = Ꮡ(new codeInt(v: (nint)7, op: 3));
    var ciʗ1 = ci;
    @try("refbearing int as *int", () => ~@unsafe.Pointer.To<nint>((@unsafe.Pointer)reflect.ValueOf(ciʗ1.OrTypedNil()).Elem().Field(1).UnsafeAddr()));
    var c = Ꮡ(new code(v: (nint)7, op: 3));
    var cʗ1 = c;
    @try("refbearing opcode as *int", () => ~@unsafe.Pointer.To<nint>((@unsafe.Pointer)reflect.ValueOf(cʗ1.OrTypedNil()).Elem().Field(1).UnsafeAddr()));
    var cʗ2 = c;
    @try("refbearing opcode as *opcode", () => (nint)(~@unsafe.Pointer.To<opcode>((@unsafe.Pointer)reflect.ValueOf(cʗ2.OrTypedNil()).Elem().Field(1).UnsafeAddr())));
    var codes = slice<ж<code>>([Ꮡ(new code(v: (nint)7, op: 3)), Ꮡ(new code(v: (@string)"s", op: 11))]);
    var codesʗ1 = codes;
    @try("slice elem opcode as *int", () => ~@unsafe.Pointer.To<nint>((@unsafe.Pointer)reflect.ValueOf(codesʗ1).Index(1).Elem().FieldByName("op").UnsafeAddr()));
    var cʗ3 = c;
    @try("write opcode via *int", () => {
        (@unsafe.Pointer.To<nint>((@unsafe.Pointer)reflect.ValueOf(cʗ3.OrTypedNil()).Elem().Field(1).UnsafeAddr())).Value = 42;
        return (nint)cʗ3.op;
    });
    var cʗ4 = c;
    @try("interface field as *any", () => ~@unsafe.Pointer.To<any>((@unsafe.Pointer)reflect.ValueOf(cʗ4.OrTypedNil()).Elem().Field(0).UnsafeAddr()));
    ref var bs = ref heap<bits>(new bits(-3, 1.5D), out var Ꮡbs);
    ref var bv = ref heap<reflectꓸValue>(reflect.ValueOf(Ꮡbs).Elem(), out var Ꮡbv);
    var bvʗ1 = bv;
    @try("int as *uint", () => ~@unsafe.Pointer.To<nuint>((@unsafe.Pointer)bvʗ1.Field(0).UnsafeAddr()));
    var bvʗ2 = bv;
    @try("float64 as *uint64", () => ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)bvʗ2.Field(1).UnsafeAddr()));
}

} // end main_package

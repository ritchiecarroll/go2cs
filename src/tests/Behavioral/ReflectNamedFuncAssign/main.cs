namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

public delegate nint AssignA();

public delegate nint Other();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string otherˢ = "other"u8;

public static @string Name(this Other o) {
    return otherˢ;
}

public static bool Equal(this AssignA x, Func<nint> y) {
    return x() == y();
}

internal static nint apply(Func<nint> f) {
    return f() * 10;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string equalˢ = "Equal"u8;
private static readonly object assignableˢ = (@string)"assignable:"u8;
private static readonly object equalViaCallˢ = (@string)"Equal via Call:"u8;
private static readonly object applyViaCallˢ = (@string)"apply via Call:"u8;
private static readonly object namedIntoUnnamedˢ = (@string)"named into unnamed:"u8;
private static readonly object unnamedIntoNamedˢ = (@string)"unnamed into named:"u8;
private static readonly object namedIntoOtherNamedˢ = (@string)"named into other named:"u8;

internal static void Main() {
    var a = new AssignA(() => 7);
    var (m, _) = reflect.TypeOf((a).OrTypedNilFunc()).MethodByName(equalˢ);
    fmt.Println(assignableˢ, m.Type.In(0).AssignableTo(m.Type.In(1)));
    fmt.Println(equalViaCallˢ, m.Func.Call(new reflectꓸValue[]{reflect.ValueOf((a).OrTypedNilFunc()), reflect.ValueOf((a).OrTypedNilFunc())}.slice())[0].Bool());
    fmt.Println(applyViaCallˢ, reflect.ValueOf(apply).Call(new reflectꓸValue[]{reflect.ValueOf((a).OrTypedNilFunc())}.slice())[0].Int());
    ref var f = ref heap<Func<nint>>(out var Ꮡf);
    reflect.ValueOf(Ꮡf).Elem().Set(reflect.ValueOf((a).OrTypedNilFunc()));
    fmt.Println(namedIntoUnnamedˢ, f());
    ref var b = ref heap<AssignA>(out var Ꮡb);
    reflect.ValueOf(Ꮡb).Elem().Set(reflect.ValueOf(nint () => 9));
    fmt.Println(unnamedIntoNamedˢ, b());
    var aʗ1 = a;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println(namedIntoOtherNamedˢ, recover());
            }, ref ᒐ);
            ref var o = ref heap<Other>(out var Ꮡo);
            reflect.ValueOf(Ꮡo).Elem().Set(reflect.ValueOf((aʗ1).OrTypedNilFunc()));
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
}

} // end main_package

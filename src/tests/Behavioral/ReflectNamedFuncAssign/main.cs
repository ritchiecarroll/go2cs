namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

public delegate nint AssignA();

public delegate nint Other();

public static @string Name(this Other o) {
    return "other";
}

public static bool Equal(this AssignA x, Func<nint> y) {
    return x() == y();
}

internal static nint apply(Func<nint> f) {
    return f() * 10;
}

internal static void Main() {
    var a = new AssignA(() => 7);

    var (m, _) = reflect.TypeOf((a).OrTypedNilFunc()).MethodByName("Equal");
    fmt.Println("assignable:", m.Type.In(0).AssignableTo(m.Type.In(1)));
    fmt.Println("Equal via Call:", m.Func.Call(new reflectꓸValue[]{reflect.ValueOf((a).OrTypedNilFunc()), reflect.ValueOf((a).OrTypedNilFunc())}.slice())[0].Bool());

    fmt.Println("apply via Call:", reflect.ValueOf(apply).Call(new reflectꓸValue[]{reflect.ValueOf((a).OrTypedNilFunc())}.slice())[0].Int());

    ref var f = ref heap<Func<nint>>(out var Ꮡf);
    reflect.ValueOf(Ꮡf).Elem().Set(reflect.ValueOf((a).OrTypedNilFunc()));
    fmt.Println("named into unnamed:", f());

    ref var b = ref heap<AssignA>(out var Ꮡb);
    reflect.ValueOf(Ꮡb).Elem().Set(reflect.ValueOf(nint () => 9));
    fmt.Println("unnamed into named:", b());

    var aʗ1 = a;
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println("named into other named:", recover());
            }, ref ᒐ);
            ref var o = ref heap<Other>(out var Ꮡo);
            reflect.ValueOf(Ꮡo).Elem().Set(reflect.ValueOf((aʗ1).OrTypedNilFunc()));
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
}

} // end main_package

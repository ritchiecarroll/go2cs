namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;

partial class main_package {

partial struct T {
    internal nint n;
}

internal static nint valueMethod(this T t) { return t.n; }

internal static nint pointerMethod(this ref T t) { return t.n; }

internal static @string nameOf(any fn) {
    var f = runtime.FuncForPC(reflect.ValueOf(fn).Pointer());
    if (f == nil) {
        return "<nil Func>";
    }
    @string name = f.Name(); if (name != ""u8) {
        return name;
    }
    return "<empty name>";
}

internal static partial Func<nint> makeLiteral() {
    return [GoFunc("makeLiteral.func1")] () => 1;
}

internal static partial (Func<nint>, Func<nint>) twoLiterals() {
    var first = [GoFunc("twoLiterals.func1")] nint () => 1;
    var second = [GoFunc("twoLiterals.func2")] nint () => 2;
    return (first, second);
}

internal static partial @string boundInsideLiteral(ж<T> t) {
    return func([GoFunc("boundInsideLiteral.func1")] @string () => {
        return nameOf(t.pointerMethod);
    })();
}

internal static partial void main() {
    ref var t = ref heap<T>(new T(n: 1), out var Ꮡt);
    var p = Ꮡt;

    fmt.Println("method expression, value receiver:   ", nameOf(((Func<T, nint>)(main_package.valueMethod))));
    fmt.Println("method expression, pointer receiver: ", nameOf(((Func<ж<T>, nint>)(main_package.pointerMethod))));
    fmt.Println("method expression, promoted to *T:   ", nameOf(((Func<ж<T>, nint>)([GoWrapper("(*T).valueMethod")] (p0) => main_package.valueMethod(builtin.panicwrapRecv(p0, "value method main.T.valueMethod called using nil *T pointer").Value)))));

    fmt.Println("method value, pointer receiver:      ", nameOf(p.pointerMethod));
    fmt.Println("method value, &t pointer receiver:   ", nameOf((Ꮡt).pointerMethod));
    fmt.Println("method value, addressable t:         ", nameOf(Ꮡt.pointerMethod));
    fmt.Println("method value inside a literal:       ", boundInsideLiteral(p));

    fmt.Println("literal returned by a function:      ", nameOf((makeLiteral()).OrTypedNilFunc()));
    var (first, second) = twoLiterals();
    fmt.Println("first literal:                       ", nameOf((first).OrTypedNilFunc()));
    fmt.Println("second literal:                      ", nameOf((second).OrTypedNilFunc()));

    fmt.Println("package function:                    ", nameOf(nameOf));

    var inMain = [GoFunc("main.func1")] nint () => 3;
    fmt.Println("literal inside main:                 ", nameOf((inMain).OrTypedNilFunc()));

    var (pc, _, _, _) = runtime.Caller(0);
    fmt.Println("main's own frame:                    ", runtime.FuncForPC(pc).Name());

    fmt.Stringer s = ((named)1);
    fmt.Println("interface method value:              ", nameOf(s.String));
}

partial struct named /*num:nint*/;

internal static @string String(this named n) { return "named"; }

} // end main_package

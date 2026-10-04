namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoType] partial struct T {
    internal nint n;
}

internal static nint valueMethod(this T t) {
    return t.n;
}

[GoRecv] internal static nint pointerMethod(this ref T t) {
    return t.n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nilFuncˢ = "<nil Func>"u8;
private static readonly @string emptyNameˢ = "<empty name>"u8;

internal static @string nameOf(any fn) {
    var f = runtime.FuncForPC(reflect.ValueOf(fn).Pointer());
    if (f == nil) {
        return nilFuncˢ;
    }
    {
        @string name = f.Name(); if (name != ""u8) {
            return name;
        }
    }
    return emptyNameˢ;
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static Func<nint> makeLiteral() {
    return () => 1;
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static (Func<nint>, Func<nint>) twoLiterals() {
    var first = nint () => 1;
    var second = nint () => 2;
    return (first, second);
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static @string boundInsideLiteral(ж<T> Ꮡt) {
    return ((Func<@string>)(() => {
        return nameOf(Ꮡt.pointerMethod);
    }))();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object methodExpressionValueˢ = (@string)"method expression, value receiver:   "u8;
private static readonly object methodExpressionPointerˢ = (@string)"method expression, pointer receiver: "u8;
private static readonly object methodExpressionPromotedˢ = (@string)"method expression, promoted to *T:   "u8;
private static readonly object methodValuePointerˢ = (@string)"method value, pointer receiver:      "u8;
private static readonly object methodValueTPointerˢ = (@string)"method value, &t pointer receiver:   "u8;
private static readonly object methodValueAddressableTˢ = (@string)"method value, addressable t:         "u8;
private static readonly object methodValueInsideAˢ = (@string)"method value inside a literal:       "u8;
private static readonly object literalReturnedByAˢ = (@string)"literal returned by a function:      "u8;
private static readonly object firstLiteralˢ = (@string)"first literal:                       "u8;
private static readonly object secondLiteralˢ = (@string)"second literal:                      "u8;
private static readonly object packageFunctionˢ = (@string)"package function:                    "u8;
private static readonly object literalInsideMainˢ = (@string)"literal inside main:                 "u8;
private static readonly object mainSOwnFrameˢ = (@string)"main's own frame:                    "u8;
private static readonly object interfaceMethodValueˢ = (@string)"interface method value:              "u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static void Main() {
    ref var t = ref heap<T>(out var Ꮡt);
    t = new T(n: 1);
    var p = Ꮡt;
    fmt.Println(methodExpressionValueˢ, nameOf(((Func<T, nint>)(valueMethod))));
    fmt.Println(methodExpressionPointerˢ, nameOf(((Func<ж<T>, nint>)(pointerMethod))));
    fmt.Println(methodExpressionPromotedˢ, nameOf(((Func<ж<T>, nint>)([GoWrapper("(*T).valueMethod")] (p0) => valueMethod(panicwrapRecv(p0, "value method main.T.valueMethod called using nil *T pointer").Value)))));
    fmt.Println(methodValuePointerˢ, nameOf(p.pointerMethod));
    fmt.Println(methodValueTPointerˢ, nameOf((Ꮡt).pointerMethod));
    fmt.Println(methodValueAddressableTˢ, nameOf(Ꮡt.pointerMethod));
    fmt.Println(methodValueInsideAˢ, boundInsideLiteral(p));
    fmt.Println(literalReturnedByAˢ, nameOf((makeLiteral()).OrTypedNilFunc()));
    var (first, second) = twoLiterals();
    fmt.Println(firstLiteralˢ, nameOf((first).OrTypedNilFunc()));
    fmt.Println(secondLiteralˢ, nameOf((second).OrTypedNilFunc()));
    fmt.Println(packageFunctionˢ, nameOf(nameOf));
    var inMain = nint () => 3;
    fmt.Println(literalInsideMainˢ, nameOf((inMain).OrTypedNilFunc()));
    var (pc, _, _, _) = runtime.Caller(0);
    fmt.Println(mainSOwnFrameˢ, runtime.FuncForPC(pc).Name());
    fmt.Stringer s = ((named)1);
    fmt.Println(interfaceMethodValueˢ, nameOf(s.String));
}

[GoType("num:nint")] partial struct named;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string namedˢ = "named"u8;

internal static @string String(this named n) {
    return namedˢ;
}

} // end main_package

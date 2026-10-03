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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object methodValueValueReceiverˢ = (@string)"method value, value receiver:        "u8;
private static readonly object methodValueViaPointerˢ = (@string)"method value, via pointer, value rcv:"u8;
private static readonly object literalCallingTheSameˢ = (@string)"literal calling the same method:     "u8;
private static readonly object boundReceiverIsACopyˢ = (@string)"bound receiver is a copy:            "u8;

internal static void Main() {
    ref var t = ref heap<T>(out var Ꮡt);
    t = new T(n: 1);
    var p = Ꮡt;
    var tʗ1 = t;
    fmt.Println(methodValueValueReceiverˢ, nameOf(() => tʗ1.valueMethod()));
    var recvʗ1 = ~p;
    fmt.Println(methodValueViaPointerˢ, nameOf(() => recvʗ1.valueMethod()));
    var literal = () => Ꮡt.Value.valueMethod();
    fmt.Println(literalCallingTheSameˢ, nameOf((literal).OrTypedNilFunc()));
    var tʗ2 = t;
    var bound = () => tʗ2.valueMethod();
    t.n = 2;
    fmt.Println(boundReceiverIsACopyˢ, bound());
    var tʗ3 = t;
    call(() => tʗ3.walk());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object frameˢ = (@string)"frame:                               "u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static nint walk(this T t) {
    var pc = new slice<uintptr>(8);
    var frames = runtime.CallersFrames(pc.slice(0, runtime.Callers(1, pc)));
    while (ᐧ) {
        var (f, more) = frames.Next();
        fmt.Println(frameˢ, f.Function);
        if (!more || f.Function == "main.main"u8) {
            return t.n;
        }
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static nint call(Func<nint> f) {
    return f();
}

} // end main_package

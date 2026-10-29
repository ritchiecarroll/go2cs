namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct T {
    internal nint n;
}

internal static nint valueMethod(this T t) {
    return t.n;
}

internal static @string nameOf(any fn) {
    var f = runtime.FuncForPC(reflect.ValueOf(fn).Pointer());
    if (f == nil) {
        return "<nil Func>";
    }
    {
        @string name = f.Name(); if (name != ""u8) {
            return name;
        }
    }
    return "<empty name>";
}

internal static void Main() {
    ref var t = ref heap<T>(new T(n: 1), out var Ꮡt);
    var p = Ꮡt;

    var tʗ1 = t;
    fmt.Println("method value, value receiver:        ", nameOf(() => tʗ1.valueMethod()));
    var recvʗ1 = ~p;
    fmt.Println("method value, via pointer, value rcv:", nameOf(() => recvʗ1.valueMethod()));

    var literal = () => Ꮡt.Value.valueMethod();
    fmt.Println("literal calling the same method:     ", nameOf((literal).OrTypedNilFunc()));

    var tʗ2 = t;
    var bound = () => tʗ2.valueMethod();
    t.n = 2;
    fmt.Println("bound receiver is a copy:            ", bound());

    var tʗ3 = t;
    call(() => tʗ3.walk());
}

internal static partial nint walk(this T t) {
    var pc = new slice<uintptr>(8);
    var frames = runtime.CallersFrames(pc.slice(0, runtime.Callers(1, pc)));
    for (;;) {
        var (f, more) = frames.Next();
        fmt.Println("frame:                               ", f.Function);
        if (!more || f.Function == "main.main"u8) {
            return t.n;
        }
    }
}

internal static partial nint call(Func<nint> f) {
    return f();
}

} // end main_package

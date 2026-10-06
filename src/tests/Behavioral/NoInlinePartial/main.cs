namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noCallerˢ = "<no caller>"u8;

internal static partial @string here() {
    var (pc, _, _, ok) = runtime.Caller(1);
    if (!ok) {
        return noCallerˢ;
    }
    return runtime.FuncForPC(pc).Name();
}

internal static partial @string plain() {
    return here();
}

internal static partial @string variadic(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.sslice();

    return here();
}

internal static partial (@string name, nint n) pair() {
    var (pc, _, _, _) = runtime.Caller(0);
    return (runtime.FuncForPC(pc).Name(), 1);
}

internal static partial @string generic<T>(T x) {
    return here();
}

[GoType] partial struct counter {
    internal nint n;
}

[GoRecv] internal static partial @string ptr(this ref counter c) {
    return here();
}

internal static partial @string val(this counter c) {
    return here();
}

[GoType] partial struct box<T> {
    internal T v;
}

[GoRecv] internal static partial @string get<T>(this ref box<T> b) {
    return here();
}

internal static @string fromInit;

[MethodImpl(MethodImplOptions.NoInlining)] [GoInit] internal static void init() {
    var (pc, _, _, _) = runtime.Caller(0);
    fromInit = runtime.FuncForPC(pc).Name();
}

internal static partial void Main() {
    var c = Ꮡ(new counter(nil));
    var b = Ꮡ(new box<nint>(v: 1));
    var lit = [MethodImpl(MethodImplOptions.NoInlining)] @string () => here();
    @string call(Func<@string> f) => f();
    [MethodImpl(MethodImplOptions.NoInlining)] @string local() {
        var (pc, _, _, _) = runtime.Caller(0);
        return runtime.FuncForPC(pc).Name();
    }
    var (name, n) = pair();
    fmt.Println(plain());
    fmt.Println(variadic(1, 2));
    fmt.Println(name, n);
    fmt.Println(generic(1));
    fmt.Println(c.ptr());
    fmt.Println((~c).val());
    fmt.Println(b.get());
    fmt.Println(fromInit);
    fmt.Println(call(lit));
    fmt.Println(local());
}

} // end main_package

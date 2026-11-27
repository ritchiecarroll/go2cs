namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct T {
    internal nint n;
}

public static @string Hello(this T _) { return "hello"; }

public static @string Show(this T t, @string s) { return fmt.Sprint(s, " n=", t.n); }

public static nint N(this T t) { return t.n; }

internal static @string set(this ref T t, nint v) {
    t.n = v;
    return "set";
}

internal static @string mark(@string s) {
    fmt.Println("  evaluated:", s);
    return s;
}

public static @string Via(this ж<T> Ꮡt) { return "via "u8 + Ꮡt.Hello(); }

public static @string ViaArg(this ж<T> Ꮡt) { return Ꮡt.Show(mark("arg")); }

public static @string ViaWrite(this ж<T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    return Ꮡt.Show(t.set(10));
}

public static nint ViaN(this ж<T> Ꮡt) { return Ꮡt.N(); }

internal static void @try(@string name, Func<@string> f) {
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("try.func1")] () => {
            var r = recover(); if (r != nil) {
                fmt.Println(name, "panicked:", r);
            }
        }, ref ᒐ);
        var evalᴛ1 = f();
        fmt.Println(name, "returned:", evalᴛ1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void main() {
    ж<T> p = default;
    var pʗ1 = p;
    @try("nil, callee ignores its receiver", [GoFunc("main.func1")] () => pʗ1.Via());
    var pʗ2 = p;
    @try("nil, side-effecting argument", [GoFunc("main.func2")] () => pʗ2.ViaArg());
    var pʗ3 = p;
    @try("nil, callee reads a field", [GoFunc("main.func3")] () => fmt.Sprint(pʗ3.ViaN()));

    var q = Ꮡ(new T(n: 7));
    var qʗ1 = q;
    @try("non-nil, callee ignores its receiver", [GoFunc("main.func4")] () => qʗ1.Via());
    var qʗ2 = q;
    @try("non-nil, argument writes the pointee", [GoFunc("main.func5")] () => qʗ2.ViaWrite());
    var qʗ3 = q;
    @try("non-nil, callee reads a field", [GoFunc("main.func6")] () => fmt.Sprint(qʗ3.ViaN()));
}

} // end main_package

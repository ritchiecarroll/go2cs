namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using framelib = LiteralFrameNames.framelib_package;
using LiteralFrameNames;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct ring {
    internal nint n;
}

internal static partial @string init(this ж<ring> Ꮡr) {
    [GoFunc("(*ring).init.func1")] @string f() {
        Ꮡr.Value.n++;
        return here();
    }
    return f();
}

internal static Func<@string> fromVar = [MethodImpl(MethodImplOptions.NoInlining), GoFunc("init.func1")] () => here();

internal static slice<@string> initNames;

[GoInit] internal static void init() {
    [GoFunc("init.0.func1")] void f() {
        nint a = 1;
        nint b = a + 1;
        nint c = b + 1;
        nint d = c + 1;
        nint e = d + 1;
        if (e > 100) {
            fmt.Println("unreachable");
        }
        initNames = append(initNames, here());
    }
    f();
}

[GoInit] internal static void initΔ1() {
    [GoFunc("init.1.func1")] void g() {
        initNames = append(initNames, here());
    }
    g();
}

internal static partial @string outer() {
    @string name = default;
    call([MethodImpl(MethodImplOptions.NoInlining), GoFunc("outer.func1")] () => {
        call([GoFunc("outer.func1.1")] () => {
            name = here();
        });
    });
    return name;
}

internal static partial void call(Action f) { f(); }

internal static partial nint callLine() {
    nint line = default;
    func([MethodImpl(MethodImplOptions.NoInlining), GoFunc("callLine.func1")] () => {
        (_, _, line, _) = runtime.Caller(1);
    })();
    return line;
}

internal static partial @string here() {
    var (pc, _, _, _) = runtime.Caller(1);
    return runtime.FuncForPC(pc).Name();
}

internal static void main() {
    foreach (var (_, name) in range(initNames)) {
        fmt.Println("init literal:", name);
    }

    fmt.Println("method init literal:", (Ꮡ(new ring())).init());
    fmt.Println("var literal:", fromVar());
    fmt.Println("nested literal:", outer());
    fmt.Println("counter literal:", framelib.CounterName());
    fmt.Println("gauge literal:", framelib.GaugeName());
    fmt.Println("caller line of an invoked literal:", callLine());
}

} // end main_package

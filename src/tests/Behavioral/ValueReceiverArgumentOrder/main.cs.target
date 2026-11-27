namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct V {
    public nint X;
}

public static @string Show(this V v, nint n) { return fmt.Sprint(v.X, " ", n); }

public static nint Bump(this V v) {
    v.X++;
    return v.X;
}

partial struct Box<T> {
    public T Item;
}

public static @string Get<T>(this Box<T> b, @string tag) { return fmt.Sprint((b.Item).OrTypedNilOf(), " ", tag); }

internal static ж<V> shared = Ꮡ(new V(X: 1));

internal static ж<V> current() { return shared; }

internal static void main() {
    var p = Ꮡ(new V(X: 1));
    var pʗ1 = p;
    [GoFunc("main.func1")] nint set(nint x) {
        pʗ1.X = x;
        return x;
    }
    var evalᴛ1 = set(10);
    fmt.Println("argument writes through p:", p.Show(evalᴛ1));

    nint bumped = p.Bump();
    fmt.Println("method writes its copy:", bumped, p.X);

    var b = Ꮡ(new Box<@string>(Item: "old"));
    var bʗ1 = b;
    [GoFunc("main.func2")] @string retag() {
        bʗ1.Item = "new";
        return "tag";
    }
    var evalᴛ2 = retag();
    fmt.Println("generic receiver:", b.Get(evalᴛ2));

    fmt.Println("call result as the pointer:", current().Show(func([GoFunc("main.func3")] nint () => {
        shared.X = 5;
        return 6;
    })()));

    ж<V> q = default;
    var ran = false;
    [GoFunc("main.func4")] nint side() {
        ran = true;
        return 0;
    }
    var qʗ1 = q;
    var sideʗ1 = side;
    func([GoFunc("main.func5")] () => {
        GoFrame ᒐ = default;
        try {
            defer([GoFunc("main.func5.1")] () => {
                fmt.Println("nil pointer:", recover(), "| argument ran:", ran);
            }, ref ᒐ);
            var evalᴛ3 = sideʗ1();
            fmt.Println(qʗ1.Show(evalᴛ3));
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
}

} // end main_package

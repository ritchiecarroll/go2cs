namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

partial struct box {
    internal nint f;
}

internal static nint g;

internal static @string pair(nint a, nint b) { return fmt.Sprint(a, " ", b); }

internal static partial void main() {
    nint x = 1;
    [GoFunc("main.func1")] nint setX() {
        x = 10;
        return 2;
    }
    fmt.Println("local:", pair(x, setX()));

    g = 1;
    [GoFunc("main.func2")] nint setG() {
        g = 10;
        return 2;
    }
    fmt.Println("global:", pair(g, setG()));

    ref var s = ref heap<box>(new box(1), out var Ꮡs);
    [GoFunc("main.func3")] nint setF() {
        Ꮡs.Value.f = 10;
        return 2;
    }
    fmt.Println("field:", pair(s.f, setF()));

    ref var v = ref heap<nint>(1, out var Ꮡv);
    var p = Ꮡv;
    var pʗ1 = p;
    [GoFunc("main.func4")] nint setP() {
        pʗ1.Value = 10;
        return 2;
    }
    fmt.Println("deref:", pair(p.Value, setP()));

    var a = slice<nint>([1]);
    var aʗ1 = a;
    [GoFunc("main.func5")] nint setA() {
        aʗ1[0] = 10;
        return 2;
    }
    fmt.Println("element:", pair(a[0], setA()));

    var ptr = Ꮡ(new box(1));
    var ptrʗ1 = ptr;
    [GoFunc("main.func6")] nint setPtr() {
        ptrʗ1.f = 10;
        return 2;
    }
    fmt.Println("pointer field:", pair(ptr.f, setPtr()));

    nint y = 1;
    var ch = new channel<nint>(0);
    var chʗ1 = ch;
    goǃ([GoFunc("main.func7")] () => {
        time.Sleep(30 * time.Millisecond);
        y = 10;
        chʗ1.ᐸꟷ(2);
    });
    fmt.Println("receive:", pair(y, ᐸꟷ(ch)));
}

} // end main_package

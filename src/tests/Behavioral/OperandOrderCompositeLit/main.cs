namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct pt {
    public nint A, B;
}

internal static void main() {
    nint a = 1;
    [GoFunc("main.func1")] nint setA() {
        a = 10;
        return 2;
    }
    var evalᴛ1 = setA();
    fmt.Println("slice:", slice<nint>([a, evalᴛ1]));

    a = 1;
    var evalᴛ2 = setA();
    fmt.Println("array:", array<nint>([a, evalᴛ2]));

    a = 1;
    var evalᴛ3 = setA();
    fmt.Println("struct:", new pt(A: a, B: evalᴛ3));

    a = 1;
    var evalᴛ4 = setA();
    fmt.Println("nested:", slice<slice<nint>>([[a], slice<nint>([evalᴛ4])]));

    a = 1;
    fmt.Println("map (in place in Go):", new map<@string, nint>{["a"] = a, ["b"] = setA()});
}

} // end main_package

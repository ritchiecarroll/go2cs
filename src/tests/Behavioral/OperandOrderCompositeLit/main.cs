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
    fmt.Println("slice:", slice<nint>([a, setA()]));

    a = 1;
    fmt.Println("array:", array<nint>([a, setA()]));

    a = 1;
    fmt.Println("struct:", new pt(A: a, B: setA()));

    a = 1;
    fmt.Println("nested:", slice<slice<nint>>([[a], slice<nint>([setA()])]));

    a = 1;
    fmt.Println("map (in place in Go):", new map<@string, nint>{["a"] = a, ["b"] = setA()});
}

} // end main_package

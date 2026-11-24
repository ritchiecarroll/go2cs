namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void main() {
    var arr = slice<nint>([0, 0, 0]);
    nint i = 0;
    [GoFunc("main.func1")] nint setI() {
        i = 2;
        return 7;
    }
    arr[i] = setI();
    fmt.Println("left index:", arr);

    ref var v1 = ref heap<nint>(out var Ꮡv1);
    v1 = 0;
    ref var v2 = ref heap<nint>(out var Ꮡv2);
    v2 = 0;
    ref var p = ref heap<ж<nint>>(Ꮡv1, out var Ꮡp);
    [GoFunc("main.func2")] nint setP() {
        Ꮡp.ValueSlot = Ꮡv2;
        return 7;
    }
    p.Value = setP();
    fmt.Println("left dereference:", v1, v2);

    nint x = 1;
    [GoFunc("main.func3")] nint setX() {
        x = 10;
        return 2;
    }
    nint y = default;
    y = x + setX();
    fmt.Println("right operand:", y);

    var m = new map<nint, nint>{};
    nint k = 1;
    [GoFunc("main.func4")] nint setK() {
        k = 5;
        return 7;
    }
    m[k] = setK();
    fmt.Println("left map key:", m);
}

} // end main_package

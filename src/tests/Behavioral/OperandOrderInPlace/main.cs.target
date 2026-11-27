namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string pair(nint a, nint b) { return fmt.Sprint(a, " ", b); }

internal static void main() {
    ref var sl = ref heap<slice<nint>>(slice<nint>([1]), out var Ꮡsl);
    [GoFunc("main.func1")] nint grow() {
        Ꮡsl.ValueSlot = append(Ꮡsl.ValueSlot, (nint)5, (nint)6);
        return 2;
    }
    fmt.Println("len:", pair(len(sl), grow()));

    sl = new slice<nint>(1, 1);
    fmt.Println("cap:", pair(cap(sl), grow()));

    sl = [1, 2];
    [GoFunc("main.func2")] nint swap() {
        Ꮡsl.ValueSlot = [8, 9];
        return 2;
    }
    fmt.Println("slice to array:", new array<nint>(sl, 2), swap());

    nint a = 1;
    [GoFunc("main.func3")] nint setA() {
        a = 10;
        return 2;
    }
    fmt.Println("map literal:", new map<@string, nint>{["a"] = a, ["b"] = setA()});
}

} // end main_package

namespace go;

using fmt = fmt_package;
using Δmath = math_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void main() {
    if (runtime.GOARCH != "amd64"u8) {
        fmt.Println("amd64 only:", runtime.GOARCH, "passes no floating-point register");
        return;
    }

    var msvcrt = syscall.NewLazyDLL("msvcrt.dll");

    var s = slice<byte>("2.5\x00"u8);
    var (_, r2, _) = msvcrt.NewProc("strtod").Call((uintptr)Ꮡ(s, 0), 0);
    fmt.Println("1 strtod(\"2.5\") =", Δmath.Float64frombits((uint64)r2));

    (_, r2, _) = msvcrt.NewProc("sqrt").Call((uintptr)Δmath.Float64bits(2.25D));
    fmt.Println("2 sqrt(2.25) =", Δmath.Float64frombits((uint64)r2));

    (_, r2, _) = msvcrt.NewProc("pow").Call((uintptr)Δmath.Float64bits(2D), (uintptr)Δmath.Float64bits(10D));
    fmt.Println("3 pow(2, 10) =", Δmath.Float64frombits((uint64)r2));

    (_, r2, _) = msvcrt.NewProc("ldexp").Call((uintptr)Δmath.Float64bits(1.5D), 3);
    fmt.Println("4 ldexp(1.5, 3) =", Δmath.Float64frombits((uint64)r2));

    var minus7 = (int32)(-7);
    var (r1, _, _) = msvcrt.NewProc("labs").Call((uintptr)minus7);
    fmt.Println("5 labs(-7) =", (int32)r1);
}

} // end main_package

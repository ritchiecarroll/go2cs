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

    var buf = new slice<byte>(64);
    var format = slice<byte>("%.2f %.2f %.2f\x00"u8);
    (r1, _, _) = msvcrt.NewProc("_snprintf").Call((uintptr)Ꮡ(buf, 0), (uintptr)len(buf),
        (uintptr)Ꮡ(format, 0), (uintptr)Δmath.Float64bits(1.25D), (uintptr)Δmath.Float64bits(2.5D),
        (uintptr)Δmath.Float64bits(3.75D));
    fmt.Println("6 _snprintf, three doubles (1.25, 2.5, 3.75) =", (@string)(buf.slice(0, (int32)r1)));

    fmt.Println("7 _snprintf, 19 arguments =", snprintfInts(msvcrt, 16));
    fmt.Println("8 _snprintf, 42 arguments =", snprintfInts(msvcrt, 39));
    fmt.Println("9 43 arguments:", tooMany(msvcrt));

    var name = slice<uint16>(['n', 'o', '-', 's', 'u', 'c', 'h', '.', 'f', 'i', 'l', 'e', 0]);
    var (_, _, err) = syscall.NewLazyDLL("kernel32.dll").NewProc("GetFileAttributesW").Call((uintptr)Ꮡ(name, 0));
    fmt.Println("10 GetFileAttributesW(\"no-such.file\") err =", (uintptr)(err._<syscall.Errno>()));
}

internal static @string snprintfInts(ж<syscall.LazyDLL> msvcrt, nint n) {
    var buf = new slice<byte>(256);
    var format = slice<byte>([]);

    for (nint i = 0; i < n; i++) {
        format = append(format, ((@string)" %d").ꓸꓸꓸ);
    }

    format = append(format, (byte)0);
    var args = slice<uintptr>([(uintptr)Ꮡ(buf, 0), (uintptr)len(buf), (uintptr)Ꮡ(format, 0)]);

    for (nint i = 1; i <= n; i++) {
        args = append(args, (uintptr)i);
    }

    var (r1, _, _) = msvcrt.NewProc("_snprintf").Call(args.ꓸꓸꓸ);
    return (@string)(buf.slice(1, (int32)r1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object returnedˢ = (@string)"returned"u8;

internal static any /*text*/ tooMany(ж<syscall.LazyDLL> msvcrt) {
    any text = default;
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("tooMany.func1")] () => {
            text = recover();
        }, ref ᒐ);

        msvcrt.NewProc("labs").Call(new slice<uintptr>(43).ꓸꓸꓸ);
        text = returnedˢ;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return text;
}

} // end main_package

namespace go;

using fmt = fmt_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object iFromThreadAndˢ = (@string)"i from thread and"u8;
private static readonly object xFromThreadˢ = (@string)"x from thread"u8;

internal static partial void Main() {
    nint i = 0;

    while (i < 10) {
        f(ref i);
        i++;
    }

    fmt.Println();
    fmt.Println("i =", i);

    for (i = 0; i < 10; i++) {
        f(ref i);

        for (nint j = 0; j < 3; j++) {
            fmt.Println(i + j);
        }
        fmt.Println();
    }

    fmt.Println("i =", i);
    fmt.Println();

@out:
    for (nint iΔ1 = 0; iΔ1 < 5; iΔ1++) {
        f(ref iΔ1);

        for (nint iΔ2 = 12; iΔ2 < 15; iΔ2++) {
            f(ref iΔ2);
            goto break_out;
        }

        if (iΔ1 > 13) {
            goto continue_out;
        }

        fmt.Println();
continue_out:;
    }
break_out:;

    fmt.Println();
    fmt.Println("i =", i);
    fmt.Println();

    var nums = new nint[]{1, 2, 3, 4}.slice();
scan:
    foreach (var (_, n) in nums) {
        foreach (var (_, m) in nums) {
            if (n == m) {
                goto continue_scan;
            }
            if (n + m > 5) {
                goto break_scan;
            }
            fmt.Println("pair", n, m);
        }
continue_scan:;
    }
break_scan:;

    fmt.Println();

    nint x = 99;
    fmt.Println("i before thread and", i, "x before thread", x);
    goǃ((ᴛ1, ᴛ2, ᴛ3, ᴛ4) => fmt.Println(ᴛ1, ᴛ2, ᴛ3, ᴛ4), iFromThreadAndˢ, i, xFromThreadˢ, x);

    while (ᐧ) {
        i++;
        x++;
        f(ref i);

        if (i > 12) {
            break;
        }
    }

    fmt.Println();
    fmt.Println("i =", i);
    fmt.Println("x = ", x);

    time.Sleep(1);

    fmt.Println("i after thread and", i, "x after thread", x);
}

internal static void f(ref nint y) {
    fmt.Print(y);
}

} // end main_package

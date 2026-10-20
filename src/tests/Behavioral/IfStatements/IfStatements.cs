namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    {
        nint a = -1; if (a < 0) {
            fmt.Println("a is less than 0");
        }
    }

    {
        nint a = 1; if (a > 0) {
            fmt.Println("a is greater than 0");
        }
    }

    nint b = 99;
    fmt.Println("b before =", b);

    {
        nint bΔ1 = -1; if (bΔ1 < 0) {
            fmt.Println("b now =", bΔ1);
            fmt.Println("b is less than 0");
        }
    }

    {
        nint bΔ2 = 1; if (bΔ2 > 0) {
            fmt.Println("b now =", bΔ2);
            fmt.Println("b is greater than 0");
        }
    }

    fmt.Println("b after =", b);
}

} // end main_package

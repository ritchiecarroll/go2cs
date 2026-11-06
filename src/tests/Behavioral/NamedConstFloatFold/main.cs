namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedFloat myPi => 3.14159265358979323846264338327950288419716939937510582097494459;
internal static UntypedFloat myLn10 => 2.30258509299404568401799145468436420760110148862877297603332790;

internal static float64 f64(float64 x) { return x; }

internal static void Main() {
    fmt.Println(/* 100000 * myPi */ 314159.26535897935D);
    fmt.Println(/* float32(100000 * myPi) */ 314159.25D);
    fmt.Println(f64(2D) * /* (1 / myLn10) */ 0.4342944819032518D);
}

} // end main_package

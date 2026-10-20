namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

partial interface Abser {
    float64 Abs();
}

partial struct MyError {
    public time.Time When;
    public @string What;
}

partial struct MyCustomError {
    public @string Message;
    /*embed*/ public Abser Abser;
    /*embed*/ public MyError MyError;
    /*embed*/ internal error error;
}

partial struct MyAbser {
}

public static float64 Abs(this ref MyCustomError myErr) {
    return 0.0D;
}

public static float64 Abs(this MyAbser myAbs) {
    return 1.0D;
}

internal static void Main() {
    var a = new MyCustomError("New One", new MyAbser(nil), new MyError(time.Now(), "Hello"), default!);
    a.Abs();
    a.Message = "New";
    fmt.Println("MyCustomError method =", a.Abs());
}

} // end main_package

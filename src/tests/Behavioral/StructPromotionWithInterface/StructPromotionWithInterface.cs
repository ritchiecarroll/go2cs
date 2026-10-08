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
    public partial ref MyError MyError { get; }
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string newˢ = "New"u8;
private static readonly object myCustomErrorMethodˢ = (@string)"MyCustomError method ="u8;

internal static void Main() {
    var a = new MyCustomError("New One"u8, new MyAbser(nil), new MyError(time.Now(), "Hello"u8), default!);
    a.Abs();
    a.Message = newˢ;
    fmt.Println(myCustomErrorMethodˢ, a.Abs());
}

} // end main_package

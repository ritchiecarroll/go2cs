namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

[GoType] partial interface Abser {
    float64 Abs();
}

[GoType] partial struct MyError {
    public time.Time When;
    public @string What;
}

[GoType] partial struct MyCustomError {
    public @string Message;
    [GoEmbedded] public Abser Abser;
    public partial ref MyError MyError { get; }
    [GoEmbedded] internal error error;
}

[GoType] partial struct MyAbser {
}

[GoRecv] public static float64 Abs(this ref MyCustomError myErr) {
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

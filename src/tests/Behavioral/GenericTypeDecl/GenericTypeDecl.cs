namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Pair<T, U> {
    public T First;
    public U Second;
}

internal static void Main() {
    var p = new Pair<@string, nint>(
        First: "answer"u8,
        Second: 42
    );
    fmt.Printf("Pair: %v, %v\n"u8, p.First, p.Second);
}

} // end main_package

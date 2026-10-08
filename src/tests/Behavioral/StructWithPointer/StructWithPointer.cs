namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct ColorList {
    public nint Total;
    public @string Color;
    public ж<ColorList> Next;
    public ж<ж<ColorList>> NextNext;
}

internal static void Main() {
    var red = new ColorList(2, "red"u8, nil, nil);
    ref var blue = ref heap<ColorList>(out var Ꮡblue);
    blue = new ColorList(2, "blue"u8, nil, nil);
    red.Next = Ꮡblue;
    fmt.Printf("Value of red = %v\n"u8, red);
    fmt.Printf("Value of blue = %v\n"u8, blue);
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uint8 takeU8(uint8 x) {
    return x;
}

internal static int8 takeI8(int8 x) {
    return x;
}

internal static uint16 takeU16(uint16 x) {
    return x;
}

partial struct box {
    internal uint8 b;
}

partial struct pix {
    public uint8 Y;
    public uint8 A;
}

internal static void Main() {
    uint8 a = 200;
    uint8 b = 100;
    fmt.Println(takeU8((uint8)(a + b)));
    fmt.Println(takeU8((uint8)(((uint8)(~a)))));
    int8 c = 100;
    int8 d = 100;
    fmt.Println(takeI8((int8)(c + d)));
    uint16 e = 60000;
    uint16 f = 10000;
    fmt.Println(takeU16((uint16)(e + f)));
    fmt.Println(takeU8((uint8)(a * 2 + b)));
    var y = (uint8)(a + b);
    fmt.Println(y);
    y = (uint8)(y + 1);
    fmt.Println(y);
    array<uint8> arr = new(1);
    arr[0] = (uint8)(a + b);
    fmt.Println(arr[0]);
    box bx = default!;
    bx.b = (uint8)(a + b);
    fmt.Println(bx.b);
    uint8 z = (uint8)(a + b);
    fmt.Println(z);
    var p1 = new pix((uint8)(((a >> (int)(1))) * 3), (uint8)(a + b));
    fmt.Println(p1.Y, p1.A);
    var p2 = new pix(Y: (uint8)(((a >> (int)(1))) * 3), A: (uint8)(((uint8)(~a))));
    fmt.Println(p2.Y, p2.A);
    fmt.Println((uint8)(a + b) == 44);
    fmt.Println((int8)(c + d) == -56);
    fmt.Println((uint16)(e + f) != 4464);
    fmt.Println((uint8)(((uint8)(~a))) > 50);
}

} // end main_package
